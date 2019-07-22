/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.Analytics.Stochastics.SABR;
using FpML.V5r10.Reporting;

namespace FpML.V5r10.Reporting.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// A data class to wrap the input data
    /// This class models a Swaption grid. It must have both an Expiry and a Tenor
    /// </summary>
    [Serializable]
    [XmlRoot("SwaptionVolatilityMatrix", IsNullable = false)]
    //[XmlRoot("VolatilityMatrix")]
    public class SwaptionVolatilityMatrix : VolatilityMatrix
    {
        #region Public Enumerations
        
        /// <summary>
        /// An enumerated type defining the SABR Surface parameters stored within this SABRSwaptionPubSubGrid
        /// </summary>
        public enum ParameterType
        {
            /// <summary>
            /// The beta parameter.
            /// </summary>
            Beta,

            /// <summary>
            /// The  nu parameter.
            /// </summary>
            Nu,

            /// <summary>
            /// The rho parameter.
            /// </summary>
            Rho
        };

        #endregion

        /// <summary>
        /// A settings object that is used to define the SABR parameters.
        /// It is used in the building of Calibration Engines from the SABR data
        /// </summary>
        public BasicAssetValuation Settings { get; set; }

        private const int IdentifierFieldCount = 3;

        // Optimization to create a fast lookup for parameters by expiry/tenor
        private Dictionary<SABRKey, GridParameters> _grid;
 
        #region Constructors

        /// <summary>
        /// default constructor to keep XMLSerialization happy
        /// </summary>
        public SwaptionVolatilityMatrix()
        { }

        /// <summary>
        /// Create a grid from an array of names and complex array of data
        /// The data will contain strings and decimals
        /// </summary>
        /// <param name="pSettings">The settings object used to generate these SABR parameters</param>
        /// <param name="fields">The headers used to identify the SABR parameters</param>
        /// <param name="data">An array of SABR parameters and expiry/tenor pairs</param>
        /// <param name="valueDate">The valuation date</param>
        /// <param name="surfaceId">The id for this matrix</param>
        public SwaptionVolatilityMatrix(object[][] pSettings, string[] fields, object[][] data, DateTime valueDate, string surfaceId)
        {
            // Include a QuotedAssetSet to hold the Settings object used to generate this SABR parameters Matrix
            Settings = AssignSettings(pSettings);
            // Set the id for this matrix
            id = surfaceId;
            // Set the value date for this vol matrix
            baseDate = new IdentifiedDate {Value = valueDate};
            // Set the buildDate for this matrix
            buildDateTime = DateTime.Now;
            // Create the dataPoints structure. This will hold the matrix data
            dataPoints = new MultiDimensionalPricingData();
            var columns = fields.Length;
            var rows = data.Length;
            dataPoints.point = new PricingStructurePoint[rows * IdentifierFieldCount];
            var point = 0;
            // Loop through the arrays to populate the underlying VolatilityMatrix
            for (var gRows = 0; gRows < rows; gRows++)
            {
                // Extract the expiry/tenor information for creating
                var expiry = PeriodHelper.Parse(data[gRows][0].ToString());
                var tenor = PeriodHelper.Parse(data[gRows][1].ToString());
                for (var gCols = columns - IdentifierFieldCount; gCols < columns; gCols++)
                {
                    dataPoints.point[point] = new PricingStructurePoint {coordinate = new PricingDataPointCoordinate[1]};
                    // Set up the co-ordinate (Expiry/Term) for each point
                    dataPoints.point[point].coordinate[0] = new PricingDataPointCoordinate
                                                                {expiration = new TimeDimension[1]};
                    // Set the Expiry for the co-ordinate point
                    dataPoints.point[point].coordinate[0].expiration[0] = new TimeDimension
                                                                              {Items = new object[] {expiry}};
                    // Set the Term for the co-ordinate point
                    dataPoints.point[point].coordinate[0].term = new TimeDimension[1];
                    dataPoints.point[point].coordinate[0].term[0] = new TimeDimension {Items = new object[] {tenor}};
                    // Add the quotation characteristics for the point
                    // We will only record a value and the measure type
                    dataPoints.point[point].valueSpecified = true;
                    dataPoints.point[point].value = Convert.ToDecimal(data[gRows][gCols]);
                    dataPoints.point[point].measureType = new AssetMeasureType {Value = fields[gCols]};
                    point++;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Check whether the grid has a row matching the expiry/tenor pair
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <returns></returns>
        public bool HasRow(string expiry, string tenor)
        {
            if (_grid == null)
                Populate();
            return _grid.ContainsKey(new SABRKey(expiry, tenor));
        }

        /// <summary>
        /// Get the grid row that corresponds to the expiry/tenor pair.
        /// The Grid parameter hold the PPD, Beta, Nu and Rho used to
        /// recreate this expiry/tenor surface
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <returns></returns>
        public GridParameters GetRow(string expiry, string tenor)
        {
            if (_grid == null)
                Populate();
            return _grid[new SABRKey(expiry, tenor)];
        }

        /// <summary>
        /// Get the named parameter for the given expiry/tenor pairing
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public decimal GetParameter(string expiry, string tenor, ParameterType parameter)
        {
            GridParameters row = GetRow(expiry, tenor);
            switch (parameter)
            {
                case ParameterType.Beta:
                    return row.Beta;
                case ParameterType.Nu:
                    return row.Nu;
                case ParameterType.Rho:
                    return row.Rho;
                default:
                    throw new ArgumentException("Expiry/tenor pair is not contained by this Grid.");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// A method to generate values for the fast lookup grid
        /// This is a fire once method
        /// </summary>
        private void Populate()
        {
            _grid = new Dictionary<SABRKey, GridParameters>(new SABRKey());
            foreach (var point in dataPoints.point)
            {
                var key = new SABRKey(((Period) point.coordinate[0].expiration[0].Items[0]).ToString(),
                                      ((Period) point.coordinate[0].term[0].Items[0]).ToString());
                if (!_grid.ContainsKey(key))
                    _grid.Add(key, new GridParameters());
                var param = _grid[key];
                if (point.measureType.Value == ParameterType.Beta.ToString())
                    param.Beta = point.value;
                else if (point.measureType.Value == ParameterType.Nu.ToString())
                    param.Nu = point.value;
                else if (point.measureType.Value == ParameterType.Rho.ToString())
                    param.Rho = point.value;
                _grid[key] = param;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static BasicAssetValuation AssignSettings(object[][] settings)
        {
            var assets = new BasicAssetValuation();
            var cols = settings[0].Length;
            var i = 0;
            assets.quote = new BasicQuotation[cols];
            for (var c = 0; c < cols; c++)
            {
                assets.quote[i] = new BasicQuotation
                                      {
                                          measureType = new AssetMeasureType {Value = Convert.ToString(settings[0][c])}
                                      };
                if (settings[1][c] is string)
                {
                    assets.quote[i].valueSpecified = false;
                    assets.quote[i].cashflowType = new CashflowType { Value = Convert.ToString(settings[1][c]) };
                }
                else
                {
                    assets.quote[i].valueSpecified = true;
                    assets.quote[i].value = Convert.ToDecimal(settings[1][c]);
                }
                i++;
            }
            return assets;
        }

        #endregion

        #region Inner Classes

        /// <summary>
        /// An inner class that is used to provide quick access to the parameters stored in the PubSub grid.
        /// </summary>
        public struct GridParameters
        {
            #region Properties

            /// <summary>
            /// The Beta used to generate the Nu and Rho parameters
            /// </summary>
            public decimal Beta { get; set; }

            /// <summary>
            /// The Nu parameter needed to recreate the surface
            /// </summary>
            public decimal Nu { get; set; }

            /// <summary>
            /// The Rho parameter needed to recreate the surface
            /// </summary>
            public decimal Rho { get; set; }

            #endregion
        }

        #endregion
    }
}