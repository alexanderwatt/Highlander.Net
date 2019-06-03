/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Orion.Util.Helpers;
using FpML.V5r3.Reporting;

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// This class models a CapFloor Volatility Matrix.
    /// This variant extends a base VolatilityMatrix to include a discount factor curve.
    /// The discount curve is included as a convenience - it is not intrinsic to a VolatilityMatrix
    /// </summary>
    [Serializable]
    [XmlType("CapFloorVolatilityMatrix")]
    [XmlRoot("VolatilityMatrix", IsNullable = false)]
    public class CapFloorVolatilityMatrix : VolatilityMatrix
    {
        #region CapFloorVolatilityMatrix Extensions

        /// <summary>
        /// An attached discount factor curve used in CapFloor bootstrapping
        /// </summary>
        public YieldCurveValuation DiscountFactorCurve { get; set; }

        /// <summary>
        /// This will be true if there is only an ATM strike applied
        /// </summary>
        public bool IsATMValuation { get; set; }

        #endregion

        #region ValuationDate to BaseDate

        /// <summary>
        /// The ValuationDate used by the CapFloor Vols matrix
        /// </summary>
        [XmlIgnore]
        public DateTime ValuationDate
        {
            get => baseDate?.Value ?? DateTime.MinValue;
            set => baseDate = new IdentifiedDate {Value = value};
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor to support XML serialization
        /// </summary>
        public CapFloorVolatilityMatrix()
        { }

        /// <summary>
        /// Full Constructor
        /// A matrix holds expiry (rows) x strike (columns) volatilities
        /// </summary>
        /// <param name="matrixId">The identifier used for this matrix</param>
        /// <param name="expiry">A list of Vol expiries</param>
        /// <param name="volType">The type of vol related to the expiry</param>
        /// <param name="vols">The vols at each strike/expiry</param>
        /// <param name="strikes">A list of strikes or null for ATM Vols</param>
        /// <param name="discountFactorDates">The discount factor dates used in the bootstrap</param>
        /// <param name="discountFactors">The discount factor values</param>
        public CapFloorVolatilityMatrix(string matrixId, string[] expiry, string[] volType, decimal[][] vols, decimal[] strikes,
                                        DateTime[] discountFactorDates, decimal[] discountFactors)
        {
            // Basic identifiers
            id = matrixId;
            // Create the yield curve
            DiscountFactorCurve = new YieldCurveValuation();
            GenerateDiscountFactorCurve(discountFactorDates, discountFactors);
            // Build the vols matrix
            dataPoints = new MultiDimensionalPricingData();
            // Add in expiry, type, strike and, vols
            GenerateVolatilityMatrix(expiry, volType, strikes, vols);
        }


        /// <summary>
        /// Full Constructor
        /// A matrix holds expiry (rows) x strike (columns) volatilities
        /// </summary>
        /// <param name="matrixId">The identifier used for this matrix</param>
        /// <param name="expiry">A list of Vol expiries</param>
        /// <param name="volType">The type of vol related to the expiry</param>
        /// <param name="vols">The vols at each strike/expiry</param>
        /// <param name="strikes">A list of strikes or null for ATM Vols</param>
        /// <param name="discountFactorDates">The discount factor dates used in the bootstrap</param>
        /// <param name="discountFactors">The discount factor values</param>
        public CapFloorVolatilityMatrix(string matrixId, string[] expiry, string[] volType, decimal[,] vols, decimal[] strikes,
                                        DateTime[] discountFactorDates, decimal[] discountFactors)
        {
            // Basic identifiers
            id = matrixId;
            // Create the yield curve
            DiscountFactorCurve = new YieldCurveValuation();
            GenerateDiscountFactorCurve(discountFactorDates, discountFactors);
            // Build the vols matrix
            dataPoints = new MultiDimensionalPricingData();
            // Add in expiry, type, strike and, vols
            GenerateVolatilityMatrix(expiry, volType, strikes, vols);
        }




        /// <summary>
        /// Full Constructor
        /// A matrix holds expiry (rows) x strike (columns) volatilities
        /// </summary>
        /// <param name="matrixId">The identifier used for this matrix</param>
        /// <param name="expiry">A list of Vol expiries</param>
        /// <param name="volType">The type of vol related to the expiry</param>
        /// <param name="vols">The vols at each strike/expiry</param>
        /// <param name="strikes">A list of strikes or null for ATM Vols</param>
        /// <param name="discountFactorDates">The discount factor dates used in the bootstrap</param>
        /// <param name="discountFactors">The discount factor values</param>
        public CapFloorVolatilityMatrix(string matrixId, string[] expiry, string[] volType, decimal[] vols, decimal strikes,
                                        DateTime[] discountFactorDates, decimal[] discountFactors)
        {
            // Basic identifiers
            id = matrixId;
            // Create the yield curve
            DiscountFactorCurve = new YieldCurveValuation();
            GenerateDiscountFactorCurve(discountFactorDates, discountFactors);
            // Build the vols matrix
            dataPoints = new MultiDimensionalPricingData();
            // Add in expiry, type, strike and, vols
            GenerateVolatilityMatrix(expiry, volType, strikes, vols);
        }

        #endregion

        #region Accessor Methods

        /// <summary>
        /// Get the discount factor curve associated with this CapFloor matrix
        /// </summary>
        /// <returns></returns>
        public SortedList<DateTime, decimal> DiscountFactors()
        {
            SortedList<DateTime, decimal> factors = null;
            if (DiscountFactorCurve.discountFactorCurve != null)
            {
                factors = new SortedList<DateTime, decimal>();
                foreach (TermPoint p in DiscountFactorCurve.discountFactorCurve.point)
                    factors.Add((DateTime)p.term.Items[0], p.mid);
            }
            return factors;
        }

        /// <summary>
        /// Get the discount factor curve associated with this CapFloor matrix
        /// </summary>
        /// <returns></returns>
        public SortedList<DateTime, double> DiscountFactorsAsDoubles()
        {
            SortedList<DateTime, double> factors = null;
            if (DiscountFactorCurve.discountFactorCurve != null)
            {
                factors = new SortedList<DateTime, double>();

                foreach (TermPoint p in DiscountFactorCurve.discountFactorCurve.point)
                    factors.Add((DateTime)p.term.Items[0], (double)p.mid);
            }
            return factors;
        }

        /// <summary>
        /// Get the volatilities in the format expected by the CapFloor Bootstrapper
        /// This returns the column of vols at all expiries that match the strike
        /// </summary>
        /// <param name="strike">The strike to retrieve volatilities at</param>
        /// <returns></returns>
        public List<CapVolatilityDataElement<int>> CapVolatilities(decimal strike)
        {
            var vols = new List<CapVolatilityDataElement<int>>();
            foreach (var p in dataPoints.point)
            {
                if (p.coordinate[0].strike == null || p.coordinate[0].strike[0] != strike) continue;
                var expiry = int.Parse(((Period)p.coordinate[0].expiration[0].Items[0]).periodMultiplier);
                var vol = p.value;
                var volType = VolatilityDataType.CapFloor;
                if (p.measureType.Value == "ETO")
                    volType = VolatilityDataType.ETO;
                var tmp = new CapVolatilityDataElement<int>(expiry, vol, volType);
                vols.Add(tmp);
            }
            return vols;
        }

        /// <summary>
        /// Get the volatilities in the format expected by the CapFloor Bootstrapper
        /// This version returns the ATM volatilities - there is no strike
        /// </summary>
        /// <returns></returns>
        public List<CapVolatilityDataElement<int>> CapVolatilities()
        {
            var vols = new List<CapVolatilityDataElement<int>>();
            foreach (var p in dataPoints.point)
            {
                if (p.coordinate[0].generic == null) continue;
                var expiry = int.Parse(((Period)p.coordinate[0].expiration[0].Items[0]).periodMultiplier);
                var vol = p.value;

                var volType = VolatilityDataType.CapFloor;
                if (p.measureType.Value == "ETO")
                    volType = VolatilityDataType.ETO;
                var tmp = new CapVolatilityDataElement<int>(expiry, vol, volType);
                vols.Add(tmp);
            }
            return vols;
        }

        /// <summary>
        /// Get the strikes from this Vol matrix
        /// If it is an ATM then return null
        /// </summary>
        /// <returns></returns>
        public decimal[] Strikes()
        {
            var strike = new List<decimal>();
            foreach (var p in dataPoints.point)
            {
                if (p.coordinate[0].strike == null) continue;
                var strikeVal = p.coordinate[0].strike[0];
                if (!strike.Contains(strikeVal))
                    strike.Add(strikeVal);
            }
            // Return the strikes or null if this is an ATM only
            decimal[] strikes = null;
            if (strike.Count > 0)
            {
                strikes = new decimal[strike.Count];
                strike.CopyTo(strikes);
            }
            return strikes;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loop through the factors and factor dates to create a Discount Factor Curve
        /// The dates and factors are 1 - 1 mapped
        /// The curve is attached to this cap/floor matrix
        /// </summary>
        /// <param name="discountFactorDates"></param>
        /// <param name="discountFactors"></param>
        private void GenerateDiscountFactorCurve(DateTime[] discountFactorDates, decimal[] discountFactors)
        {
            DiscountFactorCurve.discountFactorCurve = new TermCurve();
            var maxRow = discountFactorDates.Length;
            DiscountFactorCurve.discountFactorCurve.point = new TermPoint[maxRow];
            // loop through the factors/dates and write the values to point structure inside the discount factor curve
            for (var row = 0; row < maxRow; row++)
            {
                DiscountFactorCurve.discountFactorCurve.point[row] = 
                    new TermPoint
                        {
                            term = new TimeDimension
                                       {
                                           Items =
                                               new object[]
                                                   {
                                                       discountFactorDates[row]
                                                   }
                                       },
                            midSpecified = true,
                            mid = discountFactors[row]
                        };
            }
        }

        /// <summary>
        /// Build up the matrix of cap/floor volatilities (expiry x strike)
        /// </summary>
        /// <param name="expiry">An array of option expiries as terms</param>
        /// <param name="volType">The type of the volatility surface at each expiry</param>
        /// <param name="strike">An array of strike prices. If this is an ATM only there will be only one strike.</param>
        /// <param name="volatility">A 2d array of the volatilities</param>
        private void GenerateVolatilityMatrix(string[] expiry, string[] volType, decimal[] strike, decimal[][] volatility)
        {
            if (expiry == null) return;
            var maxRows = expiry.Length;
            var maxCols = strike?.Length ?? 1;
            dataPoints.point = new PricingStructurePoint[maxRows*maxCols];
            for (var row = 0; row < maxRows; row++)
            {
                Period volExpiry = CreateInterval(expiry[row]);
                TimeDimension expiration = new TimeDimension {Items = new object[] {volExpiry}};
                AssetMeasureType measureType = new AssetMeasureType {Value = volType[row]};
                for (var column = 0; column < maxCols; column++)
                {
                    var current = row * maxCols + column;
                    decimal? nullableStrike = strike?[column];
                    dataPoints.point[current]
                        = CreatePricingStructurePoint(nullableStrike, volatility[row][column], expiration, measureType);
                }
            }
        }

        private PricingStructurePoint CreatePricingStructurePoint(decimal? strike, decimal volatility, 
            TimeDimension expiration, AssetMeasureType measureType)
        {
            if (volatility < 0)
                throw new ArgumentException($"An illegal volatility value d: {volatility} found.");
            // Set the coordinate values
            PricingDataPointCoordinate coordinate = new PricingDataPointCoordinate { expiration = new [] { expiration } };
            // Set the strike if there are strikes
            if (strike != null)
            {
                coordinate.strike = new []{strike.Value};
            }
            else
            {
                coordinate.generic = new[] {new GenericDimension {name = "ATM", Value = "ATM"}};
            }
            return new PricingStructurePoint
                      {
                          measureType = measureType,
                          valueSpecified = true,
                          value = volatility,
                          coordinate = new []{coordinate}
                      };
        }

        /// <summary>
        /// Build up the matrix of cap/floor volatilities (expiry x strike)
        /// </summary>
        /// <param name="expiry">An array of option expiries as terms</param>
        /// <param name="volType">The type of the volatility surface at each expiry</param>
        /// <param name="strike">An array of strike prices. If this is an ATM only there will be only one strike.</param>
        /// <param name="volatility">A 2d array of the volatilities</param>
        private void GenerateVolatilityMatrix(string[] expiry, string[] volType, decimal[] strike, decimal[,] volatility)
        {
            if (expiry == null) return;
            var maxRows = expiry.Length;
            var maxCols = strike?.Length ?? 1;
            dataPoints.point = new PricingStructurePoint[maxRows * maxCols];
            for (var row = 0; row < maxRows; row++)
            {
                Period volExpiry = CreateInterval(expiry[row]);
                TimeDimension expiration = new TimeDimension { Items = new object[] { volExpiry } };
                AssetMeasureType measureType = new AssetMeasureType { Value = volType[row] };
                for (var column = 0; column < maxCols; column++)
                {
                    var current = row * maxCols + column;
                    decimal? nullableStrike = strike?[column];
                    dataPoints.point[current]
                        = CreatePricingStructurePoint(nullableStrike, volatility[row,column], expiration, measureType);
                }
            }
        }

        /// <summary>
        /// Build up the matrix of cap/floor volatilities (expiry x strike)
        /// </summary>
        /// <param name="expiry">An array of option expiries as terms</param>
        /// <param name="volType">The type of the volatility surface at each expiry</param>
        /// <param name="strike">An array of strike prices. If this is an ATM only there will be only one strike.</param>
        /// <param name="volatility">A 2d array of the volatilities</param>
        private void GenerateVolatilityMatrix(string[] expiry, string[] volType, decimal strike, decimal[] volatility)
        {
            if (expiry == null) return;
            var maxRows = expiry.Length;
            dataPoints.point = new PricingStructurePoint[maxRows];
            for (var row = 0; row < maxRows; row++)
            {
                Period volExpiry = CreateInterval(expiry[row]);
                TimeDimension expiration = new TimeDimension { Items = new object[] { volExpiry } };
                AssetMeasureType measureType = new AssetMeasureType { Value = volType[row] };
                dataPoints.point[row]
                    = CreatePricingStructurePoint(strike, volatility[row], expiration, measureType);
            }
        }

        /// <summary>
        /// Convert the term structure string representation to an interval
        /// </summary>
        /// <param name="p">a string representing a term structure</param>
        /// <returns></returns>
        private static Period CreateInterval(string p)
        {
            string alphaPart = "";
            decimal numPart = 0;
            // Split the term string into its parts
            LabelSplitter(p, ref alphaPart, ref numPart);
            Period period = new Period
                                {
                                    periodMultiplier = numPart.ToString(CultureInfo.InvariantCulture),
                                    period = EnumHelper.Parse<PeriodEnum>(alphaPart, true)
                                };
            return period;
        }

        /// <summary>
        /// Method to split and store the alpha and numeric parts of a label
        /// This method should be private to the element but is defined here for convenience
        /// </summary>
        /// <param name="label">The source label to split</param>
        /// <param name="alpha">The alpha part of the label</param>
        /// <param name="numeric">The number part of the label</param>
        private static void LabelSplitter(string label, ref string alpha, ref decimal numeric)
        {
            if (alpha == null) throw new ArgumentNullException(nameof(alpha));
            // Remove all spaces from the label.
            string tempLabel = label.Replace(" ", "");
            // Initialise the regular expressions that will be used to match
            // the necessary format.
            const string alphaPattern = "[a-zA-Z]+";
            const string numericPattern = "-*[0-9.]+";
            Regex alphaRegex = new Regex(alphaPattern, RegexOptions.IgnoreCase);
            Regex numericRegex = new Regex(numericPattern, RegexOptions.IgnoreCase);
            // Match the alpha and numeric components.
            MatchCollection alphaMatches = alphaRegex.Matches(tempLabel);
            MatchCollection numericMatches = numericRegex.Matches(tempLabel);
            alpha = alphaMatches.Count > 0 ? alphaMatches[0].Value.Substring(0, 1).ToUpper() : "D";
            numeric = numericMatches.Count > 0 ? Convert.ToDecimal(numericMatches[0].Value) : 0;
        }

        #endregion
    }
}