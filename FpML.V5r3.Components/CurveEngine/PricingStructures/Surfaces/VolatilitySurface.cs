/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.Interpolators;
using Orion.Analytics.DayCounters;
using Orion.Identifiers;
using Orion.ModelFramework.PricingStructures;
using Orion.Util.Helpers;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.PricingStructures.Surfaces
{
    /// <summary>
    /// A class to wrap a VolatilityMatrix, VolatilityRepresentation pair
    /// </summary>
    public class VolatilitySurface : PricingStructureBase
    {
        #region Private Fields

        /// <summary>
        /// An auxiliary structure used to track indexes within the volatility surface.
        /// This should make lookups faster than the default linear search.
        /// </summary>
        private readonly SortedList<ExpiryTenorStrikeKey, int> _matrixIndexHelper;

        private readonly string _algorithm;

        private readonly IInterpolation _interpolation;

        /// <summary>
        /// Auxiliary counts of the inputs
        /// </summary>
        private int _matrixRowCount;
        private int _matrixColumnCount;

        #endregion

        #region Constructors

        /// <summary>
        /// Unpack a raw surface and create a VolatilitySurface
        /// The object array is assumed to be zero based when it is passed to the constructor
        /// (That is any Excel idiosyncrasies have been expunged)
        /// We'll test and modify if necessary to zero base the array
        /// </summary>
        /// <param name="rawSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="date"></param>
        //       /// <param name="algorithm">The algorithm for interpolation. Not implemented yet.</param>
        public VolatilitySurface(object[,] rawSurface, VolatilitySurfaceIdentifier surfaceId, DateTime date)
            : this(rawSurface, surfaceId, date, DateTime.Now)
        {
        }


        /// <summary>
        /// Unpack a raw surface and create a VolatilitySurface
        /// The object array is assumed to be zero based when it is passed to the constructor
        /// (That is any Excel idiosyncrasies have been expunged)
        /// We'll test and modify if necessary to zero base the array
        /// </summary>
        /// <param name="rawSurface"></param>
        /// <param name="surfaceId"></param>
        /// <param name="date"></param>
        /// <param name="buildDateTime"></param>
        //       /// <param name="algorithm">The algorithm for interpolation. Not implemented yet.</param>
        public VolatilitySurface(object[,] rawSurface, VolatilitySurfaceIdentifier surfaceId, DateTime date, DateTime buildDateTime)
        {
            PricingStructureIdentifier = surfaceId;
            IDayCounter dc = Actual365.Instance;
            var termPoints = new List<TermPoint>
                                 {
                                     TermPointFactory.Create(1.0m, new DateTime()),
                                     TermPointFactory.Create(0.99m, new DateTime().AddDays(10)),
                                     TermPointFactory.Create(0.97m, new DateTime().AddDays(100))
                                 };
            var termCurve = TermCurve.Create(new DateTime(), new InterpolationMethod { Value = "LinearInterpolation" }, true, termPoints);
            Interpolator = new TermCurveInterpolator(termCurve, date, dc);//TODO need to create a surfaceinterpolator.
            var zeroedRawSurface = rawSurface.GetLowerBound(0) == 1 ? RedimensionRawSurface(rawSurface) : rawSurface;
            _algorithm = "Linear";
            // An ugly trick to find out if this is a cube or a surface
            bool isCube = !double.TryParse(zeroedRawSurface[1, 1].ToString(), out _);
            // Extract the strikes/tenors/expiries and build the surface
            var expiry = ExtractExpiryFromRawSurface(zeroedRawSurface);
            var term = ExtractTenorFromRawSurface(zeroedRawSurface, isCube);
            var strike = ExtractStrikeFromRawSurface(zeroedRawSurface, isCube);
            var volatility = ExtractVolatilitiesFromRawSurface(zeroedRawSurface, isCube);
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            var points = ProcessRawSurface(expiry, term, strike, volatility);
            PricingStructure = new VolatilityRepresentation
                                   {
                                       name = surfaceId.Name,
                                       id = surfaceId.Id,
                                       asset = new AnyAssetReference {href = "Unknown"},
                                   };
            PricingStructureValuation
                = new VolatilityMatrix
                      {
                          dataPoints = new MultiDimensionalPricingData {point = points},
                          objectReference = new AnyAssetReference {href = PricingStructure.id},
                          baseDate = new IdentifiedDate {Value = date},
                          buildDateTime = buildDateTime,
                          buildDateTimeSpecified = true
                      };
            // Record the row/column sizes of the inputs
            _matrixRowCount = expiry.Length;
            _matrixRowCount *= term?.Length ?? 1;
            // Columns includes expiry and term (tenor) if it exists.
            _matrixColumnCount = strike.Length + 1;
            _matrixColumnCount += term != null ? 1 : 0;
            // Generate an interpolator to use
            if (term == null || term.Length == 0)
                _interpolation = new BilinearInterpolator();
            else
                _interpolation = new TrilinearInterpolator();
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="assetRef">The Asset this volatility models</param>
        /// <param name="name">The id to use with this matrix</param>
        /// <param name="date">The value date relating to this surface</param>
        /// <param name="expiry">An array of expiry definitions</param>
        /// <param name="term">An array of tenors or null if there is no term dimension.</param>
        /// <paparam name="strike">An array of strike descriptions</paparam>
        /// <param name="strike">The strike array.</param>
        /// <param name="volatility">A 2d array of volatilities.
        /// This must be equal to (expiry.Count x (1 &lt;= y &lt;= term.Count) x strike.Count </param>
        public VolatilitySurface(string assetRef, string name, DateTime date, string[] expiry, string[] term,
                                 string[] strike, double[,] volatility)
        {
            IDayCounter dc = Actual365.Instance;
            var termPoints = new List<TermPoint>
                                 {
                                     TermPointFactory.Create(1.0m, new DateTime()),
                                     TermPointFactory.Create(0.99m, new DateTime().AddDays(10)),
                                     TermPointFactory.Create(0.97m, new DateTime().AddDays(100))
                                 };
            var termCurve = TermCurve.Create(new DateTime(), new InterpolationMethod { Value = "LinearInterpolation" }, true, termPoints);
            Interpolator = new TermCurveInterpolator(termCurve, date, dc);//TODO need to create a surfaceinterpolator.
            _algorithm = "Linear";
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            var points = ProcessRawSurface(expiry, term, strike, volatility);
            PricingStructure = new VolatilityRepresentation { name = name
                                                              ,
                                                              id = name + date.ToString("yyyyMMdd")
                                                              ,
                                                              asset = new AnyAssetReference { href = assetRef }
                                                            };
            PricingStructureValuation = new VolatilityMatrix
                                            {
                                                dataPoints = new MultiDimensionalPricingData { point = points }
                                                ,
                                                objectReference = new AnyAssetReference { href = PricingStructure.id }
                                                ,
                                                baseDate = new IdentifiedDate { Value = date }
                                                ,
                                                buildDateTime = DateTime.Now
                                                ,
                                                buildDateTimeSpecified = true};

            // Record the row/column sizes of the inputs
            _matrixRowCount = expiry.Length;
            _matrixRowCount *= term?.Length ?? 1;
            // Columns includes expiry and term (tenor) if it exists.
            _matrixColumnCount = strike.Length + 1;
            _matrixColumnCount += term != null ? 1 : 0;
            //TODO
            // Generate an interpolator to use
            if (term == null || term.Length == 0)
                _interpolation = new BilinearInterpolator();
            else
                _interpolation = new TrilinearInterpolator();
        }

        /// <summary>
        /// Create a surface from an FpML 
        /// </summary>
        /// <param name="fpmlData"></param>
        public VolatilitySurface(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            IDayCounter dc = Actual365.Instance;
            var termPoints = new List<TermPoint>
                                 {
                                     TermPointFactory.Create(1.0m, new DateTime()),
                                     TermPointFactory.Create(0.99m, new DateTime().AddDays(10)),
                                     TermPointFactory.Create(0.97m, new DateTime().AddDays(100))
                                 };
            var termCurve = TermCurve.Create(new DateTime(), new InterpolationMethod { Value = "LinearInterpolation" }, true, termPoints);
            Interpolator = new TermCurveInterpolator(termCurve, new DateTime(), dc);//TODO need to create a surfaceinterpolator.  
            _algorithm = "Linear";
            SetFpMLData(fpmlData);
//            var holder = new PricingStructureAlgorithmsHolder();
            bool doBuild = GetVolatilityMatrix().dataPoints.point == null;
            if (doBuild)
            {
                //               var bootstrapperName = holder.GetValue(PricingStructureType.RateVolatilityMatrix, _algorithm, "Bootstrapper");

                //               Bootstrapper = Bootstrap(bootstrapperName, PricingStructure, PricingStructureValuation);
            }

            //           SetInterpolator(PricingStructureValuation.baseDate.Value, _algorithm, holder);
            _matrixIndexHelper = new SortedList<ExpiryTenorStrikeKey, int>(new ExpiryTenorStrikeKey());
            ProcessVolatilityRepresentation();
        }

        #endregion

        #region IPricingStructure Members

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        protected void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        /// <summary>
        /// Gets the VolatilityMatrix.
        /// </summary>
        /// <returns></returns>
        public VolatilityMatrix GetVolatilityMatrix()
        {
            return (VolatilityMatrix)PricingStructureValuation;
        }

        /// <summary>
        /// Gets the VolatilityRepresentation.
        /// </summary>
        /// <returns></returns>
        public VolatilityRepresentation GetVolatilityRepresentation()
        {
            return (VolatilityRepresentation)PricingStructure;
        }

        /// <summary>
        /// This call returns an IValue structure for an exact match with a given point.
        /// If the point does not exist this method will return null.
        /// </summary>
        /// <param name="pt">The point in the surface/cube to get the value of</param>
        /// <returns>An IValue object representing the value/coordinates of the point</returns>
        public override IValue GetValue(IPoint pt)
        {
            IValue val = null;
            string tenor = null;
            string strike;
            var expiry = ((Period)pt.Coords[0]).ToString();
            if (pt.Coords.Count > 2)
            {
                tenor = ((Period)pt.Coords[1]).ToString();
                strike = pt.Coords[2].ToString();
            }
            else
                strike = pt.Coords[1].ToString();
            var key = new ExpiryTenorStrikeKey(expiry, tenor, strike);
            if (_matrixIndexHelper.ContainsKey(key))
            {
                var idx = _matrixIndexHelper[key];
                var volatility = GetVolatilityMatrix().dataPoints.point[idx].value;
                var id = GetVolatilityMatrix().dataPoints.point[idx].id;
                val = new VolatilityValue(id, volatility, pt);
            }
            return val;
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
            // Test each dimension for the two closest points
            // then build a set of points from each dimension point
            var hasTerm = ((Coordinate)pt).PricingDataCoordinate.term != null;
            var expiryOrigin = ((Period)((Coordinate)pt).PricingDataCoordinate.expiration[0].Items[0]).ToYearFraction();
            var termOrigin = hasTerm ? ((Period)((Coordinate)pt).PricingDataCoordinate.term[0].Items[0]).ToYearFraction() : 0;
            var strikeOrigin = (double)((Coordinate)pt).PricingDataCoordinate.strike[0];
            var expiries = new SortedList<double, List<string>>();
            var tenors = new SortedList<double, List<string>>();
            var strikes = new SortedList<double, List<string>>();
            foreach (var key in _matrixIndexHelper.Keys)
            {
                var expiryPoint = key.Expiry.ToYearFraction();
                var termPoint = key.Tenor?.ToYearFraction() ?? 0;
                var strikePoint = (double)key.Strike;
                var expiryKeyPart = key.Expiry.ToString();
                var termKeyPart = key.Tenor?.ToString();
                var strikeKeyPart = key.Strike.ToString(CultureInfo.InvariantCulture);
                // Test closest expiry - keep closest 2
                var expiryDistance = System.Math.Sqrt((expiryOrigin - expiryPoint) * (expiryOrigin - expiryPoint));
                UpdateKeyPartList(ref expiries, expiryDistance, expiryKeyPart);
                // Test closest tenor - keep closest 2
                if (hasTerm)
                {
                    var tenorDistance = System.Math.Sqrt((termOrigin - termPoint) * (termOrigin - termPoint));
                    UpdateKeyPartList(ref tenors, tenorDistance, termKeyPart);
                }
                // Test closest strike - keep closest 2
                var strikeDistance = System.Math.Sqrt((strikeOrigin - strikePoint) * (strikeOrigin - strikePoint));
                UpdateKeyPartList(ref strikes, strikeDistance, strikeKeyPart);
            }
            // We now have the 2 closest expiries/terms and strikes so we build keys for them
            var c = UnpackKeysAndMakeCoordinates(expiries, tenors, strikes, hasTerm);
            return c.Select(GetValue).ToList();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string GetAlgorithm()
        {
            return _algorithm;
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
                return Convert.ToDouble(val.Value);

            IPoint pt;
            var values = (List<IValue>)GetClosestValues(point);
            var bounds = new List<IPoint>();

            // Build list of DoublePoint2D
            if (_interpolation.GetType() == typeof(BilinearInterpolator))
            {
                pt = ToDoublePoint2D(point);
                bounds.AddRange((from VolatilityValue value in values select ToDoublePoint2D(value)));
            }
            // Build list of DoublePoint3D
            else
            {
                pt = ToDoublePoint3D(point);
                bounds.AddRange((from VolatilityValue value in values select ToDoublePoint3D(value)));
            }

            return ((Interpolator)_interpolation).Value(pt, bounds);
        }

        #endregion

        #region Public Convenience Methods

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public object[,] Surface()
        {
            // Assign the matrix size
            var rawSurface = new object[_matrixRowCount + 1, _matrixColumnCount];

            var addTenor = _matrixIndexHelper.Keys[0].Tenor != null;

            // Zeroth row will be the column headers
            for (int column = 0; column < _matrixColumnCount; column++)
            {
                if (column == 0)
                    rawSurface[0, column] = "Option Expiry";
                else if (column == 1 && addTenor)
                    rawSurface[0, column] = "Tenor";
                else if (column > 0 && !addTenor)
                    rawSurface[0, column] = _matrixIndexHelper.Keys[column - 1].Strike.ToString(CultureInfo.InvariantCulture);
                else if (column > 1 && addTenor)
                    rawSurface[0, column] = _matrixIndexHelper.Keys[column - 2].Strike.ToString(CultureInfo.InvariantCulture);
            }

            // Add the data rows to the matrix
            var idx = 0;
            for (var row = 1; row < _matrixRowCount + 1; row++)
            {
                for (var column = 0; column < _matrixColumnCount; column++)
                {
                    if (column == 0)
                        rawSurface[row, column] = _matrixIndexHelper.Keys[idx].Expiry.ToString();
                    else if (column == 1 && addTenor)
                        rawSurface[row, column] = _matrixIndexHelper.Keys[idx].Tenor.ToString();
                    else if (column > 0 && !addTenor)
                        rawSurface[row, column] = GetVolatilityMatrix().dataPoints.point[_matrixIndexHelper.Values[idx++]].value;
                    else if (column > 1 && addTenor)
                    {
                        rawSurface[row, column] = GetVolatilityMatrix().dataPoints.point[_matrixIndexHelper.Values[idx++]].value;
                    }
                }
            }
            return rawSurface;
        }

        #endregion

        #region Private Methods

        #region DoublePoint Conversions

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point2D"/>
        /// that represents the year fraction expiry and a Point2D strike
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        private static Point2D ToDoublePoint2D(VolatilityValue val)
        {
            var expiry = (Period)val.PricePoint.coordinate[0].expiration[0].Items[0];
            var strike = val.PricePoint.coordinate[0].strike[0];
            var value = Convert.ToDouble(val.Value);
            return new Point2D((double)strike, expiry.ToYearFraction(), value);
        }

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
            var strike = val.PricePoint.coordinate[0].strike[0];
            var value = Convert.ToDouble(val.Value);
            return new Point3D((double)strike, tenor.ToYearFraction(), expiry.ToYearFraction(), value);
        }

        /// <summary>
        /// Convert a VolatilityValue into a <see cref="Point3D"/>
        /// that represents the year fraction expiry and a strike
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        private static Point2D ToDoublePoint2D(IPoint coord)
        {
            var expiry = (Period)((Coordinate)coord).PricingDataCoordinate.expiration[0].Items[0];
            var strike = ((Coordinate)coord).PricingDataCoordinate.strike[0];
            const double value = 0;
            return new Point2D((double)strike, expiry.ToYearFraction(), value);
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
        /// Unpack the key lists and create a set of coordinates from them
        /// </summary>
        /// <param name="expiry">The list of expiry terms</param>
        /// <param name="tenor">The list of tenor terms</param>
        /// <param name="strike">The list of strikes</param>
        /// <param name="hasTenor">Tenor flag: True if this volatility structure is a cube</param>
        /// <returns>An array of Coordinate</returns>
        private static IEnumerable<Coordinate> UnpackKeysAndMakeCoordinates(SortedList<double, List<string>> expiry,
                                                                 SortedList<double, List<string>> tenor, SortedList<double, List<string>> strike, bool hasTenor)
        {
            Coordinate[] coord;
            var expiryKeys = new string[2];
            var tenorKeys = new string[2];
            var strikeKeys = new string[2];
            // Unpack the expiries
            if (expiry[expiry.Keys[0]].Count == 2)
            {
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
                strikeKeys[0] = strike[strike.Keys[0]][0];
                strikeKeys[1] = strike[strike.Keys[0]][1];
            }
            else
            {
                strikeKeys[0] = strike[strike.Keys[0]][0];
                strikeKeys[1] = strike[strike.Keys[1]][0];
            }
            if (!hasTenor)
            {
                coord = new Coordinate[4];
                coord[0] = new Coordinate(expiryKeys[0], strikeKeys[0]);
                coord[1] = new Coordinate(expiryKeys[0], strikeKeys[1]);
                coord[2] = new Coordinate(expiryKeys[1], strikeKeys[0]);
                coord[3] = new Coordinate(expiryKeys[1], strikeKeys[1]);
            }
            else
            {
                // Unpack the tenors
                if (tenor[tenor.Keys[0]].Count == 2)
                {
                    tenorKeys[0] = tenor[tenor.Keys[0]][0];
                    tenorKeys[1] = tenor[tenor.Keys[0]][1];
                }
                else
                {
                    tenorKeys[0] = tenor[tenor.Keys[0]][0];
                    tenorKeys[1] = tenor[tenor.Keys[1]][0];
                }
                coord = new Coordinate[8];
                coord[0] = new Coordinate(expiryKeys[0], tenorKeys[0], strikeKeys[0]);
                coord[1] = new Coordinate(expiryKeys[0], tenorKeys[0], strikeKeys[1]);
                coord[2] = new Coordinate(expiryKeys[0], tenorKeys[1], strikeKeys[0]);
                coord[3] = new Coordinate(expiryKeys[0], tenorKeys[1], strikeKeys[1]);
                coord[4] = new Coordinate(expiryKeys[1], tenorKeys[0], strikeKeys[0]);
                coord[5] = new Coordinate(expiryKeys[1], tenorKeys[0], strikeKeys[1]);
                coord[6] = new Coordinate(expiryKeys[1], tenorKeys[1], strikeKeys[0]);
                coord[7] = new Coordinate(expiryKeys[1], tenorKeys[1], strikeKeys[1]);
            }
            return coord;
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
                        var keypartList = new List<string> {keyPart};
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
        /// Generate an array of PricingStructurePoint from a set of input arrays
        /// The array can then be added to the Matrix
        /// </summary>
        /// <param name="expiry">Expiry values to use</param>
        /// <param name="term">Tenor values to use</param>
        /// <param name="strike">Strike values to use</param>
        /// <param name="volatility">An array of volatility values</param>
        /// <returns></returns>
        private PricingStructurePoint[] ProcessRawSurface(string[] expiry, string[] term, string[] strike, double[,] volatility)
        {
            var expiryLength = expiry.Length;
            var termLength = term?.Length ?? 1;
            var strikeLength = strike.Length;
            var pointIndex = 0;
            var points = new PricingStructurePoint[expiryLength * termLength * strikeLength];
            // Offset row counter
            var row = 0;
            for (var expiryIndex = 0; expiryIndex < expiryLength; expiryIndex++)
            {
                // extract the current expiry
                var expiryKeyPart = expiry[expiryIndex];
                for (var termIndex = 0; termIndex < termLength; termIndex++)
                {
                    // extract the current tenor (term) of null if there are no tenors
                    var tenorKeyPart = term?[termIndex];
                    // Offset column counter
                    var column = 0;
                    for (var strikeIndex = 0; strikeIndex < strikeLength; strikeIndex++)
                    {
                        // Extract the strike to use in the helper key
                        var strikeKeyPart = strike[strikeIndex];
                        // Extract the row,column indexed volatility
                        var vol = (decimal)volatility[row, column++];
                        // Build the index offset list helper
                        var key = new ExpiryTenorStrikeKey(expiryKeyPart, tenorKeyPart, strikeKeyPart);
                        _matrixIndexHelper.Add(key, pointIndex);
                        // Add the value to the points array (dataPoints entry in the matrix)
                        var val = new VolatilityValue(null, vol, new Coordinate(expiryKeyPart, tenorKeyPart, strike[strikeIndex]));
                        points[pointIndex++] = val.PricePoint;
                    }
                    row++;
                }
            }
            return points;
        }

        /// <summary>
        /// Record the volatilities in the key helper and counts
        /// </summary>
        private void ProcessVolatilityRepresentation()
        {
            var row = 0;
            var column = 0;
            var columnExtra = 0;
            var strikeCheck = decimal.MinValue;
            var points = GetVolatilityMatrix().dataPoints.point;
            for (var idx = 0; idx < points.Length; idx++)
            {
                var expiry = (Period)points[idx].coordinate[0].expiration[0].Items[0];
                var tenor = points[idx].coordinate[0].term != null ? (Period)points[idx].coordinate[0].term[0].Items[0] : null;
                var strike = points[idx].coordinate[0].strike[0];
                if (idx == 0)
                {
                    strikeCheck = strike;
                    // Generate an interpolator to use
                    columnExtra = tenor == null ? 1 : 2;
                }
                else
                {
                    column++;
                    if (strike == strikeCheck)
                    {
                        row++;
                        column = 0;
                    }
                }
                _matrixIndexHelper.Add(new ExpiryTenorStrikeKey(expiry,tenor,strike), idx);
            }
            _matrixColumnCount = column + columnExtra + 1;
            _matrixRowCount = row + 1;
        }

        /// <summary>
        /// When Excel Ranges are converted to arrays they are 1 based...
        /// We need to reconstruct the arrays as 0 based
        /// </summary>
        /// <param name="surface"></param>
        /// <returns>0 based 2D array</returns>
        private static object[,] RedimensionRawSurface(object[,] surface)
        {
            // Set the upper/lower bounds of the converted array and adjust the array we write to accordingly
            var hi1D = surface.GetUpperBound(0);
            var hi2D = surface.GetUpperBound(1);

            var zeroBasedArray = new object[hi1D, hi2D];
            Array.Copy(surface, surface.GetLowerBound(0), zeroBasedArray, zeroBasedArray.GetLowerBound(0), surface.Length);

            return zeroBasedArray;
        }

        private static string[] ExtractExpiryFromRawSurface(object[,] surface)
        {
            var hi1D = surface.GetUpperBound(0) + 1;

            var expiryList = new List<string>();

            for (var idx = 1; idx < hi1D; idx++)
                if (!expiryList.Contains(surface[idx, 0].ToString()))
                    expiryList.Add(surface[idx, 0].ToString());

            return expiryList.ToArray();
        }

        private static string[] ExtractTenorFromRawSurface(object[,] surface, bool isCube)
        {
            if (!isCube)
                return null;

            var hi1D = surface.GetUpperBound(0) + 1;

            var tenorList = new List<string>();

            for (var idx = 1; idx < hi1D; idx++)
                if (!tenorList.Contains(surface[idx, 1].ToString()))
                    tenorList.Add(surface[idx, 1].ToString());

            return tenorList.ToArray();
        }

        /// <summary>
        /// Extract the strikes from the array
        /// </summary>
        /// <param name="surface"></param>
        /// <param name="isCube"></param>
        /// <returns></returns>
        private static string[] ExtractStrikeFromRawSurface(object[,] surface, bool isCube)
        {
            // Set the upper/lower bounds of the converted array
            var hi2D = surface.GetUpperBound(1) + 1;

            var startIdx = isCube ? 2 : 1;

            var strikeList = new List<string>();

            for (var idx = startIdx; idx < hi2D; idx++)
                if (!strikeList.Contains(surface[0, idx].ToString()))
                    strikeList.Add(surface[0, idx].ToString());

            return strikeList.ToArray();
        }

        private static double[,] ExtractVolatilitiesFromRawSurface(object[,] surface, bool isCube)
        {
            var hi1D = surface.GetUpperBound(0) + 1;
            var hi2D = surface.GetUpperBound(1) + 1;

            var startColumn = isCube ? 2 : 1;
            const int startRow = 1;

            var vols = new double[hi1D - startRow, hi2D - startColumn];

            for (var row = startRow; row < hi1D; row++)
            {
                for (var column = startColumn; column < hi2D; column++)
                {
                    vols[row - startRow, column - startColumn] = Convert.ToDouble(surface[row, column]);
                }
            }
            return vols;
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// A class to model an expiry/term key pair.
        /// If a surface has only an expiry dimension then the term (tenor) will
        /// default to null.
        /// The class implements the IComparer&lt;ExpiryTenorPairKey&gt;.Compare() method
        /// to keep track of indexes and their keys.
        /// </summary>
        private class ExpiryTenorStrikeKey : IComparer<ExpiryTenorStrikeKey>
        {
            #region Properties

            public Period Expiry { get; }
            public Period Tenor { get; }
            public decimal Strike { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Default constructor for use as a Comparer
            /// </summary>
            public ExpiryTenorStrikeKey()
                : this(null, null, null)
            {
            }

            /// <summary>
            /// Use the Expiry and Tenor to create a key.
            /// Expiry or tenor can be null but not both.
            /// </summary>
            /// <param name="expiry">Expiry key part</param>
            /// <param name="tenor">Tenor key part (can be null)</param>
            /// <param name="strike">Strike key part</param>
            public ExpiryTenorStrikeKey(string expiry, string tenor, string strike)
            {
                try
                {
                    Expiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
                }
                catch (System.Exception)
                {
                    Expiry = null;
                }

                try
                {
                    Tenor = tenor != null ? PeriodHelper.Parse(tenor) : null;
                }
                catch (System.Exception)
                {
                    Tenor = null;
                }

                try
                {
                    Strike = strike != null ? Convert.ToDecimal(strike) : 0;
                }
                catch (System.Exception)
                {
                    Strike = 0;
                }
            }

            //public ExpiryTenorStrikeKey(string expiry, string strike)
            //    : this(expiry, null, strike)
            //{ }

            /// <summary>
            /// Create a key from a Coordinate
            /// </summary>
            /// <param name="expiry"></param>
            /// <param name="tenor"></param>
            /// <param name="strike"></param>
            public ExpiryTenorStrikeKey(Period expiry, Period tenor, decimal strike)
            {
                Expiry = expiry;
                Tenor = tenor;
                Strike = strike;
            }

            //public ExpiryTenorStrikeKey(Interval expiry, decimal strike)
            //    : this(expiry, null, strike)
            //{ }

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
    }
}