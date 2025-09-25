using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;



namespace PEAKGYMM
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && Request.QueryString["registered"] == "1")
                pnlRegistered.Visible = true;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            pnlError.Visible = false;
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Text;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Completa correo y contraseña.");
                return;
            }

            try
            {
                const string sqlUser = @"
SELECT TOP 1 UserId, PasswordHash, PasswordSalt, RoleId, IsActive
FROM dbo.[User] WHERE Email=@e;";

                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(sqlUser, cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cn.Open();

                    int userId;
                    int roleId;
                    byte[] hashDb, saltDb;

                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read() || !(bool)rd["IsActive"])
                        { ShowError("Credenciales inválidas."); return; }

                        userId = (int)rd["UserId"];
                        roleId = (int)rd["RoleId"];
                        hashDb = (byte[])rd["PasswordHash"];
                        saltDb = (byte[])rd["PasswordSalt"];
                    }

                    var hashCalc = Hash(password, saltDb);
                    if (!TimingSafeEquals(hashDb, hashCalc))
                    { ShowError("Credenciales inválidas."); return; }

                    // ======== SOLO SESSION (sin cookies) ========
                    Session["UserId"] = userId;
                    Session["Email"] = email;
                    Session["Role"] = roleId == 1 ? "Admin" : "Usuario";

                    // ¿tiene membresía activa?
                    bool tieneActiva;
                    const string sqlActive = @"
SELECT 1
FROM dbo.Member m
JOIN dbo.Membership ms ON ms.MemberId = m.MemberId
WHERE m.UserId = @uid
  AND ms.Status = 'ACTIVE'
  AND ms.EndDate >= CONVERT(date, SYSUTCDATETIME());";

                    using (var cmd2 = new SqlCommand(sqlActive, cn))
                    {
                        cmd2.Parameters.AddWithValue("@uid", userId);
                        tieneActiva = cmd2.ExecuteScalar() != null;
                    }

                    if (tieneActiva)
                        Response.Redirect("~/User/MyWorkout.aspx", false);
                    else
                        Response.Redirect("~/Payment.aspx", false);

                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            catch
            {
                ShowError("Error al iniciar sesión. Intenta de nuevo.");
            }
        }

        // ===== Helpers =====
        private static string GetCs() =>
            System.Configuration.ConfigurationManager.ConnectionStrings["GYM"].ConnectionString;

        private static byte[] Hash(string password, byte[] salt)
        {
            using (var sha = SHA256.Create())
            {
                var pw = Encoding.UTF8.GetBytes(password);
                var buf = new byte[salt.Length + pw.Length];
                System.Buffer.BlockCopy(salt, 0, buf, 0, salt.Length);
                System.Buffer.BlockCopy(pw, 0, buf, salt.Length, pw.Length);
                return sha.ComputeHash(buf);
            }
        }

        private static bool TimingSafeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0; for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private void ShowError(string msg)
        {
            pnlError.Visible = true;
            pnlError.Controls.Clear();
            pnlError.Controls.Add(new LiteralControl(HttpUtility.HtmlEncode(msg)));
        }
    }
}
