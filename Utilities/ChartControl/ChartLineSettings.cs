#region References

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for a line of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLineSettings
    {
        #region Consts
        private const DashStyle DefaultDash = DashStyle.Solid;
        private const bool DefaultVisible = true;
        private const float DefaultWeight = 1.0F;
        #endregion

        #region Fields
        private Color _color = Color.Empty;
        private DashStyle _dash = ChartLineSettings.DefaultDash;
        private bool _visible = ChartLineSettings.DefaultVisible;
        private float _weight = ChartLineSettings.DefaultWeight;
        private readonly Control _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLineSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLineSettings(Control owner)
        {
            _owner = owner;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the color of the line.
        /// </summary>
        [Description("Gets/sets a value representing the color of the line.")]
        public Color Color
        {
            get
            {
                if (_color == Color.Empty)
                {
                    return _owner.ForeColor;
                }
                return _color;
            }
            set
            {
                if (_color == value) return;
                _color = value;
                _owner.Invalidate();
            }
        }
        private bool ShouldSerializeColor()
        {
            return _color != Color.Empty;
        }

        /// <summary>
        /// Determines whether the line is displayed or not.
        /// </summary>
        [DefaultValue(DefaultVisible)]
        [Description("Determines whether the line is displayed or not.")]
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;
                _visible = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the dash style of the line.
        /// </summary>
        [DefaultValue(DefaultDash)]
        [Description("Gets/sets a value representing the dash style of the line.")]
        public DashStyle Dash
        {
            get => _dash;
            set
            {
                if (_dash == value) return;
                _dash = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the weight of the line.
        /// </summary>
        [DefaultValue(DefaultWeight)]
        [Description("Gets/sets a value representing the weight of the line.")]
        public float Weight
        {
            get => _weight;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (_weight != value)
                {
                    _weight = value;
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

        /// <summary>
        /// Convert the chart line setting to a Pen object used to draw the line.
        /// </summary>
        /// <returns>A Pen object representing the chart line settings.</returns>
        public Pen ToPen()
        {
            Pen pen = new Pen(Color) {DashStyle = _dash, Width = _weight};
            return pen;
        }

        #endregion
    }
}