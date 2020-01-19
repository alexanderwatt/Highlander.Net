using System;
using System.Collections.Generic;
using System.Linq;
using Orion.EquityCollarPricer.Helpers;

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Base Zero rate Curve
    /// </summary>
    public abstract class ZeroCurveBase
    {
        /// <summary>
        /// The curve date
        /// </summary>
        protected DateTime DTCurveDate;

        /// <summary>
        /// The Curve Currency
        /// </summary>
        protected string _currencyCode;


        /// <summary>
        /// Gets the zero rates.
        /// </summary>
        /// <value>The zero rates.</value>
        public ZeroRate[] ZeroRates { get; protected set; }

        /// <summary>
        /// Gets the tenors.
        /// </summary>
        /// <value>The tenors.</value>
        public DateTime[] Tenors
        {
            get 
            {
                var rates = new List<ZeroRate>(ZeroRates);
                return PropertyHelper.GetProperties<DateTime, ZeroRate>(rates, "TenorDate");
            }
        }

        /// <summary>
        /// Gets the rates.
        /// </summary>
        /// <value>The rates.</value>
        public Double[] Rates
        {
            get
            {
                var rates = new List<ZeroRate>(ZeroRates);
                return PropertyHelper.GetProperties<Double, ZeroRate>(rates, "Rate");
            }
        }

        /// <summary>
        /// Gets or sets the curve date.
        /// </summary>
        /// <value>The curve date.</value>
        public DateTime CurveDate
        {
            get { return DTCurveDate; }
            set 
            {
                InputValidator.NotNull("Curve Date", value, true);
                DTCurveDate = value; 
            
            }
        }

        /// <summary>
        /// Gets or sets the currency code.
        /// </summary>
        /// <value>The currency code.</value>
        public string CurrencyCode
        {
            get { return _currencyCode; }
            set 
            {
                InputValidator.IsMissingField("Curve Currency Code", value, true);
                _currencyCode = value; 
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroCurveBase"/> class.
        /// </summary>
        /// <param name="tenorDates">The tenor dates.</param>
        /// <param name="zeroRates">The zero rates.</param>
        protected ZeroCurveBase(IEnumerable<DateTime> tenorDates, List<Double> zeroRates)
        {
            ZeroRates = CreateZeroRates(tenorDates, zeroRates);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroCurveBase"/> class.
        /// </summary>
        /// <param name="curveDate">The curve date.</param>
        /// <param name="tenorDates">The tenor dates.</param>
        /// <param name="zeroRates">The zero rates.</param>
        protected ZeroCurveBase(DateTime curveDate, IEnumerable<DateTime> tenorDates, List<Double> zeroRates)
        {
            InputValidator.NotNull("Curve Date", curveDate, true);
            DTCurveDate = curveDate;
            ZeroRates = CreateZeroRates(tenorDates, zeroRates);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ZeroCurveBase"/> class.
        /// </summary>
        /// <param name="curveDate">The curve date.</param>
        /// <param name="curveCurrencyCode">The curve currency code.</param>
        /// <param name="tenorDates">The tenor dates.</param>
        /// <param name="zeroRates">The zero rates.</param>
        protected ZeroCurveBase(DateTime curveDate, string curveCurrencyCode, IEnumerable<DateTime> tenorDates, List<Double> zeroRates)
        {
            InputValidator.NotNull("Curve Date", curveDate, true);
            InputValidator.IsMissingField("Curve Currency Code", curveCurrencyCode, true);
            DTCurveDate = curveDate;
            _currencyCode = curveCurrencyCode;
            ZeroRates = CreateZeroRates(tenorDates, zeroRates);
        }

        /// <summary>
        /// Creates the zero rates.
        /// </summary>
        /// <param name="tenorDates">The tenor dates.</param>
        /// <param name="zeroRates">The zero rates.</param>
        /// <returns></returns>
        private ZeroRate[] CreateZeroRates(IEnumerable<DateTime> tenorDates, List<Double> zeroRates)
        {
            var rates = tenorDates.Select((t, index) => new ZeroRate(t, zeroRates[index])).ToList();
            // sort the list in tenor date order
            rates.Sort((rate1, rate2) => rate1.TenorDate.CompareTo(rate2.TenorDate));
            ZeroRate[] ratesArray = { };
            if (rates.Count > 0)
            {
                ratesArray = new ZeroRate[rates.Count];
                rates.CopyTo(ratesArray, 0);
            }
            return ratesArray;
        }
    }
}
