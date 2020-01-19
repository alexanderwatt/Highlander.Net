#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the display setting for the projections of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartProjectionsSettings
    {
        #region Consts

        private const bool DefaultVisible = false;

        #endregion

        #region Fields

        private bool _visible = DefaultVisible;
        private readonly Control _owner;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of class ChartProjectionsSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartProjectionsSettings(Control owner)
        {
            _owner = owner;
            // init fields
            Line = new ChartLineSettings(owner);
            Labels = new ChartLabelsSettings(owner);
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines the display settings for the line of the projection.
        /// </summary>
        [Description("Determines the display settings for the line of the projection.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLineSettings Line { get; }

        /// <summary>
        /// Determines the display settings for the labels of the projection.
        /// </summary>
        [Description("Determines the display settings for the labels of the projection.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLabelsSettings Labels { get; }

        /// <summary>
        /// Indicates if the chart marks projections are shown.
        /// </summary>
        [DefaultValue(DefaultVisible)]
        [Description("Indicates if the chart marks projections are shown.")]
        public bool Visible
        {
            get => _visible;
            set
            {
                if (_visible == value) return;
                _visible = value;
                _owner.Invalidate();
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