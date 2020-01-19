using System;
using Orion.WebViewer.Curve.Business;

namespace Orion.WebViewer.Curve
{
    public partial class MarketFpml : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string id = Request.QueryString.Get("id");
            var curveProvider = new CurveProvider();
            Business.Curve curve = curveProvider.GetCurve(id);
            Response.Write(curve.Fpml);
        }
    }
}
