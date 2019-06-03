#region References
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for a value mark of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartMarkSettings
    {
        #region Consts
        private const ChartMarkShapes defaultShape = ChartMarkShapes.Diamond;
        #endregion

        #region Fields
        private Color borderColor = Color.Empty;
        private Color fillColor = Color.Empty;
        private ChartMarkShapes shape = ChartMarkSettings.defaultShape;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartMarkSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartMarkSettings(Control owner)
        {
            this.owner = owner;
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
                if (this.borderColor == Color.Empty)
                {
                    return this.owner.ForeColor;
                }
                else
                {
                    return this.borderColor;
                }
            }
            set
            {
                if (this.borderColor != value)
                {
                    this.borderColor = value;
                    this.owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeBorderColor()
        {
            return this.borderColor != Color.Empty;
        }

        /// <summary>
        /// Gets/sets a value representing the color used to fill the value mark.
        /// </summary>
        [Description("Gets/sets a value representing the color used to fill the value mark.")]
        public Color FillColor
        {
            get
            {
                if (this.fillColor == Color.Empty)
                {
                    return this.owner.ForeColor;
                }
                else
                {
                    return this.fillColor;
                }
            }
            set
            {
                if (this.fillColor != value)
                {
                    this.fillColor = value;
                    this.owner.Invalidate();
                }
            }
        }
        private bool ShouldSerializeFillColor()
        {
            return this.fillColor != Color.Empty;
        }


        /// <summary>
        /// Determines the shape of the value mark.
        /// </summary>
        [DefaultValue( ChartMarkSettings.defaultShape)]
        [Description("Determines the shape of the value mark.")]
        public ChartMarkShapes Shape
        {
            get
            {
                return this.shape;
            }
            set
            {
                if (this.shape != value)
                {
                    this.shape = value;
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