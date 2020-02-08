#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r3.Confirmation
{
    public partial class FxSwapLeg
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public List<String> GetRequiredPricingStructures() 
        {
            var result = new List<String>();

            result.AddRange(exchangedCurrency1.GetRequiredPricingStructures());
            result.AddRange(exchangedCurrency2.GetRequiredPricingStructures());           
            return result;
        }

        public List<String> GetRequiredCurrencies()
        {
            var result = new List<string>
                             {
                                 exchangedCurrency1.paymentAmount.currency.Value,
                                 exchangedCurrency2.paymentAmount.currency.Value
                             };
            return result;
        }

        public static FxSingleLeg CreateForward(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference,
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
            var fxforward = new FxSingleLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(exchangeCurrency1PayPartyReference,
                                                             exchangeCurrency2PayPartyReference, exchangeCurrency1,
                                                             exchangeCurrency1Amount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(exchangeCurrency2PayPartyReference,
                                                             exchangeCurrency1PayPartyReference, exchangeCurrency2,
                                                             exchange2Amount),
                                    Items1 = new[] { valueDate },
                                    exchangeRate =
                                        ExchangeRate.Create(exchangeCurrency1, exchangeCurrency2, quoteBasis, spotRate,
                                                            forwardRate, forwardPoints),
                                    Items1ElementName = new[] { Items1ChoiceType.valueDate }
                                };
            return fxforward;
        }

        public static FxSingleLeg CreateSpot(string exchangeCurrency1PayPartyReference, string exchangeCurrency2PayPartyReference, decimal exchangeCurrency1Amount,
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
            var fxforward = new FxSingleLeg
                                {
                                    exchangedCurrency1 =
                                        PaymentHelper.Create(exchangeCurrency1PayPartyReference,
                                                             exchangeCurrency2PayPartyReference, exchangeCurrency1,
                                                             exchangeCurrency1Amount),
                                    exchangedCurrency2 =
                                        PaymentHelper.Create(exchangeCurrency2PayPartyReference,
                                                             exchangeCurrency1PayPartyReference, exchangeCurrency2,
                                                             exchange2Amount),
                                    Items1 = new[] {valueDate},
                                    exchangeRate =
                                        ExchangeRate.Create(exchangeCurrency1, exchangeCurrency2, quoteBasis, spotRate),
                                    Items1ElementName = new[] { Items1ChoiceType.valueDate }
                                };
            return fxforward;
        }

    }
}
