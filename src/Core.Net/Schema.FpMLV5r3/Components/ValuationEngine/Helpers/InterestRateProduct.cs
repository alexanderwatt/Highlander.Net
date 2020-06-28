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

#region Using directives

using System;
using System.Collections.Generic;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using XsdClassesFieldResolver = Highlander.Reporting.V5r3.XsdClassesFieldResolver;

#endregion

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    public class InterestRateProduct
    {
        internal static Market CreateFpMLMarketFromCurves(List<IRateCurve> uniqueCurves)
        {
            var marketFactory = new MarketFactory();
            foreach (var rateCurve in uniqueCurves)
            {
                // Add all unique curves () into market
                //
                marketFactory.AddPricingStructure(rateCurve.GetFpMLData());
            }
            return marketFactory.Create();
        }

        internal static void ReplacePartiesInValuationReport(ValuationReport valuationReport, List<PartyIdRangeItem> partyIdList)
        {
            if (null == partyIdList) return;
            var listOfParties = new List<Party>();
            foreach (PartyIdRangeItem partyIdRangeItem in partyIdList)
            {
                Party party = PartyFactory.Create(partyIdRangeItem.IdOrRole);
                var partyId = new PartyId {Value = partyIdRangeItem.PartyId};
                party.partyId = new[] { partyId };
                listOfParties.Add(party);
            }
            valuationReport.party = listOfParties.ToArray();
        }

        public static AssetValuation CreateAssetValuationFromValuationSet(List<StringObjectRangeItem> valuationSet)
        {
            var assetValuation = new AssetValuation();
            var listOfQuotations = new List<Quotation>();
            foreach (StringObjectRangeItem item in valuationSet)
            {
                var quotation = new Quotation
                                    {
                                        measureType = AssetMeasureTypeHelper.Parse(item.StringValue)
                                    };
                if (item.ObjectValue is double | item.ObjectValue is decimal)
                {
                    quotation.value = Convert.ToDecimal(item.ObjectValue);
                    quotation.valueSpecified = true;
                }
                else
                {
                    quotation.cashflowType = new CashflowType {Value = item.ObjectValue.ToString()};
                }
                listOfQuotations.Add(quotation);
            }
            assetValuation.quote = listOfQuotations.ToArray();
            return assetValuation;
        }

        internal static void AddOtherPartyPayments(ValuationReport valuationReport, List<OtherPartyPaymentRangeItem> otherPartyPaymentList)
        {
            var otherPartyPayments = new List<Payment>();
            //  other party payments
            //  
            if (null != otherPartyPaymentList)
            {
                foreach (OtherPartyPaymentRangeItem item in otherPartyPaymentList)
                {
                    var otherPartyPayment = new Payment
                                                {
                                                    payerPartyReference =
                                                        PartyReferenceFactory.Create(item.Payer),
                                                    receiverPartyReference =
                                                        PartyReferenceFactory.Create(item.Receiver),
                                                    paymentAmount = MoneyHelper.GetNonNegativeAmount(item.Amount),
                                                    paymentDate = AdjustableOrAdjustedDateHelper.CreateAdjustedDate(item.PaymentDate),
                                                    paymentType = PaymentTypeHelper.Create(item.PaymentType)
                                                };
                    otherPartyPayments.Add(otherPartyPayment);
                }
            }
            TradeValuationItem valuationItem = valuationReport.tradeValuationItem[0];
            Trade[] tradeArray = XsdClassesFieldResolver.TradeValuationItemGetTradeArray(valuationItem);
            Trade trade = tradeArray[0];
            trade.otherPartyPayment = otherPartyPayments.ToArray();
        }
    }
}