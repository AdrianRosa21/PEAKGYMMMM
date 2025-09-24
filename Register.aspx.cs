using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

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

                // 2) crear salt + hash (compat .NET 4.7.2)
                byte[] salt = new byte[16];
                using (var rng = new RNGCryptoServiceProvider()) { rng.GetBytes(salt); }
                byte[] hash = Hash(pass, salt);

                // 3) insertar usuario como 'Usuario'
                const string sqlIns =
                    @"INSERT INTO dbo.[User](Email,PasswordHash,PasswordSalt,RoleId,IsActive,CreatedAt)
                      VALUES(@e,@h,@s,(SELECT TOP 1 RoleId FROM dbo.Role WHERE Name='Usuario'),1,SYSUTCDATETIME());";
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(sqlIns, cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.Parameters.Add("@h", SqlDbType.VarBinary, hash.Length).Value = hash;
                    cmd.Parameters.Add("@s", SqlDbType.VarBinary, salt.Length).Value = salt;
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }

                // 4) autenticar de una vez y mandar a ~/User/MyWorkout.aspx
                var ticket = new FormsAuthenticationTicket(
                    1, email, DateTime.Now, DateTime.Now.AddHours(6), false, "Usuario");
                string enc = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
                { HttpOnly = true, Secure = Request.IsSecureConnection };
                Response.Cookies.Add(cookie);

                Response.Redirect("~/User/MyWorkout.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception)
            {
                AddError("Error al registrar. Intenta de nuevo.");
            }
        }

        private static string GetCs()
        {
            return System.Configuration.ConfigurationManager
                   .ConnectionStrings["GymDb"].ConnectionString;
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

        private void AddError(string msg)
        {
            Page.Validators.Add(new CustomValidator { IsValid = false, ErrorMessage = msg });
        }
    }
}
