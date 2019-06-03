#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for the legend of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartLegendSettings
    {
        #region Consts
        private const ChartLegendPosition defaultPosition = ChartLegendPosition.Right;
        private const bool defaultVisible = false;
        #endregion

        #region Fields
        private ChartLineSettings border = null;
        private ChartLegendPosition position = ChartLegendSettings.defaultPosition;
        private bool visible = ChartLegendSettings.defaultVisible;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartLegendSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartLegendSettings(Control owner)
        {
            this.owner = owner;
            // init fields
            this.border = new ChartLineSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the border of the legend.
        /// </summary>
        [Description("Determines the display settings for the border of the legend.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Border
        {
            get
            {
                return this.border;
            }
        }

        /// <summary>
        /// Determines the display position of the legend.
        /// </summary>
        [DefaultValue(ChartLegendSettings.defaultPosition)]
        [Description("Determines the display position of the legend.")]
        public ChartLegendPosition Position
        {
            get
            {
                return this.position;
            }
            set
            {
                if (this.position != value)
                {
                    this.position = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Indicates if the chart legend is shown.
        /// </summary>
        [DefaultValue(ChartLegendSettings.defaultVisible)]
        [Description("Indicates if the chart legend is shown.")]
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