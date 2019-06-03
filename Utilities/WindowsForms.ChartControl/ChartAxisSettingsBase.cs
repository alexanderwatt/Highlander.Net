#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the display setting for a generic axis of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class ChartAxisSettingsBase
    {
        #region Consts
        private const ChartTickMarkTypes defaultMinorTick = ChartTickMarkTypes.None;
        private const ChartTickMarkTypes defaultMajorTick = ChartTickMarkTypes.Outside;
        #endregion

        #region Fields
        private ChartTitleSettings title = null;
        private ChartLineSettings line = null;
        private ChartLabelsSettings labels = null;
        private ChartTickMarkTypes minorTick = ChartAxisSettingsBase.defaultMinorTick;
        private ChartTickMarkTypes majorTick = ChartAxisSettingsBase.defaultMajorTick;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartAxisSettingsBase.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartAxisSettingsBase(Control owner)
        {
            this.owner = owner;
            // init fields
            this.line = new ChartLineSettings(owner);
            this.labels = new ChartLabelsSettings(owner);
            this.title = new ChartTitleSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the title of the axis.
        /// </summary>
        [Description("Determines the display settings for the title of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTitleSettings Title
        {
            get
            {
                return this.title;
            }
        }

        /// <summary>
        /// Determines the display settings for the line of the axis.
        /// </summary>
        [Description("Determines the display settings for the line of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line
        {
            get
            {
                return this.line;
            }
        }

        /// <summary>
        /// Gets/sets a value representing the type of the minor tick of the the axis.
        /// </summary>
        [Description("Gets/sets a value representing the type of the minor tick of the the axis.")]
        [DefaultValue(ChartAxisSettingsBase.defaultMinorTick)]
        public ChartTickMarkTypes MinorTick
        {
            get
            {
                return this.minorTick;
            }
            set
            {
                if (this.minorTick != value)
                {
                    this.minorTick = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the type of the major tick of the the axis.
        /// </summary>
        [Description("Gets/sets a value representing the type of the major tick of the the axis.")]
        [DefaultValue(ChartAxisSettingsBase.defaultMajorTick)]
        public ChartTickMarkTypes MajorTick
        {
            get
            {
                return this.majorTick;
            }
            set
            {
                if (this.majorTick != value)
                {
                    this.majorTick = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Determines the display settings for the labels of the axis.
        /// </summary>
        [Description("Determines the display settings for the labels of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLabelsSettings Labels
        {
            get
            {
                return this.labels;
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