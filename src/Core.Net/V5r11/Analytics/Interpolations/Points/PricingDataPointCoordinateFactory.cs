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

#region Usings

using System;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Interpolations.Points
{
    /// <summary>
    /// Produces pricing data point coordinates.
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
            coordinate.expiration[0] = new TimeDimension {Items = new object[] {pExpiry}};
            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension {Items = new object[] {pTerm}};
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
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(string expiry, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();
            var pExpiry = expiry != null ? PeriodHelper.Parse(expiry) : null;
            GenericDimension pGeneric = null;
            if (generic != null)
            {
                pGeneric = new GenericDimension { name = generic, Value = generic };
            }
            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = new TimeDimension { Items = new object[] { pExpiry } };
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
            coordinate.expiration[0] = new TimeDimension { Items = new object[] { pExpiry } };
            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension { Items = new object[] { pTerm } };
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
            coordinate.expiration[0] = new TimeDimension { Items = new object[] { pExpiry } };
            if (pTerm != null)
            {
                coordinate.term = new TimeDimension[1];
                coordinate.term[0] = new TimeDimension { Items = new object[] { pTerm } };
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
        /// <param name="maturityDate"></param>
        /// <param name="maturityTerm"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(DateTime expiryDate, string expiryTerm, 
            DateTime maturityDate, string maturityTerm, decimal strike, string generic)
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
            coordinate.term[0] = TimeDimensionFactory.Create(maturityDate, pMaturityTerm);
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
        /// <param name="maturityDate"></param>
        /// <param name="maturityTerm"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(DateTime expiryDate, Period expiryTerm,
        DateTime maturityDate, Period maturityTerm, decimal strike, string generic)
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
            coordinate.term[0] = TimeDimensionFactory.Create(maturityDate, pMaturityTerm);
            coordinate.strike = new[] { pStrike };
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }

        /// <summary>
        /// Creates a PricingDataPointCoordinate.
        /// </summary>
        /// <param name="expiryDate"></param>
        /// <param name="maturityDate"></param>
        /// <param name="maturityTerm"></param>
        /// <param name="strike"></param>
        /// <param name="generic"></param>
        /// <returns></returns>
        public static PricingDataPointCoordinate Create(DateTime expiryDate,
            DateTime maturityDate, Period maturityTerm, decimal strike, string generic)
        {
            var coordinate = new PricingDataPointCoordinate();
            var pMaturityTerm = maturityTerm;
            var pStrike = strike;
            GenericDimension pGeneric = null;
            if (generic != null)
            {
                pGeneric = new GenericDimension { name = generic, Value = generic };
            }
            coordinate.expiration = new TimeDimension[1];
            coordinate.expiration[0] = TimeDimensionFactory.Create(expiryDate);
            coordinate.term = new TimeDimension[1];
            coordinate.term[0] = TimeDimensionFactory.Create(maturityDate, pMaturityTerm);
            coordinate.strike = new[] { pStrike };
            if (pGeneric != null)
                coordinate.generic = new[] { pGeneric };
            return coordinate;
        }
    }
}
