using System;
using Orion.WebViewer.Trade.Business;

namespace Orion.WebViewer.Trade
{
    public partial class ProductFpml : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string tradeId = Request.QueryString.Get("tradeid");
            var tradeProvider = new TradeProvider();
            Business.Trade trade = tradeProvider.GetTrade(tradeId);
            Response.Write(trade.Fpml);
        }
    }
}
