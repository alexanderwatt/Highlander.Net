#region References

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for a value mark of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartMarkSettings
    {
        #region Consts

        private const ChartMarkShapes DefaultShape = ChartMarkShapes.Diamond;

        #endregion

        #region Fields

        private Color _borderColor = Color.Empty;
        private Color _fillColor = Color.Empty;
        private ChartMarkShapes _shape = DefaultShape;
        private readonly Control _owner;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartMarkSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartMarkSettings(Control owner)
        {
            _owner = owner;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the color used to draw the value marks border.
        /// </summary>
        [Description("Gets/sets a value representing the color used to draw the value marks border.")]
        public Color BorderColor
        {
            get
            {
                if (_borderColor == Color.Empty)
                {
                    return _owner.ForeColor;
                }
                return _borderColor;
            }
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    _owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeBorderColor()
        {
            return _borderColor != Color.Empty;
        }

        /// <summary>
        /// Gets/sets a value representing the color used to fill the value mark.
        /// </summary>
        [Description("Gets/sets a value representing the color used to fill the value mark.")]
        public Color FillColor
        {
            get
            {
                if (_fillColor == Color.Empty)
                {
                    return _owner.ForeColor;
                }
                return _fillColor;
            }
            set
            {
                if (_fillColor == value) return;
                _fillColor = value;
                _owner.Invalidate();
            }
        }
        private bool ShouldSerializeFillColor()
        {
            return _fillColor != Color.Empty;
        }


        /// <summary>
        /// Determines the shape of the value mark.
        /// </summary>
        [DefaultValue( DefaultShape)]
        [Description("Determines the shape of the value mark.")]
        public ChartMarkShapes Shape
        {
            get => _shape;
            set
            {
                if (_shape != value)
                {
                    _shape = value;
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