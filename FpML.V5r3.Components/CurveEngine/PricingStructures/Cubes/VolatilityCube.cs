#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Constants;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolators;
using Orion.Analytics.DayCounters;
using FpML.V5r3.Codes;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.CurveEngine.PricingStructures.Cubes
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class VolatilityCube : PricingStructureBase, IVolatilityCube
    {
        #region Private Fields

        /// <summary>
        /// An auxilliary structure used to track indexes within the volatility surface.
        /// This should make lookups faster than the default linear search.
        /// </summary>
        private readonly SortedList<ExpiryTenorStrikeKey, int> _matrixIndexHelper;

        private readonly IInterpolation _interpolation = new TrilinearInterpolator();

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a VolatilityCube from points
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="points"></param>
        public VolatilityCube(NamedValueSet properties, PricingStructurePoint[] points)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            PricingStructureIdentifier = new VolatilitySurfaceIdentifier(properties);
            var surfaceId = (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Rates);//TODO Need to set for the different underlyers.
            SetInterpolator();
            foreach (PricingStructurePoint point in points)
            {
                point.underlyingAssetReference = surfaceId.UnderlyingAssetReference;
                point.quoteUnits = surfaceId.StrikeQuoteUnits;
            }
            PricingStructure = new VolatilityRepresentation
            {
                name = surfaceId.Name,
                id = surfaceId.Id,
                currency = surfaceId.Currency,
                asset = new AnyAssetReference { href = surfaceId.Instrument },
            };
            var datapoints = new MultiDimensionalPricingData { point = points };
            PricingStructureValuation = new VolatilityMatrix
            {
                dataPoints = datapoints,
                objectReference = new AnyAssetReference { href = surfaceId.Instrument },
                baseDate = new IdentifiedDate { Value = surfaceId.BaseDate },
                buildDateTime = DateTime.Now,
                buildDateTimeSpecified = true
            };
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        /// <summary>
        /// Construct a VolatilityCube
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="expiryTerms"></param>
        /// <param name="tenors"></param>
        /// <param name="volatilities"></param>
        /// <param name="strikes"></param>
        public VolatilityCube(NamedValueSet properties, string[] expiryTerms, string[] tenors, decimal[,] volatilities, decimal[] strikes)
        {
            properties.Set(EnvironmentProp.Function, FunctionProp.Market.ToString());
            PricingStructureIdentifier = new VolatilitySurfaceIdentifier(properties);
            var surfaceId = (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Rates);//TODO Need to set for the different underlyers.
            SetInterpolator();
            var points = ProcessRawSurface(expiryTerms, tenors, strikes, volatilities, surfaceId.StrikeQuoteUnits, surfaceId.UnderlyingAssetReference);
            PricingStructure = new VolatilityRepresentation
                                   {
                                       name = surfaceId.Name,
                                       id = surfaceId.Id,
                                       currency = surfaceId.Currency,
                                       asset = new AnyAssetReference { href = surfaceId.Instrument },
                                   };
            var datapoints = new MultiDimensionalPricingData { point = points };
            PricingStructureValuation = new VolatilityMatrix
                                            {
                                                dataPoints = datapoints,
                                                objectReference = new AnyAssetReference { href = surfaceId.Instrument },
                                                baseDate = new IdentifiedDate { Value = surfaceId.BaseDate },
                                                buildDateTime = DateTime.Now,
                                                buildDateTimeSpecified = true
                                            };
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="fpmlData"></param>
        public VolatilityCube(Pair<PricingStructure, PricingStructureValuation> fpmlData)
            : base(fpmlData)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Rates);//TODO Need to set for the different underlyers.
            SetInterpolator();
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="fpmlData"></param>
        /// <param name="properties"></param>
        public VolatilityCube(Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties)
            : base(fpmlData)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, Constants.AssetClass.Rates);//TODO Need to set for the different underlyers.
            PricingStructureIdentifier = new VolatilitySurfaceIdentifier(properties);
            SetInterpolator();
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        private void SetInterpolator()
        {
            IDayCounter dc = Actual365.Instance;
            var termPoints = new List<TermPoint>
                                 {
                                     TermPointFactory.Create(1.0m, new DateTime()),
                                     TermPointFactory.Create(0.99m, new DateTime().AddDays(10)),
                                     TermPointFactory.Create(0.97m, new DateTime().AddDays(100))
                                 };
            var termCurve = TermCurve.Create(new DateTime(), new InterpolationMethod { Value = "LinearInterpolation" }, true, termPoints);

            Interpolator = new TermCurveInterpolator(termCurve, new DateTime(), dc);
        }

        #endregion

        #region IPricingStructure Members

        /// <summary>
        /// Gets the VolatilityMatrix.
        /// </summary>
        /// <returns></returns>
        private VolatilityMatrix VolatilityMatrix
        {
            get { return (VolatilityMatrix)PricingStructureValuation; }
        }

        /// <summary>
        /// This call returns an IValue structure for an exact match with a given point.
        /// If the point does not exist this method will return null.
        /// </summary>
        /// <param name="pt">The point in the surface/cube to get the value of</param>
        /// <returns>An IValue object representing the value/coordinates of the point</returns>
        public override IValue GetValue(IPoint pt)
        {
            string expiry = ((Period)pt.Coords[0]).ToString();
            string tenor = ((Period)pt.Coords[1]).ToString();
            decimal strike = Convert.ToDecimal(pt.Coords[2]);

            var key = new ExpiryTenorStrikeKey(expiry, tenor, strike);
            if (_matrixIndexHelper.ContainsKey(key))
            {
                int idx = _matrixIndexHelper[key];
                decimal volatility = VolatilityMatrix.dataPoints.point[idx].value;
                string id = VolatilityMatrix.dataPoints.point[idx].id;
                return new VolatilityValue(id, volatility, pt);
            }
            return null;
        }

        /// <summary>
        /// Locate bounding points for a point.
        /// This method uses distance to find candidates then builds a list of
        /// <see cref="Point2D"/> for a surface
        /// or <see cref="Point3D"/> for a cube
        /// that can be passed to the default (internal) interpolator.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns>The interpolated value of the point</returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            // Test each dimensiuon for the two closest points
            // then build a set of points from each dimension point

            var expiryOrigin = ((Period)((Coordinate)pt).PricingDataCoordinate.expiration[0].Items[0]).ToYearFraction();
            var termOrigin = ((Period)((Coordinate)pt).PricingDataCoordinate.term[0].Items[0]).ToYearFraction();
            var strikeOrigin = (double)((Coordinate)pt).PricingDataCoordinate.strike[0];

            return GetClosestValues(expiryOrigin, termOrigin, strikeOrigin);
        }

        private IList<IValue> GetClosestValues(double expiryOrigin, double termOrigin, double strikeOrigin)
        {
            var expiries = new SortedList<double, List<string>>();
            var tenors = new SortedList<double, List<string>>();
            var strikes = new SortedList<double, List<string>>();

            foreach (var key in _matrixIndexHelper.Keys)
            {
                var expiryPoint = key.Expiry.ToYearFraction();
                var termPoint = key.Tenor.ToYearFraction();
                var strikePoint = (double)key.Strike;

                var expiryKeyPart = key.Expiry.ToString();
                var termKeyPart = key.Tenor.ToString();
                var strikeKeyPart = key.Strike.ToString(CultureInfo.InvariantCulture);

                // Test closest expiry - keep closest 2
                var expiryDistance = System.Math.Abs(expiryOrigin - expiryPoint);
                UpdateKeyPartList(ref expiries, expiryDistance, expiryKeyPart);

                // Test closest tenor - keep closest 2
                var tenorDistance = System.Math.Abs(termOrigin - termPoint);
                UpdateKeyPartList(ref tenors, tenorDistance, termKeyPart);

                // Test closest strike - keep closest 2
                var strikeDistance = System.Math.Abs(strikeOrigin - strikePoint);
                UpdateKeyPartList(ref strikes, strikeDistance, strikeKeyPart);
            }
            // We now have the 2 closest expiries/terms and strikes so we build keys for them
            var c = UnpackKeysAndMakeCoordinates(expiries, tenors, strikes);
            return c.Select(GetValue).ToList();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string GetAlgorithm()
        {
            return "Linear";
        }

        #endregion

        #region IPointFunction Members

        /// <summary>
        /// This method uses the supplied Interpolation function to provide a value for a given point
        /// </summary>
        /// <param name="point">A point to get a value for</param>
        /// <returns></returns>
        public override double Value(IPoint point)
        {
            var val = GetValue(point);

            if (val != null)
            {
                return Convert.ToDouble(val.Value);
            }

            var values = (List<IValue>)GetClosestValues(point);
            IPoint pt = ToDoublePoint3D(point);
            return Value(pt, values);
        }

        private double Value(double expiry, double term, double strike)
        {
            var values = (List<IValue>)GetClosestValues(expiry, term, strike);
            IPoint point = new Point3D(strike, term , expiry);
            return Value(point, values);
        }

        private double Value(IPoint point, IEnumerable<IValue> values)
        {
            var bounds = (from VolatilityValue value in values select ToDoublePoint3D(value)).Cast<IPoint>().ToList();

            return ((Interpolator)_interpolation).Value(point, bounds);
        }

        #endregion

        #region Private Methods

        #region DoublePoint Conversions

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represent year fractions expiry and tenor, and a strike
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static Point3D ToDoublePoint3D(VolatilityValue val)
        {
            var expiry = (Period)val.PricePoint.coordinate[0].expiration[0].Items[0];
            var tenor = (Period)val.PricePoint.coordinate[0].term[0].Items[0];
            decimal strike = val.PricePoint.coordinate[0].strike[0];
            double value = Convert.ToDouble(val.Value);
            return new Point3D((double)strike, tenor.ToYearFraction(), expiry.ToYearFraction(), value);
        }

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represent year fractions expiry and tenor, and a strike
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        private static Point3D ToDoublePoint3D(IPoint coord)
        {
            var expiry = (Period)((Coordinate)coord).PricingDataCoordinate.expiration[0].Items[0];
            var tenor = (Period)((Coordinate)coord).PricingDataCoordinate.term[0].Items[0];
            var strike = ((Coordinate)coord).PricingDataCoordinate.strike[0];
            const double value = 0;
            return new Point3D((double)strike, tenor.ToYearFraction(), expiry.ToYearFraction(), value);
        }

        #endregion

        /// <summary>
        /// Unpack the keyt lists and create a set of coordinates from them
        /// </summary>
        /// <param name="expiry">The list of expiry terms</param>
        /// <param name="tenor">The list of tenor terms</param>
        /// <param name="strike">The list of strikes</param>
        /// <returns>An array of Coordinate</returns>
        private static IEnumerable<Coordinate> UnpackKeysAndMakeCoordinates(
            SortedList<double, List<string>> expiry,
            SortedList<double, List<string>> tenor, 
            SortedList<double, List<string>> strike)
        {
            var expiryKeys = new string[2];
            var tenorKeys = new string[2];
            var strikeKeys = new string[2];

            // Unpack the expiries
            if (expiry[expiry.Keys[0]].Count == 2)
            {
                // both points are equidistant
                expiryKeys[0] = expiry[expiry.Keys[0]][0];
                expiryKeys[1] = expiry[expiry.Keys[0]][1]; 
            }
            else
            {
                expiryKeys[0] = expiry[expiry.Keys[0]][0];
                expiryKeys[1] = expiry[expiry.Keys[1]][0];
            }

            // Unpack the strikes
            if (strike[strike.Keys[0]].Count == 2)
            {
                // both points are equidistant
                strikeKeys[0] = strike[strike.Keys[0]][0];
                strikeKeys[1] = strike[strike.Keys[0]][1];
            }
            else
            {
                strikeKeys[0] = strike[strike.Keys[0]][0];
                if (strike.Keys.Count == 1)
                    strikeKeys[1] = strike[strike.Keys[0]][0];
                else
                    strikeKeys[1] = strike[strike.Keys[1]][0];
            }

            // Unpack the tenors
            if (tenor[tenor.Keys[0]].Count == 2)
            {
                // both points are equidistant
                tenorKeys[0] = tenor[tenor.Keys[0]][0];
                tenorKeys[1] = tenor[tenor.Keys[0]][1];
            }
            else
            {
                tenorKeys[0] = tenor[tenor.Keys[0]][0];

                if (tenor.Keys.Count == 1)
                    tenorKeys[1] = tenor[tenor.Keys[0]][0];
                else
                    tenorKeys[1] = tenor[tenor.Keys[1]][0];
            }
            var coordinates = new List<Coordinate>
                                  {
                                      new Coordinate(expiryKeys[0], tenorKeys[0], strikeKeys[0]),
                                      new Coordinate(expiryKeys[0], tenorKeys[0], strikeKeys[1]),
                                      new Coordinate(expiryKeys[0], tenorKeys[1], strikeKeys[0]),
                                      new Coordinate(expiryKeys[0], tenorKeys[1], strikeKeys[1]),
                                      new Coordinate(expiryKeys[1], tenorKeys[0], strikeKeys[0]),
                                      new Coordinate(expiryKeys[1], tenorKeys[0], strikeKeys[1]),
                                      new Coordinate(expiryKeys[1], tenorKeys[1], strikeKeys[0]),
                                      new Coordinate(expiryKeys[1], tenorKeys[1], strikeKeys[1])
                                  };

            return coordinates.ToArray();
        }

        /// <summary>
        /// Conditionally update a sorted list of lists with a candidate value.
        /// This method is a specialised variant that is configured to have a maximum of
        /// two elements containing at most two sub-elements.
        /// The lists are only meant to retain a top(2) of all possible values
        /// Recall that keys are meant to be unique so extra checking must be carried out
        /// on both elements and sub-elements.
        /// </summary>
        /// <param name="list">The keyPart list to update</param>
        /// <param name="distance">The key to check</param>
        /// <param name="keyPart">The keyPart to add to the keyPart list</param>
        private static void UpdateKeyPartList(ref SortedList<double, List<string>> list, double distance, string keyPart)
        {
            // Check the list has reached its maximum
            if (list.Count == 2)
            {
                // Check the new key is a valid list entry
                if (distance < list.Keys[1])
                {
                    if (list.ContainsKey(distance))
                    {
                        // If we have an entry then check it's not a duplicate - then add it
                        if (!list[distance].Contains(keyPart))
                        {
                            list[distance].Add(keyPart);
                        }
                    }
                    else
                    {
                        list.Remove(list.Keys[1]);
                        var keypartList = new List<string> { keyPart };
                        list.Add(distance, keypartList);
                    }
                }
                else if (list.ContainsKey(distance))
                {
                    // If we have an entry then check it's not a duplicate - then add it
                    if (!list[distance].Contains(keyPart))
                    {
                        list[distance].Add(keyPart);
                    }
                }
            }
            else if (list.Count < 2)
            {
                if (list.ContainsKey(distance))
                {
                    // If we have an entry then check it's not a duplicate - then add it
                    if (!list[distance].Contains(keyPart))
                    {
                        list[distance].Add(keyPart);
                    }
                }
                else
                {
                    var keypartList = new List<string> {keyPart};
                    list.Add(distance, keypartList);
                }
            }
        }

        /// <summary>
        /// Record the volatilities in the key helper and counts
        /// </summary>
        private void ProcessVolatilityRepresentation()
        {
            int index = 0;
            foreach (var point in VolatilityMatrix.dataPoints.point)
            {
                var expiry = (Period)point.coordinate[0].expiration[0].Items[0];
                var tenor = (Period)point.coordinate[0].term[0].Items[0];
                var strike = point.coordinate[0].strike[0];
                _matrixIndexHelper.Add(new ExpiryTenorStrikeKey(expiry, tenor, strike), index);
                index++;
            }
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// A class to model an expiry/term key pair.
        /// If a surface has only an expiry dimension then the term (tenor) will
        /// default to null.
        /// The class implements the IComparer&lt;ExpiryTenorpairKey&gt;.Compare() method
        /// to keep track of indexes and their keys.
        /// </summary>
        private class ExpiryTenorStrikeKey : IComparer<ExpiryTenorStrikeKey>
        {
            private readonly Period _expiry;
            private readonly Period _tenor;
            private readonly decimal _strike;

            #region Properties

            public Period Expiry { get { return _expiry; } }
            public Period Tenor { get { return _tenor; } }
            public decimal Strike { get { return _strike; } }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor for use as a Comparer
            /// </summary>
            public ExpiryTenorStrikeKey()
            {
            }

            /// <summary>
            /// Use the Expiry and Tenor to create a key.
            /// Expiry or tenor can be null but not both.
            /// </summary>
            /// <param name="expiry">Expiry key part</param>
            /// <param name="tenor">Tenor key part (can be null)</param>
            /// <param name="strike">Strike key pary</param>
            public ExpiryTenorStrikeKey(string expiry, string tenor, decimal strike)
            {
                try
                {
                    _expiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
                }
                catch (System.Exception)
                {
                    _expiry = null;
                }

                try
                {
                    _tenor = tenor != null ? PeriodHelper.Parse(tenor) : null;
                }
                catch (System.Exception)
                {
                    _tenor = null;
                }

                _strike = strike;
            }

            /// <summary>
            /// Create a key from a Coordinate
            /// </summary>
            /// <param name="expiry"></param>
            /// <param name="tenor"></param>
            /// <param name="strike"></param>
            public ExpiryTenorStrikeKey(Period expiry, Period tenor, decimal strike)
            {
                _expiry = expiry;
                _tenor = tenor;
                _strike = strike;
            }

            #endregion

            #region IComparer<ExpiryTenorStrikeKey> Members

            /// <summary>
            /// if x &lt; y return -1
            /// if x &gt; y return  1
            /// if x = y    return  0
            /// </summary>
            /// <param name="x">The key - x</param>
            /// <param name="y">The key - y</param>
            /// <returns>An indication of the relative positions of x and y</returns>
            public int Compare(ExpiryTenorStrikeKey x, ExpiryTenorStrikeKey y)
            {
                // Test x and y as intervals Expiry before Tenor?
                var xExpiryAsInterval = x.Expiry;
                var xTenorAsInterval = x.Tenor;
                var yExpiryAsInterval = y.Expiry;
                var yTenorAsInterval = y.Tenor;
                var xStrike = x.Strike;
                var yStrike = y.Strike;

                // Test equality
                if (IntervalHelper.Less(xExpiryAsInterval, yExpiryAsInterval))
                    return -1;
                if (IntervalHelper.Greater(xExpiryAsInterval, yExpiryAsInterval))
                    return 1;
                if (IntervalHelper.Less(xTenorAsInterval, yTenorAsInterval))
                    return -1;
                if (IntervalHelper.Greater(xTenorAsInterval, yTenorAsInterval))
                    return 1;
                if (xStrike < yStrike)
                    return -1;
                return xStrike > yStrike ? 1 : 0;
            }

            #endregion
        }

        #endregion

        private static PricingStructurePoint[] ProcessRawSurface(string[] expiries, string[] tenors, decimal[] strikes, decimal[,] volatility, PriceQuoteUnits strikeQuoteUnits, AssetReference underlyingAssetReference)
        {
            var expiryLength = expiries.Length;
            var strikeLength = strikes.Length;
            var tenorLength = tenors.Length;
            var points = new List<PricingStructurePoint>();
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                for (var strikeIndex = 0; strikeIndex < strikeLength; strikeIndex++)
                {
                    for (var tenorIndex = 0; tenorIndex < tenorLength; tenorIndex++)
                    {
                        // Extract the row,column indexed volatility
                        var vol = volatility[expiryIndex + tenorIndex * expiryLength, strikeIndex];

                        // Add the value to the points array (dataPoints entry in the matrix)
                        var coordinates = new PricingDataPointCoordinate[1];
                        coordinates[0] = PricingDataPointCoordinateFactory.Create(expiries[expiryIndex], tenors[tenorIndex],
                                                                                  strikes[strikeIndex]);
                        var point = new PricingStructurePoint
                                        {
                                            value = vol,
                                            valueSpecified = true,
                                            coordinate = coordinates,
                                            underlyingAssetReference = underlyingAssetReference,
                                            quoteUnits = strikeQuoteUnits
                                        };
                        points.Add(point);
                    }
                }
            }
            return points.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expiryInterval"></param>
        /// <param name="termInterval"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        public double GetValue(string expiryInterval, string termInterval, decimal strike)
        {
            var coordinate = new Coordinate(expiryInterval, termInterval, strike);
            return Value(coordinate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expiryInterval"></param>
        /// <param name="termInterval"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        public double GetValue(string expiryInterval, string termInterval, double strike)
        {
            return GetValue(expiryInterval, termInterval, (decimal) strike);
        }

        ///<summary>
        ///</summary>
        ///<param name="baseDate"></param>
        ///<param name="expirationAsDate"></param>
        ///<param name="maturityYearFraction"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public double GetValue(DateTime baseDate, DateTime expirationAsDate, double maturityYearFraction, decimal strike)
        {
            double expiryYearFraction = DayCounterHelper.ToDayCounter(DayCountFractionEnum.ACT_365_FIXED).YearFraction(baseDate, expirationAsDate);

            return Value(expiryYearFraction, maturityYearFraction, (double)strike);
        }
    }
}