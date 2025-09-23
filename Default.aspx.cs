using System;

namespace PEAKGYMM
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            // si ya está autenticado, lo mandamos a su panel según rol
            if (Context?.User?.Identity?.IsAuthenticated == true)
            {
                if (Context.User.IsInRole("Admin"))
                {
                    Response.Redirect("~/Admin/Members.aspx", endResponse: true);
                }
                else
                {
                    Response.Redirect("~/User/MyWorkout.aspx", endResponse: true);
                }
            }

        }
    }
}
