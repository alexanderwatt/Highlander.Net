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
using System.Text.RegularExpressions;
using Orion.Util.Helpers;

namespace FpML.V5r3.Reporting.Helpers
{
    public class PeriodHelper
    {
         #region Static Methods

        private const string AlphaPattern = "[a-zA-Z]+";
        private const string NumericPattern = "-*[0-9.]+";
        private static readonly Regex AlphaRegex = new Regex(AlphaPattern, RegexOptions.IgnoreCase);
        private static readonly Regex NumericRegex = new Regex(NumericPattern, RegexOptions.IgnoreCase);

        /// <summary>
        /// Method to split a Period string into its constituent parts and build an Period from them
        /// </summary>
        /// <param name="term">The term to convert
        /// <example>1M, 2W, 1yr, 14day</example>
        /// </param>
        public static Period Parse(string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                throw new ArgumentNullException(nameof(term));
            }
            // Remove all spaces from the string.
            string tempLabel = term.Replace(" ", "").ToUpper();
            //Filter the strings and map to valid periods.
            switch (tempLabel)
            {
                case "TN":
                case "SN":
                case "ON":
                    tempLabel = "1D";
                    break;
                case "SP":
                    tempLabel = "2D";
                    break;
            }
            // Match the alpha and numeric components.
            //
            MatchCollection alphaMatches = AlphaRegex.Matches(tempLabel);
            MatchCollection numericMatches = NumericRegex.Matches(tempLabel);
            if ((numericMatches == null || numericMatches.Count == 0) || (alphaMatches == null || alphaMatches.Count == 0))
            {
                throw new ArgumentException($"'{term}' string value has not been recognised as interval.");
            }
            var result = new Period
            {
                periodMultiplier = numericMatches[0].Value,
                period = EnumHelper.Parse<PeriodEnum>(alphaMatches[0].Value.Substring(0, 1), true),
                periodSpecified = true
            };

            return result;
        }

        /// <summary>
        /// Tries to parse the period
        /// </summary>
        public static bool TryParse(string s, out Period period)
        {
            Boolean result = false;
            try
            {
                period = Parse(s);
                result = true;
            }
            catch
            {
                period = null;
            }
            return result;
        }

        #endregion
    }
}
