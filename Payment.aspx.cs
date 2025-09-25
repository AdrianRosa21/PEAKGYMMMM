using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.Security;

using System.Web.UI.WebControls;
namespace PEAKGYMM
{
    public partial class Payment : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // start hoy por defecto (el JS también lo hace por si acaso)
                txtStartDate.Text = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            lblOk.Visible = false;

            // -------- 1) LEER INPUTS (desde POST) --------
            string plan = (Request.Form[hidPlan.UniqueID] ?? Request.Form["hidPlan"] ?? "").Trim();
            string priceRaw = (Request.Form[hidPrice.UniqueID] ?? Request.Form["hidPrice"] ?? "").Trim();

            // Método: intenta por UniqueID y por id estático; si viene inválido, default CARD
            string methodRaw = (Request.Form[ddlMethod.UniqueID] ?? Request.Form["ddlMethod"] ?? ddlMethod.SelectedValue ?? "").Trim();
            string method = NormalizeMethod(methodRaw); // -> CASH|CARD|TRANSFER; default CARD

            if (string.IsNullOrEmpty(plan)) { AddError("Selecciona un plan."); return; }

            // Meses según plan
            int months = plan == "Mensual" ? 1 : (plan == "Trimestral" ? 3 : 12);

            // Precio: hidden o fallback por plan
            if (!decimal.TryParse(priceRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                price = plan == "Mensual" ? 15m : (plan == "Trimestral" ? 40m : 120m);

            // Fechas
            if (!DateTime.TryParseExact(txtStartDate.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
                start = DateTime.UtcNow.Date;
            var end = start.AddMonths(months);

            // Refleja en UI (opcional)
            txtPlan.Text = plan;
            txtPrice.Text = "$ " + price.ToString("0.00", CultureInfo.InvariantCulture);
            txtEndDate.Text = end.ToString("yyyy-MM-dd");

            // -------- 2) RESOLVER USUARIO (a prueba de balas) --------
            if (!TryResolveUser(out int userId, out string email))
            {   // si no se pudo, fuerza login
                Response.Redirect("~/Login.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // -------- 3) ASEGURAR MEMBER --------
            int memberId;
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                {
                    cn.Open();

                    using (var find = new SqlCommand("SELECT MemberId FROM dbo.Member WHERE UserId=@uid", cn))
                    {
                        find.Parameters.AddWithValue("@uid", userId);
                        var o = find.ExecuteScalar();
                        if (o != null)
                        {
                            memberId = (int)o;
                        }
                        else
                        {
                            string fullName = "Sin nombre", uemail = email, phone = null;
                            using (var u = new SqlCommand("SELECT FullName, Email, Phone FROM dbo.[User] WHERE UserId=@uid", cn))
                            {
                                u.Parameters.AddWithValue("@uid", userId);
                                using (var rd = u.ExecuteReader())
                                {
                                    if (rd.Read())
                                    {
                                        if (!(rd["FullName"] is DBNull)) fullName = (string)rd["FullName"];
                                        if (!(rd["Email"] is DBNull)) uemail = (string)rd["Email"];
                                        if (!(rd["Phone"] is DBNull)) phone = (string)rd["Phone"];
                                    }
                                }
                            }

                            const string ins = @"
INSERT INTO dbo.Member(FullName, Phone, Email, Notes, CreatedAt, IsActive, UserId)
OUTPUT INSERTED.MemberId
VALUES(@name, @phone, @mail, NULL, SYSUTCDATETIME(), 1, @uid);";
                            using (var insCmd = new SqlCommand(ins, cn))
                            {
                                insCmd.Parameters.AddWithValue("@name", fullName ?? "Sin nombre");
                                insCmd.Parameters.AddWithValue("@phone", (object)phone ?? DBNull.Value);
                                insCmd.Parameters.AddWithValue("@mail", (object)uemail ?? DBNull.Value);
                                insCmd.Parameters.AddWithValue("@uid", userId);
                                memberId = (int)insCmd.ExecuteScalar();
                            }
                        }
                    }
                }
            }
            catch (Exception x)
            {
                AddError("No fue posible crear/obtener el miembro. " + x.Message);
                return;
            }

            // -------- 4) INSERTAR MEMBERSHIP --------
            int membershipId;
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Membership(MemberId, Plans, Price, StartDate, EndDate, Status)
OUTPUT INSERTED.MembershipId
VALUES(@mid, @plan, @price, @start, @end, 'ACTIVE');", cn))
                {
                    cmd.Parameters.AddWithValue("@mid", memberId);
                    cmd.Parameters.AddWithValue("@plan", plan);
                    cmd.Parameters.Add("@price", SqlDbType.Decimal).Value = price;
                    cmd.Parameters.Add("@start", SqlDbType.Date).Value = start.Date;
                    cmd.Parameters.Add("@end", SqlDbType.Date).Value = end.Date;

                    cn.Open();
                    membershipId = (int)cmd.ExecuteScalar();
                }
            }
            catch (SqlException sx)
            {
                AddError("No fue posible registrar la membresía. " + sx.Message);
                return;
            }

            // -------- 5) INSERTAR PAYMENT (con PaidAt) --------
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Payment(MembershipId, Amount, Method, PaidAt)
VALUES(@msid, @amount, @method, SYSUTCDATETIME());", cn))
                {
                    cmd.Parameters.AddWithValue("@msid", membershipId);
                    cmd.Parameters.Add("@amount", SqlDbType.Decimal).Value = price;
                    cmd.Parameters.AddWithValue("@method", method);

                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException sx)
            {
                AddError("No fue posible registrar el pago. " + sx.Message);
                return;
            }

            // -------- 6) LISTO --------
            lblOk.Text = "¡Membresía activada con éxito! 🎉";
            lblOk.Visible = true;
            Response.Redirect("~/User/MyWorkout.aspx", false);
        }


        // ===== Helpers =====
        private static string NormalizeMethod(string raw)
        {
            switch ((raw ?? "").Trim().ToUpperInvariant())
            {
                case "CASH": return "CASH";
                case "CARD": return "CARD";
                case "TRANSFER": return "TRANSFER";
                default: return "CARD"; // default seguro
            }
        }

        private bool TryResolveUser(out int userId, out string email)
        {
            userId = 0; email = null;

            // 1) Session de Login
            if (Session["UserId"] is int sid && sid > 0)
            {
                userId = sid;
                try
                {
                    using (var cn = new SqlConnection(GetCs()))
                    using (var cmd = new SqlCommand("SELECT Email FROM dbo.[User] WHERE UserId=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", sid);
                        cn.Open();
                        var o = cmd.ExecuteScalar();
                        if (o != null) email = (string)o;
                    }
                }
                catch { /* opcional */ }
                return true;
            }

            // 2) Context
            if (Context?.User?.Identity?.IsAuthenticated == true)
                email = Context.User.Identity.Name;

            // 3) Cookie FormsAuth
            if (string.IsNullOrEmpty(email))
            {
                var c = Request.Cookies[FormsAuthentication.FormsCookieName];
                if (c != null && !string.IsNullOrEmpty(c.Value))
                {
                    var t = FormsAuthentication.Decrypt(c.Value);
                    if (t != null && !t.Expired) email = t.Name;
                }
            }

            if (string.IsNullOrEmpty(email)) return false;

            // 4) Mapear email -> UserId
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand("SELECT UserId FROM dbo.[User] WHERE Email=@e", cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cn.Open();
                    var o = cmd.ExecuteScalar();
                    if (o == null) return false;
                    userId = (int)o;
                    return true;
                }
            }
            catch { return false; }
        }

        private static string GetCs()
        {
            return System.Configuration.ConfigurationManager
                   .ConnectionStrings["GYM"].ConnectionString;
        }

        private void AddError(string msg)
        {
            var v = new CustomValidator
            {
                IsValid = false,
                ErrorMessage = msg,
                Display = ValidatorDisplay.None, // el mensaje saldrá en el ValidationSummary
                ValidationGroup = "pay",
                EnableClientScript = false       // <-- evita exigir ServerValidate/JS
            };
            Page.Validators.Add(v);
        }

    }
}
