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
using System.Xml.Serialization;
using FpML.V5r3.Reporting.Helpers;
using FpML.V5r3.Reporting;
using Orion.Analytics.Interpolations;

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// A class to represent PPD data (MultiPointPricingData)
    /// </summary>
    [Serializable]
    [XmlRoot("PPDGrid", IsNullable = false)]
    public class SwaptionPPDGrid : PPDGrid
    {
        /// <summary>
        /// A bilinear interpolator for returning interpolated ppd's given an expiry/tenor pair
        /// </summary>
        private BilinearInterpolation _interp;

        #region Constructors

        ///<summary>
        /// A swaption ppd grid.
        ///</summary>
        public SwaptionPPDGrid()
        { }

        ///<summary>
        /// A swaption ppd grid.
        ///</summary>
        ///<param name="rows"></param>
        ///<param name="cols"></param>
        ///<param name="data"></param>
        public SwaptionPPDGrid(string[] rows, string[] cols, object[][] data)
            : this()
        {
            measureType = new AssetMeasureType { Value = "PPD" };
            quoteUnits = new PriceQuoteUnits { Value = "points" };
            // build the co-ordinate tree.. data[row][column]
            // row = expiry, column = tenor
            var points = new List<PricingStructurePoint>();
            for (int row = 0; row < rows.Length; row++)
            {
                for (int col = 0; col < cols.Length; col++)
                {
                    if (data[row][col] != null)
                    {
                        var currentPoint = new PricingStructurePoint { coordinate = new PricingDataPointCoordinate[1] };
                        // Set up the coordinate for this point
                        currentPoint.coordinate[0] = new PricingDataPointCoordinate { expiration = new TimeDimension[1] };
                        // The row - Expiry
                        currentPoint.coordinate[0].expiration[0] = new TimeDimension
                        {
                            Items =
                                new object[] { PeriodHelper.Parse(rows[row]) }
                        };
                        // The column - Tenor
                        currentPoint.coordinate[0].term = new TimeDimension[1];
                        currentPoint.coordinate[0].term[0] = new TimeDimension
                        {
                            Items = new object[] { PeriodHelper.Parse(cols[col]) }
                        };
                        // The data value
                        currentPoint.valueSpecified = true;
                        currentPoint.value = Convert.ToDecimal(data[row][col]);
                        points.Add(currentPoint);
                    }
                }
            }
            point = points.ToArray();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get an indicative PPD for a given expiry/tenor pair
        /// If the pair is not in the grid use interpolation to derive the result
        /// </summary>
        /// <param name="expiry">The expiry term</param>
        /// <param name="tenor">The tenor term</param>
        /// <returns>A PPD value for the pair</returns>
        public decimal GetPPD(string expiry, string tenor)
        {
            return GetPPD(PeriodHelper.Parse(expiry), PeriodHelper.Parse(tenor));
        }

        /// <summary>
        /// Get an indicative PPD for a given expiry/tenor pair
        /// If the pair is not in the grid use interpolation to derive the result
        /// </summary>
        /// <param name="expiry">The expiry as an Interval</param>
        /// <param name="tenor">The tenor as an Interval</param>
        /// <returns></returns>
        public decimal GetPPD(Period expiry, Period tenor)
        {
            return GetPPD(expiry.ToYearFraction(), tenor.ToYearFraction());
        }

        /// <summary>
        /// Get an indicative PPD for a given expiry/tenor pair
        /// If the pair is not in the grid use interpolation to derive the result
        /// </summary>
        /// <param name="expiryYF">The expiry as a Year Fraction</param>
        /// <param name="tenorYF">The tenor as a Year Fraction</param>
        /// <returns></returns>
        public decimal GetPPD(double expiryYF, double tenorYF)//TODO convert to newer version.
        {
            if (_interp == null)
            {
                if (ExtractTerms(out var expiry, out var tenor, out var values))
                {
                    _interp = new BilinearInterpolation(ref tenor, ref expiry, ref values);
                }
            }
            return (decimal)_interp.Interpolate(tenorYF, expiryYF);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This routine extracts and converts Intervals from the PricingStructurePoint array that holds
        /// the expiry/tenor pairs
        /// </summary>
        /// <param name="expiry">An array of expiries</param>
        /// <param name="tenor"></param>
        /// <param name="ppd"></param>
        /// <returns></returns>
        private bool ExtractTerms(out double[] expiry, out double[] tenor, out double[,] ppd)
        {
            var extracted = false;

            expiry = null;
            tenor = null;
            ppd = null;

            var expValues = new List<double>();
            var tenValues = new List<double>();

            // Only unwrap if there are points in this grid
            // First pass extracts the expiry/tenor pairs
            if (point != null)
            {
                foreach (var p in point)
                {
                    // Only get the term structure if it's available
                    if (p.coordinate == null) continue;
                    extracted = true;
                    var expiryTerm = (Period)p.coordinate[0].expiration[0].Items[0];
                    var tenorTerm = (Period)p.coordinate[0].term[0].Items[0];

                    var expiryYF = expiryTerm.ToYearFraction();
                    var tenorYF = tenorTerm.ToYearFraction();

                    if (!expValues.Contains(expiryYF))
                        expValues.Add(expiryYF);

                    if (!tenValues.Contains(tenorYF))
                        tenValues.Add(tenorYF);
                }

                // Set the expiry array
                if (expValues.Count > 0)
                {
                    expiry = new double[expValues.Count];
                    expValues.CopyTo(expiry);
                }
                else
                    extracted = false;

                // Set the tenor array
                if (tenValues.Count > 0)
                {
                    tenor = new double[tenValues.Count];
                    tenValues.CopyTo(tenor);
                }
                else
                    extracted = false;
            }

            // Only unwrap if there are points in this grid
            // Second pass extracts the values at each expiry/tenor pair
            if (extracted)
            {
                ppd = new double[expiry.Length, tenor.Length];
                foreach (var p in point)
                {
                    // just to be safe
                    var expiryTerm = (Period)p.coordinate[0].expiration[0].Items[0];
                    var tenorTerm = (Period)p.coordinate[0].term[0].Items[0];

                    int e = expValues.IndexOf(expiryTerm.ToYearFraction());
                    int t = tenValues.IndexOf(tenorTerm.ToYearFraction());

                    ppd[e, t] = (double)p.value;
                }
            }
            return extracted;
        }

        #endregion
    }
}