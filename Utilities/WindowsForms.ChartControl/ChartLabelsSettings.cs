#region References
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for labels of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLabelsSettings
    {
        #region Consts
        private const bool defaultVisible = true;
        private const float defaultRotation = 0.0F;
        #endregion

        #region Fields
        private Color color = Color.Empty;
        private Font font = null;
        private float rotation = ChartLabelsSettings.defaultRotation;
        private bool visible = ChartLabelsSettings.defaultVisible;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLabelsSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLabelsSettings(Control owner)
        {
            this.owner = owner;
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
        /// Gets/sets a value representing the font used to display the label.
        /// </summary>
        [Description("Gets/sets a value representing the font used to display the label.")]
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
        /// Determines the rotation angle of the label.
        /// </summary>
        [DefaultValue(ChartLabelsSettings.defaultRotation)]
        [Description("Determines the rotation angle of the label.")]
        public float Rotation
        {
            get
            {
                return this.rotation;
            }
            set
            {
                if (this.rotation != value)
                {
                    this.rotation = value;
                    this.owner.Invalidate();
                }
            }
        }
        /// <summary>
        /// Determines whether the label is displayed or not.
        /// </summary>
        [DefaultValue(ChartLabelsSettings.defaultVisible)]
        [Description("Determines whether the label is displayed or not.")]
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                if (this.visible != value)
                {
                    this.visible = value;
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