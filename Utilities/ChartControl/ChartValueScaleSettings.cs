#region References

using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the setting for the values axis scale of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartValueScaleSettings
    {
        #region Consts
        private const int DefaultMinimum = 0;
        private const int DefaultMaximum = 100;
        private const int DefaultMajorUnit = 20;
        private const int DefaultMinorUnit = 5;
        private const bool DefaultAutoScale = true;
        #endregion

        #region Fields
        private int _minimum = DefaultMinimum;
        private int _maximum = DefaultMaximum;
        private int _majorUnit = DefaultMajorUnit;
        private int _minorUnit = DefaultMinorUnit;
        private bool _autoScale = DefaultAutoScale;
        private readonly Control _owner;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartValueScaleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartValueScaleSettings(Control owner)
        {
            _owner = owner;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the minimum value of the scale.
        /// </summary>
        [DefaultValue(DefaultMinimum)]
        [Description("Gets/sets a value representing the minimum value of the scale.")]
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum == value) return;
                _minimum = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the maximum value of the scale.
        /// </summary>
        [DefaultValue(DefaultMaximum)]
        [Description("Gets/sets a value representing the maximun value of the scale.")]
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum == value) return;
                _maximum = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the major unit value of the scale.
        /// </summary>
        [DefaultValue(DefaultMajorUnit)]
        [Description("Gets/sets a value representing the major unit value of the scale.")]
        public int MajorUnit
        {
            get => _majorUnit;
            set
            {
                if (_majorUnit == value) return;
                _majorUnit = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing the minor unit value of the scale.
        /// </summary>
        [DefaultValue(DefaultMinorUnit)]
        [Description("Gets/sets a value representing the minor unit value of the scale.")]
        public int MinorUnit
        {
            get => _minorUnit;
            set
            {
                if (_minorUnit == value) return;
                _minorUnit = value;
                _owner.Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets a value representing whether scale parameters are determined automaticaly or not.
        /// </summary>
        [DefaultValue(DefaultAutoScale)]
        [Description("Gets/sets a value representing whether scale parameters are determined automaticaly or not.")]
        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                if (_autoScale == value) return;
                _autoScale = value;
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