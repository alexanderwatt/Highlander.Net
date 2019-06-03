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

namespace FpML.V5r3.Reporting
{
    public partial class ExchangeRate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="spotRate"></param>
        /// <param name="forwardRate"></param>
        /// <param name="forwardPoints"></param>
        /// <returns></returns>
        public static ExchangeRate Create(string currency1, string currency2, QuoteBasisEnum quoteBasis,
                                         Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            var exchangeRate = new ExchangeRate
            {
                quotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rate = forwardRate,
                rateSpecified = true,
                spotRate = spotRate,
                spotRateSpecified = true
            };
            if(forwardPoints != null)
            {
                exchangeRate.forwardPointsField = (decimal)forwardPoints;
            }
            return exchangeRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="spotRate"></param>
        /// <returns></returns>
        public static ExchangeRate Create(string currency1, string currency2, QuoteBasisEnum quoteBasis,
                                         Decimal spotRate)
        {
            var exchangeRate = new ExchangeRate
            {
                quotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rate = spotRate,
                rateSpecified = true
            };
            return exchangeRate;
        }
    }
}
