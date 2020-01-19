#region References

using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for a title of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartTitleSettings
    {
        #region Consts

        private const string DefaultText = "";

        #endregion

        #region Fields

        private Color _color = Color.Empty;
        private Font _font;
        private string _text = DefaultText;
        private readonly Control _owner;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartTitleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartTitleSettings(Control owner)
        {
            _owner = owner;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the color used to display the title.
        /// </summary>
        [Description("Gets/sets a value representing the color used to display the title.")]
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
        /// Gets/sets a value representing the font used to display the title.
        /// </summary>
        [Description("Gets/sets a value representing the font used to display the title.")]
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
        /// Gets/sets a value representing the text of the title.
        /// </summary>
        [DefaultValue(ChartTitleSettings.DefaultText)]
        [Description("Gets/sets a value representing the text of the title.")]
        public string Text
        {
            get => _text;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }
                if (_text != value)
                {
                    _text = value;
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