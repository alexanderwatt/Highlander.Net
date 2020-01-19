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

using System;
using System.Xml.Serialization;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Stochastics.SABR;
using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Analytics.V5r3.Rates
{
    /// <summary>
    /// This is data representation of a forward rates data grid
    /// The grid will have rows representing expiries and columns representing tenors
    /// </summary>
    [Serializable]
    [XmlRoot("VolatilityMatrix", IsNullable = false)]
    public class ForwardRatesMatrix : SABRDataMatrix
    {
        #region Constructors

        /// <summary>
        /// Default constructor to use for deserialization
        /// </summary>
        public ForwardRatesMatrix()
        { }

        /// <summary>
        /// Construct a forward rates matrix from a grid
        /// This uses a default grid name 
        /// </summary>
        /// <param name="expiry">An array of expiry terms as strings</param>
        /// <param name="tenor">An array of swap tenor terms as strings</param>
        /// <param name="data">The forward rates for each expiry/tenor pair</param>
        public ForwardRatesMatrix(string[] expiry, string[] tenor, decimal[][] data)
            : this(expiry, tenor, data, "Forward Rates")
        { }

        /// <summary>
        /// Construct a forward rates matrix from a grid
        /// </summary>
        /// <param name="expiry">An array of expiry terms as strings</param>
        /// <param name="tenor">An array of swap tenor terms as strings</param>
        /// <param name="data">The forward rates for each expiry/tenor pair</param>
        /// <param name="matrixId">An id to associate with this array</param>
        public ForwardRatesMatrix(string[] expiry, string[] tenor, decimal[][] data, string matrixId)
        {
            // A default id
            id = matrixId;
            dataPoints = new MultiDimensionalPricingData
                             {
                                 point = new PricingStructurePoint[expiry.Length*tenor.Length]
                             };
            // Set the points held by the matrix to be the total (expiry x tenor) values
            var point = 0;
            // Loop through our expiry/tenor and data arrays to build the matrix points
            for (var row = 0; row < expiry.Length; row++)
            {
                // Convert the expiry to an Interval
                var rateExpiry = PeriodHelper.Parse(expiry[row]);
                for (var col = 0; col < tenor.Length; col++)
                {
                    // Convert the tenor to an interval
                    var rateTenor = PeriodHelper.Parse(tenor[col]);
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
                    point++;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the asset value at the expiry/tenor pair from the underlying grid
        /// </summary>
        /// <param name="expiry">The expiry term as a string</param>
        /// <param name="tenor">The tenor term as a string</param>
        /// <exception cref="ArgumentException">this exception is thrown if the expiry/tenor pair is invalid</exception>
        /// <returns></returns>
        public decimal GetAssetPrice(string expiry, string tenor)
        {
            return GetAssetPrice(PeriodHelper.Parse(expiry), PeriodHelper.Parse(tenor));
        }

        /// <summary>
        /// Get the asset value at the expiry/tenor pair from the underlying grid
        /// </summary>
        /// <param name="expiry">The expiry term as an FpML Interval</param>
        /// <param name="tenor">The tenor term as an FpML Interval</param>
        /// <exception cref="ArgumentException">this exception is thrown if the expiry/tenor pair is invalid</exception>
        /// <returns></returns>
        public decimal GetAssetPrice(Period expiry, Period tenor)
        {
            foreach (PricingStructurePoint point in dataPoints.point)
            {
                Period intExp = (Period)point.coordinate[0].expiration[0].Items[0];
                if (!expiry.Equals(intExp)) continue;
                Period intTen = (Period)point.coordinate[0].term[0].Items[0];
                if (tenor.Equals(intTen))
                    return point.value;
            }
            throw new ArgumentException("Invalid Expiry/Tenor pair supplied.");
        }

        #endregion
    }
}