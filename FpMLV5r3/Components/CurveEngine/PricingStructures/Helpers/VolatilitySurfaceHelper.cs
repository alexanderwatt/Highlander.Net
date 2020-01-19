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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Interpolations;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Helpers
{
    ///<summary>
    ///</summary>
    public enum CubeDimension
    {
        ///<summary>
        ///</summary>
        Term,
        ///<summary>
        ///</summary>
        Expiration,
        ///<summary>
        ///</summary>
        Strike,
        /// <summary>
        /// </summary>
        Volatility
    }

    class TimeDimensionEqualityComparer : IEqualityComparer<TimeDimension>
    {
        public bool Equals(TimeDimension x, TimeDimension y)
        {
            return XmlSerializerHelper.AreEqual(x, y);
        }

        public int GetHashCode(TimeDimension obj)
        {
            return XmlSerializerHelper.SerializeToString(obj).GetHashCode();
        }
    }

    ///<summary>
    ///</summary>
    public class VolatilitySurfaceHelper
    {
        ///<summary>
        ///</summary>
        ///<param name="originalArray"></param>
        ///<returns></returns>
        public static object[,] UpcastArray(decimal[,] originalArray)
        {
            var result = new object[originalArray.GetLength(0), originalArray.GetLength(1)];
            for (int row = 0; row < originalArray.GetLength(0); ++row)
            {
                for (int column = 0; column < originalArray.GetLength(1); ++column)
                {
                    result[row, column] = originalArray[row, column];
                }
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="inflatedArray"></param>
        ///<param name="horizontalDimension"></param>
        ///<returns></returns>
        public static object[,] UpdateHorizontalDimension(object[,] inflatedArray, Array horizontalDimension)
        {
            #region check params

            Debug.Assert(null != inflatedArray);
            Debug.Assert(null != horizontalDimension);
            Debug.Assert(horizontalDimension.GetLength(0) == inflatedArray.GetLength(1) - 1);

            #endregion

            for (int i = 0; i < horizontalDimension.Length; ++i)
            {
                inflatedArray[0, 1 + i] = horizontalDimension.GetValue(i);
            }
            return inflatedArray;
        }

        ///<summary>
        ///</summary>
        ///<param name="inflatedArray"></param>
        ///<param name="verticalDimension"></param>
        ///<returns></returns>
        public static object[,] UpdateVerticalDimension(object[,] inflatedArray, Array verticalDimension)
        {
            #region check params

            Debug.Assert(null != inflatedArray);
            Debug.Assert(null != verticalDimension);
            Debug.Assert(verticalDimension.GetLength(0) == inflatedArray.GetLength(0) - 1);

            #endregion

            for (int i = 0; i < verticalDimension.Length; ++i)
            {
                inflatedArray[1 + i, 0] = verticalDimension.GetValue(i);
            }
            return inflatedArray;
        }

        ///<summary>
        ///</summary>
        ///<param name="originalArray"></param>
        ///<param name="filler"></param>
        ///<returns></returns>
        public static object[,] InflateArrayToAccomodateDimensionsVectors(object[,] originalArray, object filler)
        {
            #region check params

            Debug.Assert(null != originalArray);
            Debug.Assert(0 == originalArray.GetLowerBound(0));
            Debug.Assert(0 == originalArray.GetLowerBound(1));

            #endregion

            int rowsInOriginalArray = originalArray.GetLength(0);
            int columnsInOriginalArray = originalArray.GetLength(1);
            int rowsInInflatedArray = 1 + rowsInOriginalArray;
            int columnsInInflatedArray = 1 + columnsInOriginalArray;
            var result = new object[rowsInInflatedArray, columnsInInflatedArray];
            //  fill resulting array with the filler object first
            //
            for (int row = 0; row < rowsInInflatedArray; ++row)
            {
                for (int column = 0; column < columnsInInflatedArray; ++column)
                {
                    result[row, column] = filler;
                }
            }
            //  copy original array to a new array
            //
            for (int row = 0; row < rowsInOriginalArray; ++row)
            {
                for (int column = 0; column < columnsInOriginalArray; ++column)
                {
                    result[1 + row, 1 + column] = originalArray[row, column];
                }
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="decimalMatrix"></param>
        ///<returns></returns>
        public static double[,] ToDoubleMatrix(decimal[,] decimalMatrix)
        {
            var result = new double[decimalMatrix.GetLength(0), decimalMatrix.GetLength(1)];
            for (int i = 0; i <= decimalMatrix.GetUpperBound(0); ++i)
            {
                for (int j = 0; j <= decimalMatrix.GetUpperBound(1); ++j)
                {
                    result[i, j] = Convert.ToDouble(decimalMatrix[i, j]);
                }
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="arrayOfStrikes"></param>
        ///<returns></returns>
        public static double[] ConvertArrayOfStringsToDouble(string[] arrayOfStrikes)
        {
            var result = new List<string>(arrayOfStrikes).ConvertAll(Convert.ToDouble);
            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="arrayOfStrikes"></param>
        ///<returns></returns>
        public static double[] ConvertArrayOfDecimalsToDouble(decimal[] arrayOfStrikes)
        {
            var result = new List<decimal>(arrayOfStrikes).ConvertAll(Convert.ToDouble);
            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="arrayOfTenors"></param>
        ///<returns></returns>
        public static double[] ConvertArrayOfTenorsToDouble(string[] arrayOfTenors)
        {
            var result = new List<string>(arrayOfTenors).ConvertAll(s => PeriodHelper.Parse(s).ToYearFraction());
            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="arrayOfTimeDimensions"></param>
        ///<returns></returns>
        public static double[] ConvertArrayOfTimeDimensionsToDouble(TimeDimension[] arrayOfTimeDimensions)
        {
            var result = new List<TimeDimension>(arrayOfTimeDimensions).ConvertAll(td => (td.Items[0] as Period).ToYearFraction());
            return result.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="dataPoints"></param>
        ///<returns></returns>
        public static object[,] GetExpirationByStikeVolatilityMatrixWithDimensions(IEnumerable<PricingStructurePoint> dataPoints)
        {
            var pricingStructurePoints = dataPoints as PricingStructurePoint[] ?? dataPoints.ToArray();
            var rowVolMatrix = GetExpirationByStikeVolatilityMatrix(pricingStructurePoints);
            var upcastedMatrix = UpcastArray(rowVolMatrix);
            var inflatedMatrix = InflateArrayToAccomodateDimensionsVectors(upcastedMatrix, " ");
            var expirationVector = GetDimension1Vector(pricingStructurePoints, CubeDimension.Expiration);
            var expiryDoubleArray = ConvertArrayOfTimeDimensionsToDouble((TimeDimension[])expirationVector);          
            var strikesVector = GetDimension1Vector(pricingStructurePoints, CubeDimension.Strike);
            var strikesDoubleArray = ConvertArrayOfDecimalsToDouble((decimal[])strikesVector);
            inflatedMatrix = UpdateHorizontalDimension(inflatedMatrix, strikesDoubleArray);
            inflatedMatrix = UpdateVerticalDimension(inflatedMatrix, expiryDoubleArray);
            return inflatedMatrix;
        }

        /// <summary>
        /// The requirement is - all data points are 
        /// sorted in ascending order 
        /// first by dimension1, 
        /// then by dimension2
        /// </summary>
        /// <param name="dataPoints"></param>
        /// <returns></returns>
        public static decimal[,] GetExpirationByStikeVolatilityMatrix(IEnumerable<PricingStructurePoint> dataPoints)
        {
            var pricingStructurePoints = dataPoints as PricingStructurePoint[] ?? dataPoints.ToArray();
            var expirationDimensionSize = GetSizeOfDimension(pricingStructurePoints, CubeDimension.Expiration);
            var strikeDimensionSize = GetSizeOfDimension(pricingStructurePoints, CubeDimension.Strike);
            if (expirationDimensionSize * strikeDimensionSize != pricingStructurePoints.Length) return null;
            var result = new decimal[expirationDimensionSize, strikeDimensionSize];
            var strikeValues = GetDimensionValues(pricingStructurePoints, CubeDimension.Strike);
            var expirationValues = GetDimensionValues(pricingStructurePoints, CubeDimension.Expiration);
            for (var x = 0; x < expirationDimensionSize; x++)
            {
                for (var y = 0; y < strikeDimensionSize; y++)
                {
                    var value = GetValueByExpirationAndStrike(pricingStructurePoints, (TimeDimension)expirationValues[x],
                        (decimal)strikeValues[y]);
                    result[x, y] = value;
                }
            }
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="points"></param>
        ///<param name="expirationAsYearFraction"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        public static double GetValue(IEnumerable<PricingStructurePoint> points,
                                      double expirationAsYearFraction,
                                      double strike)
        {
            var pricingStructurePoints = points as PricingStructurePoint[] ?? points.ToArray();
            var volatilityMatrix = GetExpirationByStikeVolatilityMatrix(pricingStructurePoints);
            var expirationVector = GetDimension1Vector(pricingStructurePoints, CubeDimension.Expiration);
            var expiryDoubleArray = ConvertArrayOfTimeDimensionsToDouble((TimeDimension[])expirationVector);
            var strikesVector = GetDimension1Vector(pricingStructurePoints, CubeDimension.Strike);
            var strikesDoubleArray = ConvertArrayOfDecimalsToDouble((decimal[])strikesVector);
            var doubleMatrix = ToDoubleMatrix(volatilityMatrix);
            var bilinearInterpolation = new BilinearInterpolation(ref strikesDoubleArray, ref expiryDoubleArray, ref doubleMatrix);
            var interpolatedValue = bilinearInterpolation.Interpolate(expirationAsYearFraction, strike);
            return interpolatedValue;
        }

        private static decimal GetValueByExpirationAndStrike(IEnumerable<PricingStructurePoint> dataPoints,
                                                             TimeDimension expiration,
                                                             decimal strike)
        {
            foreach (var dataPoint in dataPoints)
            {
                if (
                    XmlSerializerHelper.AreEqual(dataPoint.coordinate[0].expiration[0], expiration)
                    &&
                    strike == dataPoint.coordinate[0].strike[0]
                    )
                {
                    return dataPoint.value;
                }
            }
            throw new ArgumentException();
        }

        ///<summary>
        ///</summary>
        ///<param name="dataPoints"></param>
        ///<param name="dimension"></param>
        ///<returns></returns>
        public static IEnumerable GetDimension1Vector(IEnumerable<PricingStructurePoint> dataPoints,
                                                      CubeDimension dimension)
        {
            var result = GetDimensionValues(dataPoints, dimension);
            return result;
        }


        ///<summary>
        ///</summary>
        ///<param name="dataPoints"></param>
        ///<param name="dimension"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static int GetSizeOfDimension(IEnumerable<PricingStructurePoint> dataPoints, CubeDimension dimension)
        {
            var strikeList = new List<Decimal>();
            var expirationList = new List<TimeDimension>();
            var termList = new List<TimeDimension>();
            foreach (var pricingStructurePoint in dataPoints)
            {
                var pricingDataPointCoordinate = pricingStructurePoint.coordinate[0];
                if (null != pricingDataPointCoordinate.strike && pricingDataPointCoordinate.strike.Length > 0)
                {
                    var strike = pricingDataPointCoordinate.strike[0];
                    strikeList.Add(strike);
                }
                if (null != pricingDataPointCoordinate.expiration && pricingDataPointCoordinate.expiration.Length > 0)
                {
                    var expiration = pricingDataPointCoordinate.expiration[0];
                    expirationList.Add(expiration);
                }
                if (null != pricingDataPointCoordinate.term && pricingDataPointCoordinate.term.Length > 0)
                {
                    var term = pricingDataPointCoordinate.term[0];
                    termList.Add(term);
                }
            }
            var numberOfMissingDimensions = 0;
            var numberOfUniqueStrikes = strikeList.Distinct().Count();
            if (numberOfUniqueStrikes <= 1) ++numberOfMissingDimensions;
            var numberOfUniqueExpirations = expirationList.Distinct(new TimeDimensionEqualityComparer()).Count();
            if (numberOfUniqueExpirations <= 1) ++numberOfMissingDimensions;
            var numberOfUniqueTerms = termList.Distinct(new TimeDimensionEqualityComparer()).Count();
            if (numberOfUniqueTerms <= 1) ++numberOfMissingDimensions;
            if (1 != numberOfMissingDimensions)
            {
                var message =
                    $"Number of missing dimensions is not 1 (actual value : '{numberOfMissingDimensions}'). Only 1 dimension out of 3 (strike, tenor, expiration) could be missing in a surface.";
                throw new NotSupportedException(message);
            }
            switch (dimension)
            {
                case CubeDimension.Term:
                    {
                        return numberOfUniqueTerms;
                    }
                case CubeDimension.Expiration:
                    {
                        return numberOfUniqueExpirations;
                    }
                case CubeDimension.Strike:
                    {
                        return numberOfUniqueStrikes;
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension));
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="dataPoints"></param>
        ///<param name="dimension"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static IList GetDimensionValues(IEnumerable<PricingStructurePoint> dataPoints, CubeDimension dimension)
        {
            var strikeList = new List<Decimal>();
            var expirationList = new List<TimeDimension>();
            var termList = new List<TimeDimension>();
            var volatilityList = new List<Decimal>();
            foreach (var pricingStructurePoint in dataPoints)
            {
                volatilityList.Add(pricingStructurePoint.value);
                var pricingDataPointCoordinate = pricingStructurePoint.coordinate[0];
                if (null != pricingDataPointCoordinate.strike && pricingDataPointCoordinate.strike.Length > 0)
                {
                    var strike = pricingDataPointCoordinate.strike[0];
                    strikeList.Add(strike);
                }
                if (null != pricingDataPointCoordinate.expiration && pricingDataPointCoordinate.expiration.Length > 0)
                {
                    var expiration = pricingDataPointCoordinate.expiration[0];
                    expirationList.Add(expiration);
                }
                if (null != pricingDataPointCoordinate.term && pricingDataPointCoordinate.term.Length > 0)
                {
                    var term = pricingDataPointCoordinate.term[0];
                    termList.Add(term);
                }
            }
            switch (dimension)
            {
                case CubeDimension.Term:
                    {
                        return termList.Distinct(new TimeDimensionEqualityComparer()).ToArray();
                    }
                case CubeDimension.Expiration:
                    {
                        return expirationList.Distinct(new TimeDimensionEqualityComparer()).ToArray();
                    }
                case CubeDimension.Strike:
                    {
                        return strikeList.Distinct().ToArray();
                    }
                case CubeDimension.Volatility:
                {
                    return volatilityList.ToArray();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(dimension));
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="dataPoints"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        public static CubeDimension GetMissingDimension(IEnumerable<PricingStructurePoint> dataPoints)
        {
            var strikeList = new List<Decimal>();
            var expirationList = new List<TimeDimension>();
            var termList = new List<TimeDimension>();
            foreach (var pricingStructurePoint in dataPoints)
            {
                var pricingDataPointCoordinate = pricingStructurePoint.coordinate[0];
                if (null != pricingDataPointCoordinate.strike && pricingDataPointCoordinate.strike.Length > 0)
                {
                    var strike = pricingDataPointCoordinate.strike[0];
                    strikeList.Add(strike);
                }
                if (null != pricingDataPointCoordinate.expiration && pricingDataPointCoordinate.expiration.Length > 0)
                {
                    var expiration = pricingDataPointCoordinate.expiration[0];
                    expirationList.Add(expiration);
                }
                if (null != pricingDataPointCoordinate.term && pricingDataPointCoordinate.term.Length > 0)
                {
                    var term = pricingDataPointCoordinate.term[0];
                    termList.Add(term);
                }
            }
            var numberOfMissingDimensions = 0;
            var numberOfUniqueStrikes = strikeList.Distinct().Count();
            if (numberOfUniqueStrikes <= 1) ++numberOfMissingDimensions;
            var numberOfUniqueExpirations = expirationList.Distinct(new TimeDimensionEqualityComparer()).Count();
            if (numberOfUniqueExpirations <= 1) ++numberOfMissingDimensions;
            var numberOfUniqueTerms = termList.Distinct(new TimeDimensionEqualityComparer()).Count();
            if (numberOfUniqueTerms <= 1) ++numberOfMissingDimensions;
            if (1 != numberOfMissingDimensions)
            {
                var message =
                    $"Number of missing dimensions is not 1 (actual value : '{numberOfMissingDimensions}'). Only 1 dimension out of 3 (strike, tenor, expiration) could be missing in a surface.";
                throw new NotSupportedException(message);
            }
            if (numberOfUniqueStrikes <= 1)
            {
                return CubeDimension.Strike;
            }
            if (numberOfUniqueExpirations <= 1)
            {
                return CubeDimension.Expiration;
            }
            if (numberOfUniqueTerms <= 1)
            {
                return CubeDimension.Term;
            }
            throw new NotSupportedException("You should not be get there.");
        }
    }
}