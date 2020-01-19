#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for a generic axis of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class ChartAxisSettingsBase
    {
        #region Consts
        private const ChartTickMarkTypes DefaultMinorTick = ChartTickMarkTypes.None;
        private const ChartTickMarkTypes DefaultMajorTick = ChartTickMarkTypes.Outside;
        #endregion

        #region Fields

        private ChartTickMarkTypes _minorTick = DefaultMinorTick;
        private ChartTickMarkTypes _majorTick = DefaultMajorTick;
        private readonly Control _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartAxisSettingsBase.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartAxisSettingsBase(Control owner)
        {
            _owner = owner;
            // init fields
            Line = new ChartLineSettings(owner);
            Labels = new ChartLabelsSettings(owner);
            Title = new ChartTitleSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the title of the axis.
        /// </summary>
        [Description("Determines the display settings for the title of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTitleSettings Title { get; }

        /// <summary>
        /// Determines the display settings for the line of the axis.
        /// </summary>
        [Description("Determines the display settings for the line of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line { get; }

        /// <summary>
        /// Gets/sets a value representing the type of the minor tick of the the axis.
        /// </summary>
        [Description("Gets/sets a value representing the type of the minor tick of the the axis.")]
        [DefaultValue(DefaultMinorTick)]
        public ChartTickMarkTypes MinorTick
        {
            get => _minorTick;
            set
            {
                if (_minorTick == value) return;
                _minorTick = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the type of the major tick of the the axis.
        /// </summary>
        [Description("Gets/sets a value representing the type of the major tick of the the axis.")]
        [DefaultValue(DefaultMajorTick)]
        public ChartTickMarkTypes MajorTick
        {
            get => _majorTick;
            set
            {
                if (_majorTick == value) return;
                _majorTick = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Determines the display settings for the labels of the axis.
        /// </summary>
        [Description("Determines the display settings for the labels of the axis.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLabelsSettings Labels { get; }

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