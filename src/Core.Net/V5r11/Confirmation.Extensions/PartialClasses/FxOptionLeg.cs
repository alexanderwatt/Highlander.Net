#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace nab.QDS.FpML.V47
{
    public partial class FxOptionLeg
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public override List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();
            var putCurrency = PaymentHelper.Create(buyerPartyReference.href, sellerPartyReference.href, putCurrencyAmount.currency.Value, putCurrencyAmount.amount, valueDate);
            var callCurrency = PaymentHelper.Create(sellerPartyReference.href, buyerPartyReference.href, callCurrencyAmount.currency.Value, callCurrencyAmount.amount, valueDate);
            result.AddRange(putCurrency.GetRequiredPricingStructures());
            result.AddRange(callCurrency.GetRequiredPricingStructures());
            result.AddRange(GetRequiredVolatilitySurfaces());
            if (fxOptionPremium != null)
            {
                foreach (var premium in fxOptionPremium)
                {
                    var ps = premium.GetRequiredPricingStructures();
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
            if (fxOptionPremium != null)
            {
                foreach (var premium in fxOptionPremium)
                {
                    if (!result.Contains(premium.premiumAmount.currency.Value))
                    {
                        result.Add(premium.premiumAmount.currency.Value);
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
            var quoteBasis = fxStrikePrice.strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
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
            result.Add(Helpers.GetFxVolatilityMatrixName(fxRate, "FxSpot"));
            return result;
        }

        /// <summary>
        /// Creates a vanilla fx option.
        /// </summary>
        /// <param name="buyerPartyReference"></param>
        /// <param name="sellerPartyReference"></param>
        /// <param name="quotedAs"></param>
        /// <param name="expiryDate"></param>
        /// <param name="exerciseStyle"> </param>
        /// <param name="putCurrencyAmount"></param>
        /// <param name="callCurrencyAmount"></param>
        /// <param name="fxStrikePrice"></param>
        /// <param name="valueDate"></param>
        /// <param name="premia"></param>
        /// <returns></returns>
        public static FxOptionLeg CreateVanillaOption(PartyOrTradeSideReference buyerPartyReference, 
            PartyOrTradeSideReference sellerPartyReference, //FxOptionType optionType,
            QuotedAs quotedAs, ExpiryDateTime expiryDate, ExerciseStyleEnum exerciseStyle,
            Money putCurrencyAmount, Money callCurrencyAmount, FxStrikePrice fxStrikePrice,
            DateTime valueDate, List<FxOptionPremium> premia)
        {
            var fxOption = new FxOptionLeg
            {
                putCurrencyAmount = putCurrencyAmount,
                callCurrencyAmount = callCurrencyAmount,
                fxStrikePrice = fxStrikePrice,
                quotedAs = quotedAs,
                valueDate = valueDate,
                buyerPartyReference = buyerPartyReference,
                sellerPartyReferenceField = sellerPartyReference,
                fxOptionPremium = premia.ToArray(),
                expiryDateTime = expiryDate,
                exerciseStyle = exerciseStyle
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
        public static FxLeg CreateFxLeg(bool hasExpired, string putCurrencyPayPartyReference, string callCurrencyPayPartyReference, decimal putCurrencyAmount,
                string putCurrency, decimal callCurrencyAmount, string callCurrency, StrikeQuoteBasisEnum strikeQuoteBasis, DateTime valueDate, Decimal fxRate)
        {
            QuoteBasisEnum quoteBasis = strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
            ExchangeRate exchangeRate = hasExpired ? ExchangeRate.Create(putCurrency, callCurrency, quoteBasis, fxRate)
                                            : ExchangeRate.Create(putCurrency, callCurrency, quoteBasis, fxRate, fxRate, null);
            var fxforward = new FxLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(putCurrencyPayPartyReference, callCurrencyPayPartyReference,
                                                             putCurrency, putCurrencyAmount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(callCurrencyPayPartyReference, putCurrencyPayPartyReference,
                                                             callCurrency, callCurrencyAmount),
                                    Items = new[] {valueDate},
                                    exchangeRate = exchangeRate,
                                    ItemsElementName = new[] {ItemsChoiceType15.valueDate}
                                };
            return fxforward;
        }

        /// <summary>
        /// Builds and fx trade from the current class.
        /// </summary>
        /// <param name="hasExpired"></param>
        /// <returns></returns>
        public FxLeg CreateFxLeg(bool hasExpired)
        {
            QuoteBasisEnum quoteBasis = fxStrikePrice.strikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency ? QuoteBasisEnum.Currency2PerCurrency1 : QuoteBasisEnum.Currency1PerCurrency2;
            ExchangeRate exchangeRate = hasExpired ? ExchangeRate.Create(putCurrencyAmount.currency.Value, putCurrencyAmount.currency.Value, quoteBasis, fxStrikePrice.rate)
                                            : ExchangeRate.Create(putCurrencyAmount.currency.Value, putCurrencyAmount.currency.Value, quoteBasis, fxStrikePrice.rate, fxStrikePrice.rate, null);
            var fxforward = new FxLeg
            {
                //exchangedCurrency1 =
                //    PaymentHelper.Create(this.putCurrencyPayPartyReference, callCurrencyPayPartyReference,
                //                         putCurrency, putCurrencyAmount),
                //exchangedCurrency2 =
                //    PaymentHelper.Create(callCurrencyPayPartyReference, putCurrencyPayPartyReference,
                //                         callCurrency, callCurrencyAmount),
                Items = new[] { valueDate },
                exchangeRate = exchangeRate,
                ItemsElementName = new[] { ItemsChoiceType15.valueDate }
            };
            return fxforward;
        }
    }
}
