#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using Orion.Analytics.Helpers;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Helpers
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