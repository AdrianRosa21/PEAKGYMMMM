using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace PEAKGYMM
{
    public partial class Payment : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Bloquea acceso directo sin login (solo Session)
            if (Session["UserId"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                txtStartDate.Text = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
            }
        }

        protected void btnPay_Click(object sender, EventArgs e)
        {
            lblOk.Visible = false;

            // ===== 1) LEER INPUTS DEL POST =====
            string plan = (Request.Form[hidPlan.UniqueID] ?? Request.Form["hidPlan"] ?? "").Trim();
            string priceRaw = (Request.Form[hidPrice.UniqueID] ?? Request.Form["hidPrice"] ?? "").Trim();
            string methodRaw = (Request.Form[ddlMethod.UniqueID] ?? ddlMethod.SelectedValue ?? "").Trim();

            if (string.IsNullOrEmpty(plan))
            { AddError("Selecciona un plan."); return; }

            string method;
            switch (methodRaw.ToUpperInvariant())
            {
                case "CASH": method = "CASH"; break;
                case "CARD": method = "CARD"; break;
                case "TRANSFER": method = "TRANSFER"; break;
                default: AddError("Elige un método de pago."); return;
            }

            int months = plan == "Mensual" ? 1 : (plan == "Trimestral" ? 3 : 12);

            if (!decimal.TryParse(priceRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
                price = plan == "Mensual" ? 15m : (plan == "Trimestral" ? 40m : 120m);

            if (!DateTime.TryParseExact(txtStartDate.Text, "yyyy-MM-dd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var start))
                start = DateTime.UtcNow.Date;

            var end = start.AddMonths(months);

            // Reflejo UI
            txtPlan.Text = plan;
            txtPrice.Text = "$ " + price.ToString("0.00", CultureInfo.InvariantCulture);
            txtEndDate.Text = end.ToString("yyyy-MM-dd");

            // ===== 2) USUARIO POR SESSION =====
            if (Session["UserId"] == null)
            { AddError("Sesión expirada. Inicia sesión nuevamente."); return; }

            int userId = (int)Session["UserId"];

            // ===== 3) ASEGURAR MEMBER (crear si no existe) =====
            int memberId;
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                {
                    cn.Open();

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
                            string fullName = "Sin nombre", uemail = null, phone = null;
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
                                cmdIns.Parameters.AddWithValue("@mail", (object)uemail ?? DBNull.Value);
                                cmdIns.Parameters.AddWithValue("@uid", userId);
                                memberId = (int)cmdIns.ExecuteScalar();
                            }
                        }
                    }
                }
            }
            catch
            {
                AddError("No fue posible crear/obtener el perfil de miembro.");
                return;
            }

            // ===== 4) INSERTAR MEMBERSHIP =====
            int membershipId;
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                using (var cmdMs = new SqlCommand(@"
INSERT INTO dbo.Membership(MemberId, Plans, Price, StartDate, EndDate, Status)
OUTPUT INSERTED.MembershipId
VALUES(@mid, @plan, @price, @start, @end, 'ACTIVE');", cn))
                {
                    cmdMs.Parameters.AddWithValue("@mid", memberId);
                    cmdMs.Parameters.AddWithValue("@plan", plan);
                    cmdMs.Parameters.AddWithValue("@price", price);
                    cmdMs.Parameters.AddWithValue("@start", start);
                    cmdMs.Parameters.AddWithValue("@end", end);

                    cn.Open();
                    membershipId = (int)cmdMs.ExecuteScalar();
                }
            }
            catch (SqlException sx)
            {
                AddError("No fue posible registrar la membresía. " + sx.Message);
                return;
            }
            catch
            {
                AddError("No fue posible registrar la membresía.");
                return;
            }

            // ===== 5) INSERTAR PAYMENT =====
            try
            {
                using (var cn = new SqlConnection(GetCs()))
                using (var cmdPay = new SqlCommand(@"
INSERT INTO dbo.Payment(MembershipId, Amount, Method)
VALUES(@msid, @amount, @method);", cn))
                {
                    cmdPay.Parameters.AddWithValue("@msid", membershipId);
                    cmdPay.Parameters.AddWithValue("@amount", price);
                    cmdPay.Parameters.AddWithValue("@method", method);

                    cn.Open();
                    cmdPay.ExecuteNonQuery();
                }
            }
            catch (SqlException sx)
            {
                AddError("No fue posible registrar el pago. " + sx.Message);
                return;
            }
            catch
            {
                AddError("No fue posible registrar el pago.");
                return;
            }

            // ===== 6) LISTO =====
            lblOk.Text = "¡Membresía activada con éxito! 🎉";
            lblOk.Visible = true;
            Response.Redirect("~/User/MyWorkout.aspx", false);
        }

        // ===== Helpers =====
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
                Display = ValidatorDisplay.None,
                ValidationGroup = "pay",
                EnableClientScript = false
            };
            Page.Validators.Add(v);
        }
    }
}
