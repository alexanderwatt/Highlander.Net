#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the setting for the time series of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartSeriesSettings
    {
        #region Fields
        private ChartLineSettings line = null;
        private ChartLabelsSettings labels = null;
        private ChartMarkSettings mark = null;
        private ChartProjectionsSettings projections = null;
        private ChartSeriesValueCollection values = null;
        private ChartTitleSettings title = null;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartSeriesSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartSeriesSettings(Control owner)
        {
            this.owner = owner;
            // init fields
            this.line = new ChartLineSettings(owner);
            this.labels = new ChartLabelsSettings(owner);
            this.mark = new ChartMarkSettings(owner);
            this.projections = new ChartProjectionsSettings(owner);
            this.values = new ChartSeriesValueCollection();
            this.title = new ChartTitleSettings(owner);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings of the line for the chart series.
        /// </summary>
        [Description("Determines the display settings of the line for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line
        {
            get
            {
                return this.line;
            }
        }

        /// <summary>
        /// Determines the display settings of the labels for the chart series.
        /// </summary>
        [Description("Determines the display settings of the labels for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLabelsSettings Labels
        {
            get
            {
                return this.labels;
            }
        }

        /// <summary>
        /// Determines the display settings of the marks for the chart series.
        /// </summary>
        [Description("Determines the display settings of the marks for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartMarkSettings Mark
        {
            get
            {
                return this.mark;
            }
        }

        /// <summary>
        /// Determines the display settings of the projections for the chart series.
        /// </summary>
        [Description("Determines the display settings of the projections for the chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartProjectionsSettings Projections
        {
            get
            {
                return this.projections;
            }
        }

        /// <summary>
        /// Represents a colletion of values to be displayed in the chart.
        /// </summary>
        [Description("Represents a colletion of values to be displayed in the chart.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartSeriesValueCollection Values
        {
            get
            {
                return this.values;
            }
        }

        /// <summary>
        /// Determines the display settings for the title of the chart series.
        /// </summary>
        [Description("Determines the display settings for the title of chart series.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTitleSettings Title
        {
            get
            {
                return this.title;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Returns a string that represents the current object.  
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "(" + this.GetType().Name + ")";
        }
        #endregion
    }
}