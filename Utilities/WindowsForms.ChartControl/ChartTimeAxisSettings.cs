#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for the time axis of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartTimeAxisSettings : ChartAxisSettingsBase
    {
        #region Fields
        private ChartTimeScaleSettings scale = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartTimeAxisSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartTimeAxisSettings(Control owner) : base(owner)
        {
            // init fields
            this.scale = new ChartTimeScaleSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the scale of the axis.
        /// </summary>
        [Description("Determines the display settings for the scale of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTimeScaleSettings Scale
        {
            get
            {
                return this.scale;
            }
        }

        #endregion
    }
}