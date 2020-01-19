#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for the legend of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLegendSettings
    {
        #region Consts

        private const ChartLegendPosition DefaultPosition = ChartLegendPosition.Right;
        private const bool DefaultVisible = false;

        #endregion

        #region Fields

        private ChartLegendPosition _position = DefaultPosition;
        private bool _visible = DefaultVisible;
        private readonly Control _owner;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLegendSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLegendSettings(Control owner)
        {
            _owner = owner;
            // init fields
            Border = new ChartLineSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the border of the legend.
        /// </summary>
        [Description("Determines the display settings for the border of the legend.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Border { get; }

        /// <summary>
        /// Determines the display position of the legend.
        /// </summary>
        [DefaultValue(ChartLegendSettings.DefaultPosition)]
        [Description("Determines the display position of the legend.")]
        public ChartLegendPosition Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Indicates if the chart legend is shown.
        /// </summary>
        [DefaultValue(DefaultVisible)]
        [Description("Indicates if the chart legend is shown.")]
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible != value)
                {
                    _visible = value;
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