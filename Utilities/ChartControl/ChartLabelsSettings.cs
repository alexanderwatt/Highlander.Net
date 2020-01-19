#region References

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for labels of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLabelsSettings
    {
        #region Consts
        private const bool DefaultVisible = true;
        private const float DefaultRotation = 0.0F;
        #endregion

        #region Fields
        private Color _color = Color.Empty;
        private Font _font;
        private float _rotation = DefaultRotation;
        private bool _visible = DefaultVisible;
        private readonly Control _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLabelsSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLabelsSettings(Control owner)
        {
            _owner = owner;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the color used to display the label.
        /// </summary>
        [Description("Gets/sets a value representing the color used to display the label.")]
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
                if (_color != value)
                {
                    _color = value;
                    _owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeColor()
        {
            return _color != Color.Empty;
        }

        /// <summary>
        /// Gets/sets a value representing the font used to display the label.
        /// </summary>
        [Description("Gets/sets a value representing the font used to display the label.")]
        public Font Font
        {
            get
            {
                if (_font == null)
                {
                    return _owner.Font;
                }
                return _font;
            }
            set
            {
                if (_font != value)
                {
                    _font = value;
                    _owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeFont()
        {
            return _font != null;
        }

        /// <summary>
        /// Determines the rotation angle of the label.
        /// </summary>
        [DefaultValue(DefaultRotation)]
        [Description("Determines the rotation angle of the label.")]
        public float Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    _owner.Invalidate();
                }
            }
        }
        /// <summary>
        /// Determines whether the label is displayed or not.
        /// </summary>
        [DefaultValue(DefaultVisible)]
        [Description("Determines whether the label is displayed or not.")]
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