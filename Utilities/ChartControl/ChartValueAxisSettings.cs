
#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for the value axis of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartValueAxisSettings : ChartAxisSettingsBase
    {
        #region Fields

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartValueAxisSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartValueAxisSettings(Control owner) : base(owner)
        {
            // init fields
            Scale = new ChartValueScaleSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the scale of the axis.
        /// </summary>
        [Description("Determines the display settings for the scale of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartValueScaleSettings Scale { get; }

        #endregion
    }
}