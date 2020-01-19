using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Orion.WebViewer.Controls
{
    public partial class DatePicker : UserControl
    {
        public string DateFormat = "dd/MM/yyyy";

        #region Properties
        /// <summary>
        /// Gets or sets the selected date.
        /// </summary>
        /// <value>The selected date.</value>
        public DateTime SelectedDate
        {
            get
            {
                return Calendar.SelectedDate;
            }
            set
            {
                Calendar.SelectedDate = value;
                Calendar.VisibleDate = value;
                dateTextBox.Text = value.ToString(DateFormat, CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Event Handlers
        protected void Page_Load(object sender, EventArgs e)
        {
            if (SelectedDate == DateTime.MinValue)
            {
                SelectedDate = DateTime.Today;
            }
            if (!IsPostBack)
            {
                calendarPopup.Style[HtmlTextWriterStyle.Display] = "none"; 
            }
        }

        protected void CalendarSelectionChanged(object sender, EventArgs e)
        {
            SelectedDate = Calendar.SelectedDate;
            calendarPopup.Style[HtmlTextWriterStyle.Display] = "none";
        }

        protected void CalendarVisibleMonthChanged(object sender, MonthChangedEventArgs e)
        {
            calendarPopup.Style[HtmlTextWriterStyle.Display] = "";
        }

        #endregion
    }
}