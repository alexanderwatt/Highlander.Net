#region References

using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using static System.DateTime;
using static System.Decimal;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents a value from the time series of the chart control.
    /// </summary>
    [TypeConverter(typeof(ChartSeriesValueTypeConverter))]
    public class ChartSeriesValue
    {
        #region Fields

        private DateTime _date = Today;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of class ChartSeriesValue.
        /// </summary>
        public ChartSeriesValue()
        {
            // nothing
        }

        /// <summary>
        /// Initializes a new instance of class ChartSeriesValue.
        /// </summary>
        /// <param name="date">A DateTime object representing the date for the given value.</param>
        /// <param name="value">A Decimal value representing the value to be drawn on the chart.</param>
        public ChartSeriesValue(DateTime date, decimal value)
        {
            _date = date;
            Value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets/sets the date for the given value.
        /// </summary>
        [Description("Gets/sets the date for the given value.")]
        public DateTime Date
        {
            get => _date;
            set => _date = value;
        }

        /// <summary>
        /// Gets/sets the value to be drawn on the chart.
        /// </summary>
        [Description("Gets/sets the value to be drawn on the chart.")]
        public decimal Value { get; set; } = Zero;

        #endregion

        #region Methods
        /// <summary>
        /// Returns a string that represents the current object.  
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Value.ToString(CultureInfo.InvariantCulture) + " / " + _date.ToShortDateString();
        }
        #endregion

        #region ChartSeriesValueTypeConverter
        /// <summary>
        /// ChartSeriesValue Type Converter class.
        /// </summary>
        internal class ChartSeriesValueTypeConverter : TypeConverter 
        {
            /// <summary>
            /// Method used to define if object can be converted to specific type.
            /// </summary>
            public override bool CanConvertTo(ITypeDescriptorContext context, Type destType)
            {
                // check if InstanceDescriptor (used by IDE to serialize the values)
                if (destType == typeof(InstanceDescriptor))
                {
                    // override to convert to InstanceDescriptor
                    return true;
                }
                // call base class for other types
                return base.CanConvertTo(context, destType);
            }

            /// <summary>
            /// Method used to do the conversion.
            /// </summary>
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType) 
            {
                // check if InstanceDescriptor (used by IDE to serialize the values)
                if (destType == typeof(InstanceDescriptor)) 
                {
                    ChartSeriesValue seriesValue = (ChartSeriesValue)value;
                    // use the default constructor
                    return new InstanceDescriptor(typeof(ChartSeriesValue).GetConstructor(new Type[] {typeof(DateTime), typeof(Decimal)}),
                                                  new object[] {seriesValue.Date, seriesValue.Value},
                                                  false);
                }
                // call base class for other types
                return base.ConvertTo(context, culture, value, destType);
            }
        }

        #endregion
    }
}