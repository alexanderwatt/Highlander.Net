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

#endregion

namespace FpML.V5r11.Reporting
{
    public partial class FxOption
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<string> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            //var putCurrency = PaymentHelper.Create(buyerPartyReference.href, sellerPartyReference.href, putCurrencyAmount.currency.Value, putCurrencyAmount.amount, valueDate);
            //var callCurrency = PaymentHelper.Create(sellerPartyReference.href, buyerPartyReference.href, callCurrencyAmount.currency.Value, callCurrencyAmount.amount, valueDate);
            result.AddRange(putCurrencyAmount.GetRequiredPricingStructures());
            result.AddRange(callCurrencyAmount.GetRequiredPricingStructures());
            result.AddRange(GetRequiredVolatilitySurfaces());
            if (premium != null)
            {
                foreach (var fxOptionPremium in premium)
                {
                    var ps = fxOptionPremium.GetRequiredPricingStructures();
                    result.AddRange(ps);
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override List<String> GetRequiredCurrencies()
        {
            var result = new List<string> {putCurrencyAmount.currency.Value, callCurrencyAmount.currency.Value};
            if (premium != null)
            {
                foreach (var fxOptionPremium in premium)
                {
                    if (!result.Contains(fxOptionPremium.paymentAmount.currency.Value))
                    {
                        result.Add(fxOptionPremium.paymentAmount.currency.Value);
                    }
                }
            }
            return result.Distinct().ToList();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredVolatilitySurfaces()
        {
            var result = new List<String>();
            var quoteBasis = strike.strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
            var fxRate = new FxRate
                             {
                                 quotedCurrencyPair =
                                     new QuotedCurrencyPair
                                         {
                                             currency1 = putCurrencyAmount.currency,
                                             currency2 = callCurrencyAmount.currency,
                                             quoteBasis = quoteBasis
                                         }
                             };
            //THe other values are not necessary for the volatility curve definition.
            result.Add(CurveNameHelpers.GetFxVolatilityMatrixName(fxRate, "FxSpot"));
            return result;
        }

        /// <summary>
        /// Creates a vanilla fx option.
        /// </summary>
        /// <param name="buyerPartyReference"></param>
        /// <param name="sellerPartyReference"></param>
        /// <param name="fxEuropeanExercise"></param>
        /// <param name="soldAs"></param>
        /// <param name="fxCashSettlement"></param>
        /// <param name="putCurrencyAmount"></param>
        /// <param name="callCurrencyAmount"></param>
        /// <param name="fxStrikePrice"></param>
        /// <param name="valueDate"></param>
        /// <param name="premia"></param>
        /// <returns></returns>
        public static FxOption CreateVanillaFXOption(PartyReference buyerPartyReference,
            PartyReference sellerPartyReference, FxEuropeanExercise fxEuropeanExercise,
            PutCallEnum soldAs, FxCashSettlement fxCashSettlement,
            //QuotedAs quotedAs, ExpiryDateTime expiryDate, ExerciseStyleEnum exerciseStyle,
            NonNegativeMoney putCurrencyAmount, NonNegativeMoney callCurrencyAmount, FxStrikePrice fxStrikePrice,
            DateTime valueDate, List<FxOptionPremium> premia)
        {
            var fxOption = new FxOption
                {
                    putCurrencyAmount = putCurrencyAmount,
                    callCurrencyAmount = callCurrencyAmount,
                    strike = fxStrikePrice,
                    buyerPartyReference = buyerPartyReference,
                    sellerPartyReference = sellerPartyReference,
                    premium = premia.ToArray(),
                    soldAs = soldAs,
                    cashSettlement = fxCashSettlement,
                    Item = fxEuropeanExercise
                };
            return fxOption;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hasExpired"></param>
        /// <param name="putCurrencyPayPartyReference"></param>
        /// <param name="callCurrencyPayPartyReference"></param>
        /// <param name="putCurrencyAmount"></param>
        /// <param name="putCurrency"></param>
        /// <param name="callCurrencyAmount"></param>
        /// <param name="callCurrency"></param>
        /// <param name="strikeQuoteBasis"></param>
        /// <param name="valueDate"></param>
        /// <param name="fxRate"></param>
        /// <returns></returns>
        public static FxSingleLeg CreateFxSingleLeg(bool hasExpired, string putCurrencyPayPartyReference, string callCurrencyPayPartyReference, decimal putCurrencyAmount,
                string putCurrency, decimal callCurrencyAmount, string callCurrency, StrikeQuoteBasisEnum strikeQuoteBasis, DateTime valueDate, Decimal fxRate)
        {
            QuoteBasisEnum quoteBasis = strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
            ExchangeRate exchangeRate = hasExpired ? ExchangeRate.Create(putCurrency, callCurrency, quoteBasis, fxRate)
                                            : ExchangeRate.Create(putCurrency, callCurrency, quoteBasis, fxRate, fxRate, null);
            var fxForward = new FxSingleLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(putCurrencyPayPartyReference, callCurrencyPayPartyReference,
                                                             putCurrency, putCurrencyAmount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(callCurrencyPayPartyReference, putCurrencyPayPartyReference,
                                                             callCurrency, callCurrencyAmount),
                                    Items = new[] { valueDate },
                                    ItemsElementName = new[] { ItemsChoiceType31.valueDate },
                                    exchangeRate = exchangeRate,
                                };
            return fxForward;
        }

        /// <summary>
        /// Builds an  fx trade from the current vanilla fx option class.
        /// </summary>
        /// <param name="hasExpired"></param>
        /// <returns></returns>
        public FxSingleLeg CreateFxSingleLeg(bool hasExpired)
        {
            QuoteBasisEnum quoteBasis = strike.strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
            ExchangeRate exchangeRate = hasExpired ? ExchangeRate.Create(putCurrencyAmount.currency.Value, putCurrencyAmount.currency.Value, quoteBasis, strike.rate)
                                            : ExchangeRate.Create(putCurrencyAmount.currency.Value, putCurrencyAmount.currency.Value, quoteBasis, strike.rate, strike.rate, null);
            var fxForward = new FxSingleLeg
            {
                //exchangedCurrency1 =
                //    PaymentHelper.Create(this.putCurrencyPayPartyReference, callCurrencyPayPartyReference,
                //                         putCurrency, putCurrencyAmount),
                //exchangedCurrency2 =
                //    PaymentHelper.Create(callCurrencyPayPartyReference, putCurrencyPayPartyReference,
                //                         callCurrency, callCurrencyAmount),
                exchangeRate = exchangeRate,
                ItemsElementName = new[] { ItemsChoiceType31.valueDate }
            };
            if (Item is FxEuropeanExercise exercise && exercise.expiryDateSpecified)
            {
                fxForward.Items = new[] {exercise.expiryDate};
            }
            return fxForward;
        }
    }
}
