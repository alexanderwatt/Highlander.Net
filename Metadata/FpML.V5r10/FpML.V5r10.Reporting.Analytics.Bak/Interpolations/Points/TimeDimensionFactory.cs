using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// A helper to create timedimensions.
    /// </summary>
    public static class TimeDimensionFactory
    {
        /// <summary>
        /// Parses a timedimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry)
        {
            var pExpiry = expiry;
            var dimension = new TimeDimension { Items = new object[] { pExpiry } };
            return dimension;
        }

        /// <summary>
        /// Parses a timedimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry, string term)
        {
            var pExpiry = expiry;
            var pTerm = term != null ? PeriodHelper.Parse(term) : null;
            var dimension = pTerm != null ? new TimeDimension { Items = new object[] { pExpiry, pTerm } } : new TimeDimension { Items = new object[] { pExpiry} };
            return dimension;
        }

        /// <summary>
        /// Parses a timedimension from strings.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static TimeDimension Create(DateTime expiry, Period term)
        {
            var pExpiry = expiry;
            var pTerm = term;
            var dimension = pTerm != null ? new TimeDimension { Items = new object[] { pExpiry, pTerm } } : new TimeDimension { Items = new object[] { pExpiry } };
            return dimension;
        }
    }
}
