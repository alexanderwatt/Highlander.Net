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

        ///// <summary>
        ///// Convert the term structure string representation to an interval
        ///// </summary>
        ///// <param name="p">a string representing a term structure</param>
        ///// <returns></returns>
        //protected Interval CreateInterval(string p)
        //{
        //    var alphaPart = string.Empty;
        //    decimal numPart = 0;

        //    // Split the term string into its parts
        //    LabelSplitter(p, ref alphaPart, ref numPart);

        //    var i = new Interval();
        //    i.periodMultiplier = numPart.ToString();

        //    // Convert the alpha to a PeriodEnum
        //    switch (alphaPart)
        //    {
        //        case "D":
        //            i.period = PeriodEnum.D;
        //            break;
        //        case "M":
        //            i.period = PeriodEnum.M;
        //            break;
        //        case "Y":
        //            i.period = PeriodEnum.Y;
        //            break;
        //        default:
        //            i.period = PeriodEnum.M;
        //            break;
        //    }
        //    return i;
        //}

        #endregion
        #endregion
    }
}