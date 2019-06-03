#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the setting for the time axis scale of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartTimeScaleSettings
    {
        #region Consts
        private const ChartTimeUnits defaultBaseUnit = ChartTimeUnits.Days;
        private const int defaultMajorUnit = 7;
        private const int defaultMinorUnit = 1;
        private const bool defaultAutoScale = true;
        #endregion

        #region Fields
        private ChartTimeUnits baseUnit = ChartTimeScaleSettings.defaultBaseUnit;
        private DateTime minimum = DateTime.Today;
        private DateTime maximum = DateTime.Today;
        private int majorUnit = ChartTimeScaleSettings.defaultMajorUnit;
        private int minorUnit = ChartTimeScaleSettings.defaultMinorUnit;
        private bool autoScale = ChartTimeScaleSettings.defaultAutoScale;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartTimeScaleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartTimeScaleSettings(Control owner)
        {
            this.owner = owner;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the base unit of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.defaultBaseUnit)]
        [Description("Gets/sets a value representing the base unit of the scale.")]
        public ChartTimeUnits BaseUnit
        {
            get
            {
                return this.baseUnit;
            }
            set
            {
                if (this.baseUnit != value)
                {
                    this.baseUnit = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the minimum value of the scale.
        /// </summary>
        [Description("Gets/sets a value representing the minimum value of the scale.")]
        public DateTime Minimum
        {
            get
            {
                return this.minimum;
            }
            set
            {
                if (this.minimum != value)
                {
                    this.minimum = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the maximum value of the scale.
        /// </summary>
        [Description("Gets/sets a value representing the maximum value of the scale.")]
        public DateTime Maximum
        {
            get
            {
                return this.maximum;
            }
            set
            {
                if (this.maximum != value)
                {
                    this.maximum = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the major unit value of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.defaultMajorUnit)]
        [Description("Gets/sets a value representing the major unit value of the scale.")]
        public int MajorUnit
        {
            get
            {
                return this.majorUnit;
            }
            set
            {
                if (this.majorUnit != value)
                {
                    this.majorUnit = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing the minor unit value of the scale.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.defaultMinorUnit)]
        [Description("Gets/sets a value representing the minor unit value of the scale.")]
        public int MinorUnit
        {
            get
            {
                return this.minorUnit;
            }
            set
            {
                if (this.minorUnit != value)
                {
                    this.minorUnit = value;
                    this.owner.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets/sets a value representing whether scale parameters are determined automaticaly or not.
        /// </summary>
        [DefaultValue(ChartTimeScaleSettings.defaultAutoScale)]
        [Description("Gets/sets a value representing whether scale parameters are determined automaticaly or not.")]
        public bool AutoScale
        {
            get
            {
                return this.autoScale;
            }
            set
            {
                if (this.autoScale != value)
                {
                    this.autoScale = value;
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