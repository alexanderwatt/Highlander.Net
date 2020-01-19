using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using Orion.WebViewer.Properties;

namespace Orion.WebViewer.Trade
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FromDateLabel.Text = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);
                ToDateLabel.Text = DateTime.MinValue.ToString(CultureInfo.InvariantCulture);

                FromDatePicker.SelectedDate = DateTime.Today.AddDays(-1);

                var productTypes = new List<ListItem> { new ListItem(Resources.UserInteface.AllItems) };
                productTypes.AddRange(Business.TradeProvider.SupportedProductTypes.Select(a => new ListItem(a)));
                ProductTypeDropDownList.Items.AddRange(productTypes.ToArray());
            }
        }

        protected void SubmitButtonClick(object sender, EventArgs e)
        {
            // Reset previous clicks
            TradesGridView.PageIndex = 0;
            TradesGridView.SelectedIndex = -1;
            // Only bind the dates on submit
            FromDateLabel.Text = FromDatePicker.SelectedDate.ToString(Resources.UserInteface.DateFormat);
            ToDateLabel.Text = ToDatePicker.SelectedDate.ToString(Resources.UserInteface.DateFormat);
        }

        protected void TradesGridViewInit(object sender, EventArgs e)
        {
            // Set the pagesize from config settings
            TradesGridView.PageSize = (new Settings()).PageSize;
        }

        protected void TradesGridViewPageIndexChanged(object sender, EventArgs e)
        {
            // If you change pages, then remove the chosen trade
            TradesGridView.SelectedIndex = -1;
        }

        protected void TradesGridViewRowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Handle clicking on a row to choose the trade
            if (e.Row.RowIndex > -1)
            {
                string reference = ClientScript.GetPostBackEventReference(TradesGridView, "Select$" + e.Row.RowIndex);
                e.Row.Attributes.Add("onclick", reference);
            }
        }
    }
}
