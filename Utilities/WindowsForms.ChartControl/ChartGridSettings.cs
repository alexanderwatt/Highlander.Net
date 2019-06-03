#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for the grid of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartGridSettings
    {
        #region Consts
        private const ChartGridStyles defaultXAxisStyle = ChartGridStyles.None;
        private const ChartGridStyles defaultYAxisStyle = ChartGridStyles.Major;
        #endregion

        #region Fields
        private ChartGridStyles xAxisStyle = ChartGridSettings.defaultXAxisStyle;
        private ChartGridStyles yAxisStyle = ChartGridSettings.defaultYAxisStyle;
        private ChartLineSettings line = null;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartGridSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartGridSettings(Control owner)
        {
            this.owner = owner;
            // init fields
            this.line = new ChartLineSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the line of the grid.
        /// </summary>
        [Description("Determines the display settings for the line of the grid.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line
        {
            get
            {
                return this.line;
            }
        }

        /// <summary>
        /// Gets/sets a value representing the style of the time scale grid of the chart.
        /// </summary>
        [DefaultValue(ChartGridSettings.defaultXAxisStyle)]
        [Description("Gets/sets a value representing the style of the time scale grid of the chart.")]
        public ChartGridStyles XAxisStyle
        {
            get
            {
                return this.xAxisStyle;
            }
            set
            {
                if (this.xAxisStyle != value)
                {
                    this.xAxisStyle = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the style of the values scale grid of the chart.
        /// </summary>
        [DefaultValue(ChartGridSettings.defaultYAxisStyle)]
        [Description("Gets/sets a value representing the style of the values scale grid of the chart.")]
        public ChartGridStyles YAxisStyle
        {
            get
            {
                return this.yAxisStyle;
            }
            set
            {
                if (this.yAxisStyle != value)
                {
                    this.yAxisStyle = value;
                    this.owner.Invalidate();
                }
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