using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using Orion.Analytics.Stochastics.SABR;

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// This is data representation of a volatility data grid
    /// The grid will have a 3d structure representing expiries / tenors / strike(s)
    /// ATM Swaptions will have a single strike
    /// </summary>
    [Serializable]
    [XmlRoot("VolatilityMatrix", IsNullable = false)]
    public class SwaptionDataMatrix : SABRDataMatrix
    {
        #region Private Fields

        private List<decimal> _strike;
        private List<string> _tenor;
        private Dictionary<Period, Dictionary<decimal, decimal>> _volatility;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor to use for deserialization
        /// </summary>
        public SwaptionDataMatrix()
        {
            _strike = null;
            _tenor = null;
            _volatility = null;
        }

        /// <summary>
        /// Construct a forward rates matrix from a grid
        /// This uses a default grid name 
        /// </summary>
        /// <param name="tenor">An array of swap tenor terms as strings</param>
        /// <param name="strike">An array of strike terms</param>
        /// <param name="expiry">An expiry term as string</param>
        /// <param name="data">The forward rates for each expiry/tenor pair</param>
        public SwaptionDataMatrix(string[] tenor, decimal[] strike, string expiry, decimal[][] data)
            : this(tenor, strike, expiry, data, "Swaption Volatilities")
        { }

        /// <summary>
        /// Construct a forward rates matrix from a grid
        /// </summary>
        /// <param name="strike">An array of strikes.</param>
        /// <param name="expiry">An array of expiry terms as strings</param>
        /// <param name="tenor">An array of swap tenor terms as strings</param>
        /// <param name="data">The forward rates for each expiry/tenor pair</param>
        /// <param name="matrixId">An id to associate with this array</param>
        public SwaptionDataMatrix(string[] tenor, decimal[] strike, string expiry, decimal[][] data, string matrixId)
            : this()
        {
            // A default id
            id = matrixId;
            dataPoints = new MultiDimensionalPricingData
                             {
                                 point = new PricingStructurePoint[tenor.Length*strike.Length]
                             };
            // Set the points held by the matrix to be the total (expiry x tenor) values
            var point = 0;
            var rateExpiry = PeriodHelper.Parse(expiry);
            // Loop through our expiry/tenor and data arrays to build the matrix points
            for (var row = 0; row < tenor.Length; row++)
            {
                // Convert the expiry to an Interval
                var rateTenor = PeriodHelper.Parse(tenor[row]);
                for (var col = 0; col < strike.Length; col++)
                {
                    var rateStrike = strike[col];
                    // Add the value to our expiry/tenor point
                    dataPoints.point[point] = new PricingStructurePoint
                                                  {
                                                      valueSpecified = true,
                                                      value = data[row][col],
                                                      coordinate = new PricingDataPointCoordinate[1]
                                                  };
                    // Set the value of this point
                    // Set the expiry/tenor coordinate for this point
                    dataPoints.point[point].coordinate[0] = new PricingDataPointCoordinate
                                                                {expiration = new TimeDimension[1]};
                    // Expiry
                    dataPoints.point[point].coordinate[0].expiration[0] = new TimeDimension
                                                                              {Items = new object[] {rateExpiry}};
                    // tenor
                    dataPoints.point[point].coordinate[0].term = new TimeDimension[1];
                    dataPoints.point[point].coordinate[0].term[0] = new TimeDimension
                                                                        {Items = new object[] {rateTenor}};
                    // strike
                    dataPoints.point[point].coordinate[0].strike = new[] { rateStrike };
                    point++;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Return the volatility at this tenor/strike pair
        /// </summary>
        /// <param name="tenor">The swap tenor to use</param>
        /// <param name="strike">The strike to use</param>
        /// <returns></returns>
        public decimal GetVolatility(string tenor, decimal strike)
        {
            return GetVolatility(PeriodHelper.Parse(tenor), strike);
        }

        /// <summary>
        /// Return the volatility at this tenor/strike pair
        /// </summary>
        /// <param name="tenor">The swap tenor to use</param>
        /// <param name="strike">The strike to use</param>
        /// <returns></returns>
        public decimal GetVolatility(Period tenor, decimal strike)
        {
            var temp = InternalGetVolatility(tenor);
            if (temp.ContainsKey(strike))
                return temp[strike];
            throw new ArgumentException("The matrix is malformed. No volatilities are available.");
        }

        /// <summary>
        /// Return the array of volatilities for all strikes that match the supplied tenor
        /// </summary>
        /// <param name="tenor">The swap tenor to use</param>
        /// <returns></returns>
        public decimal[] GetVolatility(string tenor)
        {
            return GetVolatility(PeriodHelper.Parse(tenor));
        }

        /// <summary>
        /// Return the array of volatilities for all strikes that match the supplied tenor
        /// </summary>
        /// <param name="tenor">The swap tenor to use</param>
        /// <returns></returns>
        public decimal[] GetVolatility(Period tenor)
        {
            if (dataPoints == null)
                throw new ArgumentException("The matrix is malformed. No volatilities are available.");
            var temp = InternalGetVolatility(tenor);
            var values = new decimal[temp.Values.Count];
            temp.Values.CopyTo(values, 0);
            return values;
        }

        /// <summary>
        /// Get the expiry associated with this volatility matrix
        /// </summary>
        /// <returns></returns>
        public string GetExpiry()
        {
            if (dataPoints == null)
                throw new ArgumentException("The matrix is malformed. No expiry available.");
            return ((Period)dataPoints.point[0].coordinate[0].expiration[0].Items[0]).ToString();
        }

        /// <summary>
        /// Return the tenors represented in this matrix
        /// </summary>
        /// <returns></returns>
        public string[] GetTenors()
        {
            if (dataPoints == null)
                throw new ArgumentException("The matrix is malformed. No swap tenors available.");
            // If we haven't collected the strikes then we should do so now
            // Once collected we can return them immediately
            if (_tenor == null)
            {
                _tenor = new List<string>();
                // Loop through the points
                foreach (PricingStructurePoint p in dataPoints.point)
                {
                    // Add the next tenor
                    string tenor = ((Period)p.coordinate[0].term[0].Items[0]).ToString();
                    if (!_tenor.Contains(tenor))
                        _tenor.Add(tenor);
                }
            }
            return _tenor.ToArray();
        }

        /// <summary>
        /// Return the strikes maintained by this matrix
        /// </summary>
        /// <returns></returns>
        public decimal[] GetStrikes()
        {
            if (dataPoints == null)
                throw new ArgumentException("The matrix is malformed. No strikes available.");
            // If we haven't collected the strikes then we should do so now
            // Once collected we can return them immediately
            if (_strike == null)
            {
                _strike = new List<decimal>();
                Period tenor = null;
                // Loop through the points
                foreach (PricingStructurePoint p in dataPoints.point)
                {
                    // Limit to only the first tenor
                    if (tenor == null)
                        tenor = (Period)p.coordinate[0].term[0].Items[0];
                    if (!tenor.Equals((Period)p.coordinate[0].term[0].Items[0]))
                        break;
                    // Add the next strike
                    if (!_strike.Contains(p.coordinate[0].strike[0]))
                        _strike.Add(p.coordinate[0].strike[0]);
                }
            }
            return _strike.ToArray();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Return a dictionary of strike/volatility for the given tenor
        /// If the master dictionary doesn't contain the tenor add the vols from the base
        /// </summary>
        /// <param name="tenor"></param>
        /// <returns></returns>
        private Dictionary<decimal, decimal> InternalGetVolatility(Period tenor)
        {            // If there are no vols begin the process - use the IntervalComparer to allow collections to use Interval natively
            if (_volatility == null)
            {
                _volatility = new Dictionary<Period, Dictionary<decimal, decimal>>(new PeriodComparer());
            }
            // If the tenor isn't in the dictionary then move it to the dictionary
            if (!_volatility.ContainsKey(tenor))
            {
                // Add a new entry to our vols for this tenor
                var newTenor = new Dictionary<decimal, decimal>();
                // Loop through the points to retrieve volatilities
                foreach (var p in dataPoints.point)
                {
                    if (!((Period) p.coordinate[0].term[0].Items[0]).Equals(tenor)) continue;
                    var strike = p.coordinate[0].strike[0];
                    if (!newTenor.ContainsKey(strike))
                        newTenor.Add(strike, p.value);
                }
                if (newTenor.Count > 0)
                    _volatility.Add(tenor, newTenor);
                else
                    throw new ArgumentException("The matrix is malformed. No volatilities are available.");
            }
            return _volatility[tenor];
        }

        #endregion
    }
}