#region References
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for a line of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLineSettings
    {
        #region Consts
        private const DashStyle defaultDash = DashStyle.Solid;
        private const bool defaultVisible = true;
        private const float defaultWeight = 1.0F;
        #endregion

        #region Fields
        private Color color = Color.Empty;
        private DashStyle dash = ChartLineSettings.defaultDash;
        private bool visible = ChartLineSettings.defaultVisible;
        private float weight = ChartLineSettings.defaultWeight;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLineSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLineSettings(Control owner)
        {
            this.owner = owner;
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
        /// Determines whether the line is displayed or not.
        /// </summary>
        [DefaultValue(ChartLineSettings.defaultVisible)]
        [Description("Determines whether the line is displayed or not.")]
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

        /// <summary>
        /// Gets/sets a value representing the dash style of the line.
        /// </summary>
        [DefaultValue(ChartLineSettings.defaultDash)]
        [Description("Gets/sets a value representing the dash style of the line.")]
        public DashStyle Dash
        {
            get
            {
                return this.dash;
            }
            set
            {
                if (this.dash != value)
                {
                    this.dash = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the weight of the line.
        /// </summary>
        [DefaultValue(ChartLineSettings.defaultWeight)]
        [Description("Gets/sets a value representing the weight of the line.")]
        public float Weight
        {
            get
            {
                return this.weight;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                if (this.weight != value)
                {
                    this.weight = value;
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

        /// <summary>
        /// Convert the chart line setting to a Pen object used to draw the line.
        /// </summary>
        /// <returns>A Pen object representing the chart line settings.</returns>
        public Pen ToPen()
        {
            Pen pen = new Pen(this.Color);
            pen.DashStyle = this.dash;
            pen.Width = this.weight;
            return pen;
        }

        #endregion
    }
}