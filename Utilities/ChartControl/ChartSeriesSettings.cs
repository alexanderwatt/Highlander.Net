#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the setting for the time series of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartSeriesSettings
    {
        #region Fields

        private Control _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartSeriesSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartSeriesSettings(Control owner)
        {
            _owner = owner;
            // init fields
            Line = new ChartLineSettings(owner);
            Labels = new ChartLabelsSettings(owner);
            Mark = new ChartMarkSettings(owner);
            Projections = new ChartProjectionsSettings(owner);
            Values = new ChartSeriesValueCollection();
            Title = new ChartTitleSettings(owner);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings of the line for the chart series.
        /// </summary>
        [Description("Determines the display settings of the line for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line { get; }

        /// <summary>
        /// Determines the display settings of the labels for the chart series.
        /// </summary>
        [Description("Determines the display settings of the labels for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLabelsSettings Labels { get; }

        /// <summary>
        /// Determines the display settings of the marks for the chart series.
        /// </summary>
        [Description("Determines the display settings of the marks for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartMarkSettings Mark { get; }

        /// <summary>
        /// Determines the display settings of the projections for the chart series.
        /// </summary>
        [Description("Determines the display settings of the projections for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartProjectionsSettings Projections { get; }

        /// <summary>
        /// Represents a collection of values to be displayed in the chart.
        /// </summary>
        [Description("Represents a colletion of values to be displayed in the chart.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartSeriesValueCollection Values { get; }

        /// <summary>
        /// Determines the display settings for the title of the chart series.
        /// </summary>
        [Description("Determines the display settings for the title of chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTitleSettings Title { get; }

        #endregion

        #region Methods
        /// <summary>
        /// Returns a string that represents the current object.  
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "(" + GetType().Name + ")";
        }
        #endregion
    }
}