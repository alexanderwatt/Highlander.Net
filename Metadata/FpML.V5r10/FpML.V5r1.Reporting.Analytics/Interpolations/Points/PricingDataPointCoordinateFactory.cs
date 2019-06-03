using System;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// Produces pricingdatapoint coordinates.
    /// </summary>
    public static class PricingDataPointCoordinateFactory
    {
        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(string expiry, string term, string strike, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();

            var pExpiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
            var pTerm = term != null ? PeriodHelper.Parse(term) : null;
            GenericDimension pGeneric = null;

            if (generic != null)
            {
                pGeneric = new GenericDimension {name = generic, Value = generic};
            }

            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = new TimeDimension {Items = new[] {pExpiry}};

            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension {Items = new[] {pTerm}};
            }
            if (strike != null)
            {
                coordinate.strike = new[] { decimal.Parse(strike) };
            }
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }

        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(string expiry, string term, decimal strike)
        {
            var coordinate = new PricingDataPointCoordinate();

            var pExpiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
            var pTerm = term != null ? PeriodHelper.Parse(term) : null;
            var pStrike = strike;

            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = new TimeDimension { Items = new[] { pExpiry } };

            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension { Items = new[] { pTerm } };
            }

            coordinate.strike = new[] { pStrike };

            return coordinate;
        }

        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="term"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(string expiry, string term, decimal strike, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();

            var pExpiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
            var pTerm = term != null ? PeriodHelper.Parse(term) : null;
            var pStrike = strike;
            GenericDimension pGeneric = null;

            if (generic != null)
            {
                pGeneric = new GenericDimension { name = generic, Value = generic };
            }

            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = new TimeDimension { Items = new[] { pExpiry } };

            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension { Items = new[] { pTerm } };
            }

            coordinate.strike = new[] { pStrike };
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }

        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiryDate"></param>
        /// <param name="expiryTerm"></param>
        /// <param name="maturitydate"></param>
        /// <param name="maturityTerm"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(DateTime expiryDate, string expiryTerm, 
            DateTime maturitydate, string maturityTerm, decimal strike, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();

            var pExpiryTerm = expiryTerm != null ? PeriodHelper.Parse(expiryTerm) : null;
            var pMaturityTerm = maturityTerm != null ? PeriodHelper.Parse(maturityTerm) : null;
            var pStrike = strike;
            GenericDimension pGeneric = null;

            if (generic != null)
            {
                pGeneric = new GenericDimension { name = generic, Value = generic };
            }

            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = TimeDimensionFactory.Create(expiryDate, pExpiryTerm);

            coordinate.term = new TimeDimension[1];
            coordinate.term[0] = TimeDimensionFactory.Create(maturitydate, pMaturityTerm);

            coordinate.strike = new[] { pStrike };
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }

        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiryDate"></param>
        /// <param name="expiryTerm"></param>
        /// <param name="maturitydate"></param>
        /// <param name="maturityTerm"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(DateTime expiryDate, Period expiryTerm,
    DateTime maturitydate, Period maturityTerm, decimal strike, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();

            var pExpiryTerm = expiryTerm;
            var pMaturityTerm = maturityTerm;
            var pStrike = strike;
            GenericDimension pGeneric = null;

            if (generic != null)
            {
                pGeneric = new GenericDimension { name = generic, Value = generic };
            }

            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = TimeDimensionFactory.Create(expiryDate, pExpiryTerm);

            coordinate.term = new TimeDimension[1];
            coordinate.term[0] = TimeDimensionFactory.Create(maturitydate, pMaturityTerm);

            coordinate.strike = new[] { pStrike };
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }
    }
}
