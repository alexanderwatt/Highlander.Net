#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for the grid of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartGridSettings
    {
        #region Consts

        private const ChartGridStyles DefaultXAxisStyle = ChartGridStyles.None;
        private const ChartGridStyles DefaultYAxisStyle = ChartGridStyles.Major;

        #endregion

        #region Fields

        private ChartGridStyles _xAxisStyle = DefaultXAxisStyle;
        private ChartGridStyles _yAxisStyle = DefaultYAxisStyle;
        private readonly Control _owner;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of class ChartGridSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartGridSettings(Control owner)
        {
            _owner = owner;
            // init fields
            Line = new ChartLineSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the line of the grid.
        /// </summary>
        [Description("Determines the display settings for the line of the grid.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line { get; }

        /// <summary>
        /// Gets/sets a value representing the style of the time scale grid of the chart.
        /// </summary>
        [DefaultValue(DefaultXAxisStyle)]
        [Description("Gets/sets a value representing the style of the time scale grid of the chart.")]
        public ChartGridStyles XAxisStyle
        {
            get => _xAxisStyle;
            set
            {
                if (_xAxisStyle != value)
                {
                    _xAxisStyle = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the style of the values scale grid of the chart.
        /// </summary>
        [DefaultValue(DefaultYAxisStyle)]
        [Description("Gets/sets a value representing the style of the values scale grid of the chart.")]
        public ChartGridStyles YAxisStyle
        {
            get => _yAxisStyle;
            set
            {
                if (_yAxisStyle != value)
                {
                    _yAxisStyle = value;
                    _owner.Invalidate();
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
            return "(" + GetType().Name + ")";
        }

        #endregion
    }
}