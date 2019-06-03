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
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using FpML.V5r3.Reporting;

namespace Orion.Analytics.Stochastics.SABR
{
    /// <summary>
    /// Base class (non-FpML) providing some default 
    /// </summary>
    [Serializable]
    [XmlRoot("VolatilityMatrix", IsNullable = false)]
    public class SABRDataMatrix : VolatilityMatrix
    {
        #region Protected Alpha Default

        /// <summary>
        /// A default to use if there is no alpha label part present
        /// </summary>
        protected string _defaultAlpha;

        #endregion

        #region Constructor

        /// <summary>
        /// A simple default constructor that initializes the alpha default
        /// </summary>
        public SABRDataMatrix()
        {
            _defaultAlpha = "Y";
        }

        #endregion

        #region Protected Methods

        #region Static Methods

        /// <summary>
        /// Method to split and store the alpha and numeric parts of a label
        /// Default is for any label with no period to be a Y(ear)
        /// </summary>
        /// <param name="label">The source label to split</param>
        /// <param name="alpha">The alpha part of the label</param>
        /// <param name="numeric">The number part of the label</param>
        protected void LabelSplitter(string label, ref string alpha, ref decimal numeric)
        {
            // Remove all spaces from the label.
            var tempLabel = label.Replace(" ", "");
            // Initialise the regular expressions that will be used to match
            // the necessary format.
            const string alphaPattern = "[a-zA-Z]+";
            const string numericPattern = "-*[0-9.]+";
            var alphaRegex = new Regex(alphaPattern, RegexOptions.IgnoreCase);
            var numericRegex = new Regex(numericPattern, RegexOptions.IgnoreCase);
            // Match the alpha and numeric components.
            var alphaMatches = alphaRegex.Matches(tempLabel);
            var numericMatches = numericRegex.Matches(tempLabel);
            // Generate the alpha and numeric elements with null checking
            alpha = (alphaMatches.Count > 0) ? alphaMatches[0].Value.Substring(0, 1).ToUpper() : _defaultAlpha;
            numeric = (numericMatches.Count > 0) ? Convert.ToDecimal(numericMatches[0].Value) : 0;
        }

        #endregion

        #endregion
    }
}