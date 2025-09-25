using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web;
using System.Web.UI;
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
            // 1) Tomar el plan desde el HiddenField (NO desde txtPlan)
            var plan = (hidPlan.Value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(plan))
            {
                AddError("Selecciona un plan.");
                return;
            }

            // 2) Método de pago
            if (string.IsNullOrWhiteSpace(ddlMethod.SelectedValue))
            {
                AddError("Elige un método de pago.");
                return;
            }

            try
            {
                // 3) Usuario autenticado
                var email = Context.User?.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                {
                    AddError("Sesión expirada. Inicia sesión nuevamente.");
                    return;
                }

                // 4) Meses por plan
                int months = plan == "Mensual" ? 1 : (plan == "Trimestral" ? 3 : 12);

                // 5) Precio (usa el hidden; si no viene, back-up por plan)
                decimal price;
                if (!decimal.TryParse(hidPrice.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                    price = plan == "Mensual" ? 15m : (plan == "Trimestral" ? 40m : 120m);

                // 6) Fechas
                DateTime start;
                if (!DateTime.TryParseExact(txtStartDate.Text, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
                    start = DateTime.UtcNow.Date;

                DateTime end = start.AddMonths(months);

                // (opcional) reflejar en UI por si quieres ver el resumen ya calculado
                txtPlan.Text = plan;
                txtPrice.Text = "$ " + price.ToString("0.00", CultureInfo.InvariantCulture);
                txtEndDate.Text = end.ToString("yyyy-MM-dd");

                // 7) Buscar UserId
                int userId;
                using (var cn = new SqlConnection(GetCs()))
                using (var cmd = new SqlCommand("SELECT UserId FROM dbo.[User] WHERE Email=@e", cn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    cn.Open();
                    var o = cmd.ExecuteScalar();
                    if (o == null) { AddError("Usuario no encontrado."); return; }
                    userId = (int)o;
                }

                // 8) Asegurar Member y crear Membership + Payment
                int memberId;
                using (var cn = new SqlConnection(GetCs()))
                {
                    cn.Open();

                    // ¿Member ya existe para este UserId?
                    using (var cmdFind = new SqlCommand(
                        "SELECT MemberId FROM dbo.Member WHERE UserId=@uid", cn))
                    {
                        cmdFind.Parameters.AddWithValue("@uid", userId);
                        var o = cmdFind.ExecuteScalar();
                        if (o != null)
                        {
                            memberId = (int)o;
                        }
                        else
                        {
                            // crear Member con datos desde [User]
                            string fullName = "Sin nombre", uemail = email, phone = null;
                            using (var cmdU = new SqlCommand(
                                "SELECT FullName, Email, Phone FROM dbo.[User] WHERE UserId=@uid", cn))
                            {
                                cmdU.Parameters.AddWithValue("@uid", userId);
                                using (var rd = cmdU.ExecuteReader())
                                {
                                    if (rd.Read())
                                    {
                                        if (!(rd["FullName"] is DBNull)) fullName = (string)rd["FullName"];
                                        if (!(rd["Email"] is DBNull)) uemail = (string)rd["Email"];
                                        if (!(rd["Phone"] is DBNull)) phone = (string)rd["Phone"];
                                    }
                                }
                            }

                            const string insMember = @"
INSERT INTO dbo.Member(FullName, Phone, Email, Notes, CreatedAt, IsActive, UserId)
OUTPUT INSERTED.MemberId
VALUES(@name, @phone, @mail, NULL, SYSUTCDATETIME(), 1, @uid);";
                            using (var cmdIns = new SqlCommand(insMember, cn))
                            {
                                cmdIns.Parameters.AddWithValue("@name", fullName ?? "Sin nombre");
                                cmdIns.Parameters.AddWithValue("@phone", (object)phone ?? DBNull.Value);
                                cmdIns.Parameters.AddWithValue("@mail", uemail);
                                cmdIns.Parameters.AddWithValue("@uid", userId);
                                memberId = (int)cmdIns.ExecuteScalar();
                            }
                        }
                    }

                    // Membership
                    int membershipId;
                    const string insMembership = @"
INSERT INTO dbo.Membership(MemberId, Plans, Price, StartDate, EndDate, Status)
OUTPUT INSERTED.MembershipId
VALUES(@mid, @plan, @price, @start, @end, 'ACTIVE');";
                    using (var cmdMs = new SqlCommand(insMembership, cn))
                    {
                        cmdMs.Parameters.AddWithValue("@mid", memberId);
                        cmdMs.Parameters.AddWithValue("@plan", plan);
                        cmdMs.Parameters.AddWithValue("@price", price);
                        cmdMs.Parameters.AddWithValue("@start", start);
                        cmdMs.Parameters.AddWithValue("@end", end);
                        membershipId = (int)cmdMs.ExecuteScalar();
                    }

                    // Payment
                    const string insPay = @"
INSERT INTO dbo.Payment(MembershipId, Amount, Method)
VALUES(@msid, @amount, @method);";
                    using (var cmdPay = new SqlCommand(insPay, cn))
                    {
                        cmdPay.Parameters.AddWithValue("@msid", membershipId);
                        cmdPay.Parameters.AddWithValue("@amount", price);
                        cmdPay.Parameters.AddWithValue("@method", ddlMethod.SelectedValue);
                        cmdPay.ExecuteNonQuery();
                    }
                }

                // 9) OK
                lblOk.Text = "¡Membresía activada con éxito!  🎉";
            }
            catch (Exception ex)
            {
                AddError("No fue posible procesar el pago: " + ex.Message);
            }
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
