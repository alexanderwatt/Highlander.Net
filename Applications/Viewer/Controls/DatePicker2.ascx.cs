using System;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Orion.WebViewer.Controls
{
    public partial class DatePicker2 : UserControl
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
                Calendar.SelectedDate = Calendar.VisibleDate = value;
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
            ToggleCalendar(false);
        }

        protected void CalendarSelectionChanged(object sender, EventArgs e)
        {
            SelectedDate = Calendar.SelectedDate;
            ToggleCalendar(false);
        }

        protected void CalendarVisibleMonthChanged(object sender, MonthChangedEventArgs e)
        {
            ToggleCalendar(true);
        }

        #endregion

        #region Helper Methods
        /// <summary>
        /// Toggles display of the calendar popup div.
        /// </summary>
        /// <param name="show">if set to <c>true</c> [show].</param>
        private void ToggleCalendar(bool show)
        {
            calendarPopup.Style[HtmlTextWriterStyle.Display] = show ? string.Empty : "none";
        }
        #endregion

    }
}