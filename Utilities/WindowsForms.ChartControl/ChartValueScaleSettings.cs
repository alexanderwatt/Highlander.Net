#region References
using System;
using System.ComponentModel;
using System.Windows.Forms;
#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents the setting for the values axis scale of the chart control.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ChartValueScaleSettings
    {
        #region Consts
        private const int defaultMinimum = 0;
        private const int defaultMaximum = 100;
        private const int defaultMajorUnit = 20;
        private const int defaultMinorUnit = 5;
        private const bool defaultAutoScale = true;
        #endregion

        #region Fields
        private int minimum = ChartValueScaleSettings.defaultMinimum;
        private int maximum = ChartValueScaleSettings.defaultMaximum;
        private int majorUnit = ChartValueScaleSettings.defaultMajorUnit;
        private int minorUnit = ChartValueScaleSettings.defaultMinorUnit;
        private bool autoScale = ChartValueScaleSettings.defaultAutoScale;
        private Control owner = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartValueScaleSettings.
        /// </summary>
        /// <param name="owner">A Control object representing the owner chart.</param>
        internal ChartValueScaleSettings(Control owner)
        {
            this.owner = owner;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets a value representing the minimum value of the scale.
        /// </summary>
        [DefaultValue(ChartValueScaleSettings.defaultMinimum)]
        [Description("Gets/sets a value representing the minimum value of the scale.")]
        public int Minimum
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
        /// Gets/sets a value representing the maximun value of the scale.
        /// </summary>
        [DefaultValue(ChartValueScaleSettings.defaultMaximum)]
        [Description("Gets/sets a value representing the maximun value of the scale.")]
        public int Maximum
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
        [DefaultValue(ChartValueScaleSettings.defaultMajorUnit)]
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
        [DefaultValue(ChartValueScaleSettings.defaultMinorUnit)]
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
        [DefaultValue(ChartValueScaleSettings.defaultAutoScale)]
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