using System;
using System.Collections.Generic;
using Core.Common;
using Orion.Constants;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.PricingStructures.Surfaces;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using Orion.Util.Logging;

namespace Highlander.CurveViewer
{
    internal static class PricingStructureFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <param name="rowHeaderCount"></param>
        /// <returns></returns>
        public static decimal[,] ConvertToDecimalArray(object[,] inputRange, int rowHeaderCount)
        {
            var numRows = inputRange.GetLength(0);
            var numColumns = inputRange.GetLength(1) - rowHeaderCount;
            var result = new decimal[numRows, numColumns];
            for (var row = 0; row < numRows; row++)
            {
                for (var col = 0; col < numColumns; col++)
                {
                    try
                    {
                        result[row, col] = Convert.ToDecimal(inputRange[row, col + rowHeaderCount]);
                    }
                    catch (Exception)
                    {
                        throw new ArgumentException("Input range must contain numbers");
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputRange"></param>
        /// <returns></returns>
        public static decimal[] ConvertToDecimalArray(object[] inputRange)
        {
            var rowCount = inputRange.Length;
            var result = new decimal[rowCount];
            for (var row = 0; row < rowCount; row++)
            {
                try
                {
                    result[row] = Convert.ToDecimal(inputRange[row]);
                }
                catch (Exception)
                {
                    throw new ArgumentException("Input range must contain numbers");
                }
            }
            return result;
        }

        ///<summary>
        /// Gets the distinct expiries, which must be in the first column.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<returns></returns>
        public static IEnumerable<string> ExtractExpiries(object[,] inputRange)
        {
            var numRows = inputRange.GetLength(0);
            var result = new List<string>();
            for (var i = 0; i < numRows; i++)
            {
                result.Add(Convert.ToString(inputRange[i, 0]));
            }
            return result.ToArray();
        }

        ///<summary>
        /// Gets the distinct expiries, which must be in the first column.
        ///</summary>
        ///<param name="inputRange">The input data range.</param>
        ///<returns></returns>
        public static IEnumerable<string> ExtractTenors(object[,] inputRange)
        {
            var numRows = inputRange.GetLength(0);
            var result = new List<string>();
            for (var i = 0; i < numRows; i++)
            {
                result.Add(Convert.ToString(inputRange[i, 1]));
            }
            return result.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static object[,] TrimNullRows(object[,] matrix)
        {
            //  find the last non null row
            //
            int index1LowerBound = matrix.GetLowerBound(1);
            int nonNullRows = 0;
            for (int i = matrix.GetLowerBound(0); i <= matrix.GetUpperBound(0); ++i)
            {
                object o = matrix.GetValue(i, index1LowerBound);
                if ((o == null) || (o.ToString() == String.Empty))
                    break;
                ++nonNullRows;
            }
            var width = matrix.GetUpperBound(1) - matrix.GetLowerBound(1);
            var result = new object[nonNullRows, width + 1];
            for (int i = matrix.GetLowerBound(0); i < matrix.GetLowerBound(0) + nonNullRows; ++i)
            {
                for (int j = matrix.GetLowerBound(1); j <= matrix.GetUpperBound(1); ++j)
                {
                    var value = matrix.GetValue(i, j);
                    result.SetValue(value, i, j);
                }
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static Array TrimNulls(Array array)
        {
            //  find the last non null row
            //
            int index1LowerBound = array.GetLowerBound(1);
            int nonNullRows = 0;
            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                object o = array.GetValue(i, index1LowerBound);
                if (o == null || o.ToString() == String.Empty)
                    break;
                ++nonNullRows;
            }
            //  get the element type
            //
            Type elementType = array.GetType().GetElementType();
            if (elementType != null)
            {
                var result = Array.CreateInstance(elementType, new[] { nonNullRows, array.GetLength(1) },
                    new[] { array.GetLowerBound(0), array.GetLowerBound(1) });
                for (int i = array.GetLowerBound(0); i < array.GetLowerBound(0) + nonNullRows; ++i)
                {
                    for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                    {
                        var value = array.GetValue(i, j);
                        result.SetValue(value, i, j);
                    }
                }
                return result;
            }
            return null;
        }

        #region Public Creators

        /// <summary>
        /// Mainly used for RateVolMatrix surfaces that have the inputs sent as additional
        /// </summary>
        /// <returns></returns>
        internal static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace, object[][] propertiesRange, object[][] values, object[][] additional)
        {
            var properties = propertiesRange.ToNamedValueSet();
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            if (psType != PricingStructureTypeEnum.RateVolatilityMatrix)
            {
                return CreatePricingStructure(logger, cache, nameSpace, propertiesRange, values);
            }
            var expiries = new List<DateTime>();
            var vols = new List<double>();
            int rowCount = values.GetUpperBound(0);
            for (int row = 0; row <= rowCount; row++)
            {
                if (values[row][0] == null || string.IsNullOrEmpty(values[row][0].ToString())) break;
                expiries.Add((DateTime)values[row][0]);
                vols.Add((double)values[row][1]);
            }
            int additionalLength = additional.GetUpperBound(0);
            var inputInstruments = new List<string>();
            var inputSwapRates = new List<double>();
            var inputBlackVolRates = new List<double>();
            for (int row = 0; row <= additionalLength; row++)
            {
                if (additional[row][0] == null || string.IsNullOrEmpty(additional[row][0].ToString())) break;
                inputInstruments.Add(additional[row][0].ToString());
                inputSwapRates.Add((double)additional[row][1]);
                inputBlackVolRates.Add((double)additional[row][2]);
            }
            //return RateATMVolatilitySurface
            return new RateVolatilitySurface(logger, cache, nameSpace, properties, expiries.ToArray(), vols.ToArray(), inputInstruments.ToArray(),
                inputSwapRates.ToArray(), inputBlackVolRates.ToArray());
        }

        internal static IPricingStructure CreatePricingStructure(ILogger logger, ICoreCache cache, string nameSpace, object[][] propertiesRange, object[][] values)
        {
            var properties = propertiesRange.ToNamedValueSet();
            string suppliedPricingStructureType = properties.GetString(CurveProp.PricingStructureType, true);
            EnumHelper.TryParse(suppliedPricingStructureType, true, out PricingStructureTypeEnum psType);
            if (PricingStructureHelper.CurveTypes.Contains(psType))
            {
                return Orion.CurveEngine.Factory.PricingStructureFactory.CreatePricingStructure(logger, cache, nameSpace, null, null, properties, ConvertObjectArray(values));
            }
            var expiries = ExtractExpiries(values);
            var volatilities = ExtractVols(values, expiries.Length);
            if (psType == PricingStructureTypeEnum.RateATMVolatilityMatrix)
            {
                string[] tenors = ExtractTenors(values);
                return new RateATMVolatilitySurface(logger, cache, nameSpace, properties, expiries, tenors, volatilities);
            }
            if (PricingStructureHelper.VolSurfaceTypes.Contains(psType))
            {
                var strikes = ExtractStrikes(values);
                return Orion.CurveEngine.Factory.PricingStructureFactory.CreateVolatilitySurface(logger, cache, nameSpace, properties, expiries, strikes, volatilities);
            }
            throw (new Exception($"{suppliedPricingStructureType} cannot be created using this function"));
        }

        #endregion

        #region Private Creators

        private static object[,] ConvertObjectArray(object[][] values)
        {
            var length = values.GetLength(0);
            var result = new object[length, 3];
            int lby = values.GetLowerBound(0);
            int uby = values.GetUpperBound(0);
            int lbx = values[0].GetLowerBound(0);
            int ubx = values[0].GetUpperBound(0);
            for (var i = lby; i <= uby; ++i)
            {
                if (values[i][lbx] == null || string.IsNullOrEmpty(values[i][lbx].ToString())) break;
                result[i - lby, 0] = Convert.ToString(values[i][lbx]);
                result[i - lby, 1] = Convert.ToDecimal(values[i][lbx + 1]);
                if (ubx < lbx + 2) continue;
                if (null != values[i][lbx + 2])
                {
                    result[i - lby, 2] = Convert.ToDecimal(values[i][lbx + 2]);
                }
            }
            return TrimNullRows(result);
        }

        //private static CurveBase CreateCurveStructure(ILogger logger, ICoreCache cache, NamedValueSet properties, object[][] values)
        //{
        //    var instruments = new string[values.GetLength(0)];
        //    var rates = new decimal[values.GetLength(0)];
        //    var additional = new decimal[values.GetLength(0)];

        //    int lby = values.GetLowerBound(0);
        //    int uby = values.GetUpperBound(0);
        //    int lbx = values[0].GetLowerBound(0);
        //    int ubx = values[0].GetUpperBound(0);
        //    for (var i = lby; i <= uby; ++i)
        //    {
        //        if (values[i][lbx] == null || string.IsNullOrEmpty(values[i][lbx].ToString())) break;
        //        instruments[i - lby] = Convert.ToString(values[i][lbx]);
        //        rates[i - lby] = Convert.ToDecimal(values[i][lbx + 1]);
        //        if (ubx < lbx + 2) continue;
        //        if (null != values[i][lbx + 2])
        //        {
        //            additional[i - lby] = Convert.ToDecimal(values[i][lbx + 2]);
        //        }
        //    }
        //    var structure = (CurveBase)CurveEngine.Factory.PricingStructureFactory.CreatePricingStructure(logger, cache, null, null, properties, instruments, rates, additional);
        //    return structure;
        //}

        #endregion

        #region Utilities

        /// <summary>
        /// Extract the strikes information held in the first row of the array
        /// </summary>
        private static double[] ExtractStrikes(object[][] rawGrid)
        {
            var strikes = new List<double>();
            int maxColumns = rawGrid[0].GetUpperBound(0) + 1;
            for (int cols = 1; cols < maxColumns; cols++)
            {
                if (rawGrid[0][cols] == null || string.IsNullOrEmpty(rawGrid[0][cols].ToString())) break;
                if (!double.TryParse(rawGrid[0][cols].ToString(), out var strike))
                {
                    throw new InvalidCastException($"Cannot cast '{rawGrid[0][cols]}' to strike");
                }
                strikes.Add(strike);
            }
            return strikes.ToArray();
        }

        /// <summary>
        /// Extract the expiry column from the raw data grid
        /// </summary>
        private static string[] ExtractExpiries(object[][] rawGrid)
        {
            int maxRows = rawGrid.GetUpperBound(0) + 1;
            var expiries = new List<string>();
            for (int row = 1; row < maxRows; row++)
            {
                if (rawGrid[row][0] == null || string.IsNullOrEmpty(rawGrid[row][0].ToString())) break;
                expiries.Add(rawGrid[row][0].ToString());
            }
            return expiries.ToArray();
        }

        /// <summary>
        /// Extract the expiry column from the raw data grid
        /// </summary>
        private static string[] ExtractTenors(object[][] rawGrid)
        {
            int maxColumns = rawGrid[0].GetUpperBound(0) + 1;
            var tenors = new List<string>();
            for (int column = 1; column < maxColumns; column++)
            {
                if (rawGrid[0][column] == null || string.IsNullOrEmpty(rawGrid[0][column].ToString())) break;
                tenors.Add(rawGrid[0][column].ToString());
            }
            return tenors.ToArray();
        }

        /// <summary>
        /// Extract the vols from the raw Data grid (everything except the first column and row)
        /// </summary>
        private static double[,] ExtractVols(object[][] rawGrid, int maxRows)
        {
            int maxColumns = rawGrid[0].GetUpperBound(0);
            var vols = new double[maxRows, maxColumns];
            for (int column = 1; column <= maxColumns; column++)
            {
                for (int row = 1; row <= maxRows; row++)
                {
                    if (rawGrid[row][column] == null || string.IsNullOrEmpty(rawGrid[row][column].ToString())) break;
                    if (!double.TryParse(rawGrid[row][column].ToString(), out var vol) || vol < 0)
                    {
                        throw new InvalidCastException($"Cannot cast '{rawGrid[row][column]}' to volatility");
                    }
                    vols[row - 1, column - 1] = vol;
                }
            }
            return vols;
        }

        #endregion
    }
}