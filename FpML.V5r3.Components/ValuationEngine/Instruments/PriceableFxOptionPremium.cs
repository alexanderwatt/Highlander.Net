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
using System.Collections.Generic;
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.ModelFramework;
using Orion.ModelFramework.Instruments;
using Math = System.Math;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableFxOptionPremium : PriceableCashflow, IPriceableInstrumentController<FxOptionPremium>
    {
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
        /// <param name="payerIsBase">The flag determining if the payer is the base party.</param>
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
        /// <param name="payerIsBase">The flag determining if the payer is the base party.</param>
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
            , decimal amount
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
