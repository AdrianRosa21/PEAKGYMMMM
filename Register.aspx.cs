using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;  // <-- agrega este using


namespace PEAKGYMM
{
    public partial class Register : Page
    {
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            string email = txtEmail.Text.Trim();
            string pass = txtPassword.Text;
            string pass2 = txtConfirm.Text;

            if (pass != pass2)
            {
                AddError("Las contraseñas no coinciden.");
                return;
            }

            // Validaciones de servidor (fuerte)
            // 1) Email simple
            if (!System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                AddError("Correo inválido.");
                return;
            }

            // 2) Fecha nacimiento: no futuro y >=18
            DateTime birth;
            if (!DateTime.TryParseExact(txtBirthDate.Text, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out birth))
            {
                AddError("Fecha de nacimiento inválida.");
                return;
            }
            if (birth > DateTime.UtcNow.Date)
            {
                AddError("La fecha de nacimiento no puede ser futura.");
                return;
            }
            var eighteen = birth.AddYears(18);
            if (eighteen > DateTime.UtcNow.Date)
            {
                AddError("Debes ser mayor de 18 años.");
                return;
            }

            // 3) Género permitido
            string gender = (ddlGender.SelectedValue ?? "").Trim().ToUpperInvariant();
            if (!(gender == "" || gender == "M" || gender == "F" || gender == "O"))
            {
                AddError("Género inválido.");
                return;
            }

            // 4) Peso / Altura (opcionales) con rangos razonables
            decimal? weight = null, height = null;
            if (!string.IsNullOrWhiteSpace(txtWeight.Text))
            {
                decimal w;
                if (!decimal.TryParse(txtWeight.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out w)
                    || w < 25 || w > 350)
                { AddError("Peso fuera de rango (25 - 350 kg)."); return; }
                weight = Math.Round(w, 2);
            }
            if (!string.IsNullOrWhiteSpace(txtHeight.Text))
            {
                decimal h;
                if (!decimal.TryParse(txtHeight.Text.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out h)
                    || h < 50 || h > 250)
                { AddError("Altura fuera de rango (50 - 250 cm)."); return; }
                height = Math.Round(h, 2);
            }

            // 5) Teléfono (opcional) simple
            string phone = txtPhone.Text.Trim();
            if (!string.IsNullOrEmpty(phone) &&
                !System.Text.RegularExpressions.Regex.IsMatch(phone, @"^[\d+\-\s()]{7,20}$"))
            {
                AddError("Teléfono inválido.");
                return;
            }

            // 6) Imagen: validar extensión y tamaño (máx 3MB)
            string filePath = null;
            if (fuProfileImage.HasFile)
            {
                var ext = System.IO.Path.GetExtension(fuProfileImage.FileName).ToLowerInvariant();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                { AddError("Formato de imagen no permitido (jpg, png, webp)."); return; }
                if (fuProfileImage.PostedFile.ContentLength > 3 * 1024 * 1024)
                { AddError("La imagen no debe superar 3 MB."); return; }
            }

            try
            {
                // 1) ¿correo ya existe?
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand("SELECT 1 FROM dbo.[User] WHERE Email=@e", cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cn.Open();
                    if (cmd.ExecuteScalar() != null)
                    {
                        AddError("Ese correo ya está registrado.");
                        return;
                    }
                }

                // 2) salt + hash
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(salt); }
                byte[] hash = Hash(pass, salt);

                // 3) insertar
                const string sqlIns = @"
INSERT INTO dbo.[User]
(Email, PasswordHash, PasswordSalt, RoleId, IsActive, CreatedAt,
 FullName, BirthDate, Gender, HeightCm, WeightKg, Phone, ProfileImagePath)
VALUES
(@e, @h, @s,
 (SELECT TOP 1 RoleId FROM dbo.Role WHERE Name='Usuario'),
 1, SYSUTCDATETIME(),
 @name, @birth, @gender, @height, @weight, @phone, @img);";

                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(sqlIns, cn))
                {
                    // ---------------- Credenciales ----------------
                    cmd.Parameters.Add("@e", SqlDbType.VarChar, 120).Value = email;

                    var pHash = cmd.Parameters.Add("@h", SqlDbType.VarBinary, hash.Length);
                    pHash.Value = hash;

                    var pSalt = cmd.Parameters.Add("@s", SqlDbType.VarBinary, salt.Length);
                    pSalt.Value = salt;

                    // ---------------- Perfil ----------------
                    cmd.Parameters.Add("@name", SqlDbType.VarChar, 120).Value =
                        string.IsNullOrWhiteSpace(txtFullName.Text) ? (object)DBNull.Value : txtFullName.Text.Trim();

                    cmd.Parameters.Add("@birth", SqlDbType.Date).Value = birth;

                    // @gender: una sola vez
                    var pGender = cmd.Parameters.Add("@gender", SqlDbType.VarChar, 10);
                    pGender.Value = string.IsNullOrEmpty(gender) ? (object)DBNull.Value : gender;

                    // @height y @weight con precisión/escala correctas
                    var pHeight = cmd.Parameters.Add("@height", SqlDbType.Decimal);
                    pHeight.Precision = 5; pHeight.Scale = 2;
                    pHeight.Value = height.HasValue ? (object)height.Value : DBNull.Value;

                    var pWeight = cmd.Parameters.Add("@weight", SqlDbType.Decimal);
                    pWeight.Precision = 5; pWeight.Scale = 2;
                    pWeight.Value = weight.HasValue ? (object)weight.Value : DBNull.Value;

                    // @phone
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar, 20).Value =
                        string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone;


                    // Guardar imagen si viene
                    if (fuProfileImage.HasFile)
                    {
                        string fileName = Guid.NewGuid() + System.IO.Path.GetExtension(fuProfileImage.FileName);
                        string folder = Server.MapPath("~/Uploads/");
                        if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
                        string savePath = System.IO.Path.Combine(folder, fileName);
                        fuProfileImage.SaveAs(savePath);
                        filePath = "~/Uploads/" + fileName;
                    }
                    cmd.Parameters.AddWithValue("@img", (object)filePath ?? DBNull.Value);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                // 4) autenticar y redirigir
                var ticket = new FormsAuthenticationTicket(1, email, DateTime.Now,
                                DateTime.Now.AddHours(6), false, "Usuario");
                string enc = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
                { HttpOnly = true, Secure = Request.IsSecureConnection };
                Response.Cookies.Add(cookie);

                Response.Redirect("Payment.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception ex)
            {
                AddError("Error al registrar. Intenta de nuevo. Detalle: " + ex.Message);
            }
        }

        private void AddError(string msg)
        {
            Page.Validators.Add(new CustomValidator
            {
                IsValid = false,
                ErrorMessage = msg,
                Display = ValidatorDisplay.None,
                ValidationGroup = "reg",
                EnableClientScript = false
            });
        }
  


private static string GetCs()
        {
            return System.Configuration.ConfigurationManager
                   .ConnectionStrings["GYM"].ConnectionString;
        }

        private static byte[] Hash(string password, byte[] salt)
        {
            using (var sha = SHA256.Create())
            {
                byte[] pw = Encoding.UTF8.GetBytes(password);
                byte[] buf = new byte[salt.Length + pw.Length];
                System.Buffer.BlockCopy(salt, 0, buf, 0, salt.Length);
                System.Buffer.BlockCopy(pw, 0, buf, salt.Length, pw.Length);
                return sha.ComputeHash(buf);
            }
        }

   
    }
}
