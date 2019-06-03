#region References
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for a title of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartTitleSettings
    {
        #region Consts
        private const string defaultText = "";
        #endregion

        #region Fields
        private Color color = Color.Empty;
        private Font font = null;
        private string text = ChartTitleSettings.defaultText;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartTitleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartTitleSettings(Control owner)
        {
            this.owner = owner;
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
                if (this.color == Color.Empty)
                {
                    return this.owner.ForeColor;
                }
                else
                {
                    return this.color;
                }
            }
            set
            {
                if (this.color != value)
                {
                    this.color = value;
                    this.owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeColor()
        {
            return this.color != Color.Empty;
        }

        /// <summary>
        /// Gets/sets a value representing the font used to display the title.
        /// </summary>
        [Description("Gets/sets a value representing the font used to display the title.")]
        public Font Font
        {
            get
            {
                if (this.font == null)
                {
                    return this.owner.Font;
                }
                else
                {
                    return this.font;
                }
            }
            set
            {
                if (this.font != value)
                {
                    this.font = value;
                    this.owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeFont()
        {
            return this.font != null;
        }
		
        /// <summary>
        /// Gets/sets a value representing the text of the title.
        /// </summary>
        [DefaultValue(ChartTitleSettings.defaultText)]
        [Description("Gets/sets a value representing the text of the title.")]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (value == null)
                {
                    value = String.Empty;
                }
                if (this.text != value)
                {
                    this.text = value;
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