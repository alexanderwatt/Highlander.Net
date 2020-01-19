#region References

using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents the setting for the time axis scale of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartTimeScaleSettings
    {
        #region Consts

        private const ChartTimeUnits DefaultBaseUnit = ChartTimeUnits.Days;
        private const int DefaultMajorUnit = 7;
        private const int DefaultMinorUnit = 1;
        private const bool DefaultAutoScale = true;

        #endregion

        #region Fields

        private ChartTimeUnits _baseUnit = ChartTimeScaleSettings.DefaultBaseUnit;
        private DateTime _minimum = DateTime.Today;
        private DateTime _maximum = DateTime.Today;
        private int _majorUnit = DefaultMajorUnit;
        private int _minorUnit = DefaultMinorUnit;
        private bool _autoScale = DefaultAutoScale;
        private readonly Control _owner;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of class ChartTimeScaleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartTimeScaleSettings(Control owner)
        {
            _owner = owner;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the base unit of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.DefaultBaseUnit)]
        [Description("Gets/sets a value representing the base unit of the scale.")]
        public ChartTimeUnits BaseUnit
        {
            get => _baseUnit;
            set
            {
                if (_baseUnit != value)
                {
                    _baseUnit = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the minimum value of the scale.
        /// </summary>
        [Description("Gets/sets a value representing the minimum value of the scale.")]
        public DateTime Minimum
        {
            get => _minimum;
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the maximum value of the scale.
        /// </summary>
        [Description("Gets/sets a value representing the maximum value of the scale.")]
        public DateTime Maximum
        {
            get => _maximum;
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the major unit value of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.DefaultMajorUnit)]
        [Description("Gets/sets a value representing the major unit value of the scale.")]
        public int MajorUnit
        {
            get => _majorUnit;
            set
            {
                if (_majorUnit != value)
                {
                    _majorUnit = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the minor unit value of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.DefaultMinorUnit)]
        [Description("Gets/sets a value representing the minor unit value of the scale.")]
        public int MinorUnit
        {
            get => _minorUnit;
            set
            {
                if (_minorUnit != value)
                {
                    _minorUnit = value;
                    _owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing whether scale parameters are determined automatically or not.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.DefaultAutoScale)]
        [Description("Gets/sets a value representing whether scale parameters are determined automaticaly or not.")]
        public bool AutoScale
        {
            get => _autoScale;
            set
            {
                if (_autoScale != value)
                {
                    _autoScale = value;
                    _owner.Invalidate();
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
            return "(" + GetType().Name + ")";
        }

        #endregion
    }
}