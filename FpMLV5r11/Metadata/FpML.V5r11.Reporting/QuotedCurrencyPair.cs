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

namespace FpML.V5r11.Reporting
{
    public partial class QuotedCurrencyPair
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <returns></returns>
        public static QuotedCurrencyPair Create(string currency1, string currency2, QuoteBasisEnum quoteBasis)
        {
            var quotedCurrencyPair = new QuotedCurrencyPair
            {
                currency1 = Parse(currency1),
                currency2 = Parse(currency2),
                quoteBasis = quoteBasis
            };

            return quotedCurrencyPair;
        }

        private static Currency Parse(string currencyCode)
        {
            var currency = new Currency { Value = currencyCode };
            return currency;
        }
    }
}
