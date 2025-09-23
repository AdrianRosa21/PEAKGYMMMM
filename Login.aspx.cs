using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace PEAKGYMM.Account
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
            { ShowError("Completa correo y contraseña."); return; }

            try
            {
                const string sql = @"SELECT TOP 1 UserId, PasswordHash, PasswordSalt, RoleId, IsActive
                                     FROM [User] WHERE Email=@e";
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cn.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (!rd.Read() || !(bool)rd["IsActive"])
                        { ShowError("Credenciales inválidas."); return; }

                        var hashDb = (byte[])rd["PasswordHash"];
                        var saltDb = (byte[])rd["PasswordSalt"];
                        var roleId = (int)rd["RoleId"];

                        var hashCalc = Hash(password, saltDb);
                        if (!TimingSafeEquals(hashDb, hashCalc))
                        { ShowError("Credenciales inválidas."); return; }

                        var roleName = roleId == 1 ? "Admin" : "Usuario";

                        // Ticket con rol
                        var ticket = new FormsAuthenticationTicket(
                            1, email, DateTime.Now,
                            DateTime.Now.AddHours(chkRemember.Checked ? 24 : 6),
                            chkRemember.Checked, roleName);

                        var enc = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enc)
                        { HttpOnly = true, Secure = Request.IsSecureConnection };
                        Response.Cookies.Add(cookie);

                        // Redirección limpia sin Page.Buffer
                        Response.Redirect("~/Default.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
            catch
            {
                ShowError("Error al iniciar sesión. Intenta de nuevo.");
            }
        }

        // ===== Helpers =====
        private static string GetCs() =>
            System.Configuration.ConfigurationManager.ConnectionStrings["GymDb"].ConnectionString;

        private static byte[] Hash(string password, byte[] salt)
        {
            using (var sha = SHA256.Create())
            {
                var pw = Encoding.UTF8.GetBytes(password);
                var buf = new byte[salt.Length + pw.Length];
                
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
