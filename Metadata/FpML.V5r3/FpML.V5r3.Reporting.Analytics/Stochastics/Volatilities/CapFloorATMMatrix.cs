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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Stochastics.SABR;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// This class is used to house raw Cap/Floor ATM matrices for publication on the Orion network
    /// The parameter constructor picks up a matrix from a spreadsheet and populates the underlying FpML
    /// </summary>
    [Serializable]
    [XmlRoot("CapFloorATMMatrix", IsNullable = false)]
    public class CapFloorATMMatrix : SABRDataMatrix
    {
        #region CapFloorATMMatrix Extensions

        /// <summary>
        /// A settings object that is used to define the SABR parameters.
        /// It is used in the building of Calibration Engines from the SABR data
        /// </summary>
        public BasicAssetValuation Settings
        {
            get;
            set;
        }

        #endregion

        #region Public Properties

        ///<summary>
        ///</summary>
        public string MatrixType
        {
            get
            {
                if (dataPoints?.point?[0].measureType == null)
                    return string.Empty;
                return dataPoints.point[0].measureType.Value;
            }
        }

        #endregion

        #region Constructors

        // Default constructor for FpML Schema
        ///<summary>
        ///</summary>
        public CapFloorATMMatrix()
        {
            _defaultAlpha = "D";
        }

        /// <summary>
        /// Construct an FpML MultiDimensionalPricingData from the spreadsheet values
        /// </summary>
        /// <param name="headers">The columns to write</param>
        /// <param name="data">The values to store</param>
        /// <param name="settings">The settings used by this matrix</param>
        /// <param name="valueDate">The valuation date</param>
        /// <param name="surfaceId">The id of this surface</param>
        public CapFloorATMMatrix(string[] headers, object[][] data, object[][] settings, DateTime valueDate, string surfaceId)
            : this()
        {
            // Include a QuotedAssetSet to hold the Settings object used to generate this SABR parameters Matrix
            if (settings != null)
                Settings = AssignSettings(settings);
            id = surfaceId;
            baseDate = new IdentifiedDate { Value = valueDate };
            var rows = data.GetUpperBound(0) + 1;
            var points = new List<PricingStructurePoint>();
            int expiry = Find("Expiry", headers);
            int atm = Find("ATM", headers);
            int type = Find("Type", headers);
            int ppd = Find("PPD", headers);
            for (int row = 0; row < rows; row++)
            {
                var point = new PricingStructurePoint();
                if (data[row][0] != null)
                {
                    // Populate each data point from the data array
                    for (var column = 0; column < headers.Length; column++)
                    {
                        object datum = data[row][column];
                        if (column == expiry)
                        {
                            // Add the coordinate value (the expiry)
                            var expiration = PeriodHelper.Parse(datum.ToString());
                            point.coordinate = new PricingDataPointCoordinate[1];
                            point.coordinate[0] = new PricingDataPointCoordinate {expiration = new TimeDimension[1]};
                            point.coordinate[0].expiration[0] = new TimeDimension {Items = new object[] {expiration}};
                        }
                        else if (column == atm)
                        {
                            point.measureType = new AssetMeasureType {Value = "ATM"};
                            point.valueSpecified = true;
                            point.value = Convert.ToDecimal(datum);
                        }
                        else if (column == ppd)
                        {
                            point.measureType = new AssetMeasureType {Value = "PPD"};
                            point.valueSpecified = true;
                            point.value = Convert.ToDecimal(datum);
                        }
                        else if (column == type)
                        {
                            point.cashflowType = new CashflowType { Value = datum.ToString() };
                        }
                        else
                        {
                            throw new ArgumentException("Unknown column name: " + column);
                        }
                    }
                }
                points.Add(point);
            }
            dataPoints = new MultiDimensionalPricingData {point = points.ToArray()};
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Get the volatilities for all expiries/strikes
        /// For this there is only a single strike (ATM)
        /// </summary>
        /// <returns></returns>
        public decimal[][] GetVolatilities()
        {
            decimal[][] vols = null;
            var atmVols = dataPoints.point.Select(point => point.valueSpecified ? point.value : 0).ToList();
            if (atmVols.Count > 0)
            {
                vols = new decimal[atmVols.Count][];
                for (var row = 0; row < atmVols.Count; row++)
                    vols[row] = new[] { atmVols[row] };
            }
            return vols;
        }

        /// <summary>
        /// Get the headers
        /// </summary>
        /// <returns></returns>
        public string[] GetHeaders()
        {
            // Hard-coded assumption. This class will only ever deal with Expiry, ATM & Type
            // So we'll just create and return these.
            // Not any more. Must take into account we may have ppd values instead
            return new[] { "Expiry", MatrixType, "Type" };
        }

        /// <summary>
        /// Get the expiries
        /// </summary>
        /// <returns></returns>
        public string[] GetExpiries()
        {
            string[] expiries = null;
            var exp = new List<string>();
            foreach (PricingStructurePoint point in dataPoints.point)
            {
                string tenor = string.Empty;
                if (point.coordinate?[0].expiration?[0].Items != null)
                {
                    tenor = ((Period)point.coordinate[0].expiration[0].Items[0]).ToString();
                }
                if (!string.IsNullOrEmpty(tenor))
                    exp.Add(tenor);
            }
            if (exp.Count > 0)
            {
                expiries = new string[exp.Count];
                exp.CopyTo(expiries);
            }
            return expiries;
        }

        /// <summary>
        /// Get the volatility types
        /// </summary>
        /// <returns></returns>
        public string[] GetVolatilityTypes()
        {
            string[] types = null;
            var typ = (from point in dataPoints.point where point.cashflowType != null select point.cashflowType.Value).ToList();
            if (typ.Count > 0)
            {
                types = new string[typ.Count];
                typ.CopyTo(types);
            }
            return types;
        }

        /// <summary>
        /// Get the settings object associated with this ATM CapFloor matrix
        /// </summary>
        /// <returns></returns>
        public object[][] GetVolatilitySettings()
        {
            object[][] retvals = null;
            var settings = new Dictionary<string, object>();
            // Loop through each of the settings in the _settings
            // Each setting is a separate quote
            foreach (BasicQuotation quote in Settings.quote)
            {
                string name = quote.measureType != null ? quote.measureType.Value : string.Empty;
                if (!string.IsNullOrEmpty(name))
                {
                    if (quote.cashflowType != null)
                    {
                        // A string
                        settings.Add(name, quote.cashflowType.Value);
                    }
                    else if (quote.valuationDateSpecified)
                    {
                        // A DateTime
                        settings.Add(name, quote.valuationDate);
                    }
                    else if (quote.valueSpecified)
                    {
                        // a decimal
                        settings.Add(name, quote.value);
                    }
                }
            }
            // Now convert the recovered settings to a 2d array
            if (settings.Count > 0)
            {
                retvals = new object[settings.Count][];
                int row = 0;
                foreach (string key in settings.Keys)
                {
                    retvals[row] = new object[2];
                    retvals[row][0] = key;
                    retvals[row][1] = settings[key];
                    row++;
                }
            }
            return retvals;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Add the settings used by this ATM vol object
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static BasicAssetValuation AssignSettings(object[][] settings)
        {
            var assets = new BasicAssetValuation();
            int rows = settings.Length;
            int startRow = settings[0][0].ToString().ToLower() == "settings" ? 1 : 0;
            var quotes = new List<BasicQuotation>();
            for (int row = startRow; row < rows; row++)
            {
                if (settings[row][0] != null)
                {
                    var quote
                        = new BasicQuotation
                        {
                            measureType = new AssetMeasureType { Value = settings[row][0].ToString() }
                        };
                    object value = settings[row][1];
                    if (value is string)
                    {
                        // A string
                        quote.cashflowType = new CashflowType { Value = value.ToString() };
                    }
                    else if (value is DateTime)
                    {
                        // A DateTime
                        quote.valuationDateSpecified = true;
                        quote.valuationDate = Convert.ToDateTime(value);
                    }
                    else
                    {
                        // a decimal
                        quote.valueSpecified = true;
                        quote.value = Convert.ToDecimal(value);
                    }
                    quotes.Add(quote);
                }
            }
            assets.quote = quotes.ToArray();
            return assets;
        }

        #region Static Methods

        /// <summary>
        /// Return the column (0-based) that matches the candidate from the array
        /// </summary>
        /// <param name="term"></param>
        /// <param name="headers"></param>
        /// <returns>The column or -1 on failure</returns>
        private static int Find(string term, string[] headers)
        {
            int column = -1;

            for (int i = 0; i < headers.Length; i++)
            {
                if (headers[i].ToLowerInvariant().Contains(term.ToLowerInvariant()))
                {
                    column = i;
                    break;
                }
            }
            return column;
        }

        #endregion
        
        #endregion
    }
}