#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r10.Reporting;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using Math = System.Math;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableFxOptionPremium : PriceableCashflow, IPriceableInstrumentController<FxOptionPremium>
    {
        /// <summary>
        /// PayerPartyReference
        /// </summary>
        public PartyReference PayerPartyReference { get; set; }
        
        /// <summary>
        /// ReceiverPartyReference
        /// </summary>
        public PartyReference ReceiverPartyReference { get; set;}
        
        /// <summary>
        /// 
        /// </summary>
        public IdentifiedDate SettlementDate => IdentifiedDateHelper.Create("adjustedDate", PaymentDate);

        /// <summary>
        /// The premium quote
        /// </summary>
        public PremiumQuote PremiumQuote { get; set; }

        /// <summary>
        /// The settlement information
        /// </summary>
        public SettlementInformation SettlementInformation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxOptionPremium"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="receiverPartyReference">The receiver.</param>
        /// <param name="payerIsBase">The flag determing if the payer is the base party.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="settlementDate">The adjusted payment date.</param>
        /// <param name="payerPartyReference">The payer.</param>
        /// <param name="premiumQuote">The premium quote </param>
        /// <param name="settlementInformation">The settlement information. </param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceableFxOptionPremium
            (
            string id
            , string payerPartyReference
            , string receiverPartyReference
            , bool payerIsBase
            , Money amount
            , DateTime settlementDate
            , PremiumQuote premiumQuote
            , SettlementInformation settlementInformation
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBase, amount,
            AdjustableOrAdjustedDateHelper.CreateAdjustedDate(settlementDate),
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.Premium.ToString()), false, paymentCalendar)
        {
            PayerPartyReference = PartyReferenceFactory.Create(payerPartyReference);
            ReceiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference);
            OrderedPartyNames.Add(PayerPartyReference.href);
            OrderedPartyNames.Add(ReceiverPartyReference.href);
            if (premiumQuote != null)
            {
                PremiumQuote = new PremiumQuote
                                   {
                                       quoteBasis = premiumQuote.quoteBasis,
                                       quoteBasisSpecified = true,
                                       value = premiumQuote.value,
                                       valueSpecified = true
                                   };
            }
            if (settlementInformation == null) return;
            SettlementInformation = new SettlementInformation();
            var item = BinarySerializerHelper.Clone(settlementInformation.Item);
            SettlementInformation.Item = item;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableFxOptionPremium"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="receiverPartyReference">The receiver.</param>
        /// <param name="payerIsBase">The flag determing if the payer is the base party.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="settlementDate">The adjusted payment date.</param>
        /// <param name="payerPartyReference">The payer.</param>
        /// <param name="premiumQuote">The premium quote </param>
        /// <param name="settlementInformation">The settlement information. </param>
        /// <param name="paymentCalendar">Type paymentCalendar.</param>
        public PriceableFxOptionPremium
            (
            string id
            , string payerPartyReference
            , string receiverPartyReference
            , bool payerIsBase
            , Decimal amount
            , string currency
            , DateTime settlementDate
            , PremiumQuote premiumQuote
            , SettlementInformation settlementInformation
            , IBusinessCalendar paymentCalendar) :
            base(id, "DiscountedCashflow", payerIsBase, MoneyHelper.GetAmount(amount, currency),
            AdjustableOrAdjustedDateHelper.CreateAdjustedDate(settlementDate),
            PaymentTypeHelper.Create("Certain"), CashflowTypeHelper.Create(CashflowTypeEnum.Premium.ToString()), false, paymentCalendar)
        {
            PayerPartyReference = PartyReferenceFactory.Create(payerPartyReference);
            ReceiverPartyReference = PartyReferenceFactory.Create(receiverPartyReference);
            OrderedPartyNames.Add(PayerPartyReference.href);
            OrderedPartyNames.Add(ReceiverPartyReference.href);
            if (premiumQuote != null)
            {
                PremiumQuote = new PremiumQuote
                                   {
                                       quoteBasis = premiumQuote.quoteBasis,
                                       value = premiumQuote.value
                                   };
            }
            if (settlementInformation == null) return;
            SettlementInformation = new SettlementInformation();
            var item = BinarySerializerHelper.Clone(settlementInformation.Item);
            SettlementInformation.Item = item;
        }

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public FxOptionPremium Build()
        {
            var px = FxOptionPremium.Create(PayerPartyReference.href, ReceiverPartyReference.href, PaymentAmount.currency.Value, Math.Abs(PaymentAmount.amount), PaymentDate);
            px.quote = PremiumQuote;
            px.settlementInformation = SettlementInformation;
            return px;
        }

        #endregion

        #region Overrides of InstrumentControllerBase

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return null;
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return null;
        }

        #endregion
    }
}
