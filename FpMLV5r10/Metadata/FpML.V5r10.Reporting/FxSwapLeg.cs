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

#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting
{
    public partial class FxSwapLeg
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<string> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            result.AddRange(exchangedCurrency1.GetRequiredPricingStructures());
            result.AddRange(exchangedCurrency2.GetRequiredPricingStructures());           
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<String> GetRequiredCurrencies()
        {
            var result = new List<string>
                             {
                                 exchangedCurrency1.paymentAmount.currency.Value,
                                 exchangedCurrency2.paymentAmount.currency.Value
                             };
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeCurrency1PayPartyReference"></param>
        /// <param name="exchangeCurrency2PayPartyReference"></param>
        /// <param name="exchangeCurrency1Amount"></param>
        /// <param name="exchangeCurrency1"></param>
        /// <param name="exchangeCurrency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="spotRate"></param>
        /// <param name="forwardRate"></param>
        /// <param name="forwardPoints"></param>
        /// <returns></returns>
        public static FxSwapLeg CreateForward(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
            decimal exchangeCurrency1Amount, string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis,
            DateTime valueDate, Decimal spotRate, Decimal forwardRate, Decimal? forwardPoints)
        {
            decimal exchange2Amount;
            if (quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
            {
                exchange2Amount = exchangeCurrency1Amount * forwardRate;
            }
            else
            {
                exchange2Amount = exchangeCurrency1Amount / forwardRate;
            }
            var fxForward = new FxSwapLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(exchangeCurrency1PayPartyReference,
                                                             exchangeCurrency2PayPartyReference, exchangeCurrency1,
                                                             exchangeCurrency1Amount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(exchangeCurrency2PayPartyReference,
                                                             exchangeCurrency1PayPartyReference, exchangeCurrency2,
                                                             exchange2Amount),
                                    Items = new[] { valueDate },
                                    exchangeRate =
                                        ExchangeRate.Create(exchangeCurrency1, exchangeCurrency2, quoteBasis, spotRate,
                                                            forwardRate, forwardPoints),
                                    ItemsElementName = new[] { ItemsChoiceType9.valueDate }
                                };
            return fxForward;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exchangeCurrency1PayPartyReference"></param>
        /// <param name="exchangeCurrency2PayPartyReference"></param>
        /// <param name="exchangeCurrency1Amount"></param>
        /// <param name="exchangeCurrency1"></param>
        /// <param name="exchangeCurrency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="spotRate"></param>
        /// <returns></returns>
        public static FxSwapLeg CreateSpot(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference, decimal exchangeCurrency1Amount,
                string exchangeCurrency1, string exchangeCurrency2, QuoteBasisEnum quoteBasis, DateTime valueDate, Decimal spotRate)
        {
            decimal exchange2Amount;
            if (quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
            {
                exchange2Amount = exchangeCurrency1Amount * spotRate;
            }
            else
            {
                exchange2Amount = exchangeCurrency1Amount / spotRate;
            }
            var fxForward = new FxSwapLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(exchangeCurrency1PayPartyReference,
                                                             exchangeCurrency2PayPartyReference, exchangeCurrency1,
                                                             exchangeCurrency1Amount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(exchangeCurrency2PayPartyReference,
                                                             exchangeCurrency1PayPartyReference, exchangeCurrency2,
                                                             exchange2Amount),
                                    Items = new[] {valueDate},
                                    exchangeRate =
                                        ExchangeRate.Create(exchangeCurrency1, exchangeCurrency2, quoteBasis, spotRate),
                                    ItemsElementName = new[] { ItemsChoiceType9.valueDate }
                                };
            return fxForward;
        }
    }
}
