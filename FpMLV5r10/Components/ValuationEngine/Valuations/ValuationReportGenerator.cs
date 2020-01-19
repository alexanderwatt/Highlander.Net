#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.Helpers;
using Orion.Util.Serialisation;
using FpML.V5r10.Reporting;
using XsdClassesFieldResolver = FpML.V5r10.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.ValuationEngine.Valuations
{
    public class ValuationReportGenerator
    {
        public static ValuationReport Merge(ValuationReport valuation1, 
                                            ValuationReport valuation2)
        {
            ValuationReport result = XmlSerializerHelper.Clone(valuation1);
            ValuationReport copyOfValuation2 = XmlSerializerHelper.Clone(valuation2);            
            //  generate new id from a valuation report
            //
            string valuationIdForCombinedValuationReport = Guid.NewGuid().ToString();
            result.header.messageId.Value = valuationIdForCombinedValuationReport;
            //  copy tradeValuationItems from valuation2 to result.
            //
            var listOfTradeValuations = new List<TradeValuationItem>();
            listOfTradeValuations.AddRange(result.tradeValuationItem);
            listOfTradeValuations.AddRange(copyOfValuation2.tradeValuationItem);
            result.tradeValuationItem = listOfTradeValuations.ToArray();
            return result;
        }
     
        public static ValuationReport Generate(string valuationId, string baseParty, 
                                               string tradeId, DateTime tradeDate,
                                               Swap swap, Market market, 
                                               AssetValuation assetValuation)
        {
            var valuationReport = new ValuationReport
                                      {
                                          header = new NotificationMessageHeader
                                                       {
                                                           messageId = new MessageId
                                                                           {
                                                                               Value
                                                                                   =
                                                                                   valuationId
                                                                           }
                                                       },
                                          market = market
                                      };
            //  Associate id with the valuation
            //
            var tradeValuationItem = new TradeValuationItem();
            valuationReport.tradeValuationItem = new[] { tradeValuationItem };

            //Party nabParty = PartyFactory.Create("Party1");
            //Party counterParty = PartyFactory.Create(_counterpartyName);
//
//            valuationReport.party = new Party[] { nabParty, counterParty };

//            PartyOrAccountReference nabPartyReference = PartyOrAccountReferenceFactory.Create(nabParty.id);
//            PartyOrAccountReference counterPartyReference = PartyOrAccountReferenceFactory.Create(counterParty.id);

//            // NAB is the payer of pay paystream and receiver of receive stream
//            //
//            SwapHelper.GetPayerStream(swap).payerPartyReference = nabPartyReference;
//            SwapHelper.GetReceiverStream(swap).receiverPartyReference = nabPartyReference;
//
//            // CounterParty is the receiver of paystream and payer of receivestream
//            //
//            SwapHelper.GetPayStream(swap).receiverPartyReference = counterPartyReference;
//            SwapHelper.GetReceiveStream(swap).payerPartyReference = counterPartyReference;

            var trade = new Trade();          
            //  Generate trade header
            //
            TradeHeader tradeHeader = CreateTradeHeader(tradeDate, tradeId);
            trade.tradeHeader = tradeHeader;          
            XsdClassesFieldResolver.TradeSetSwap(trade, swap);
            tradeValuationItem.Items = new object[] { trade };
            tradeValuationItem.valuationSet = new ValuationSet
                                                  {
                                                      baseParty = PartyReferenceFactory.Create(baseParty),
                                                      assetValuation = new[] {assetValuation}
                                                  };
            return valuationReport;
        }

        private static TradeHeader CreateTradeHeader(DateTime tradeDate, string tradeId)
        {
            var tradeHeader = new TradeHeader
                                  {
                                      tradeDate = IdentifiedDateHelper.Create(TradeProp.TradeDate, tradeDate),
                                      partyTradeIdentifier = new[] {new PartyTradeIdentifier()}
                                  };
            var tradeIdAsObject = new TradeId {tradeIdScheme = "FpML", Value = tradeId};
            XsdClassesFieldResolver.TradeIdentifierSetTradeId(tradeHeader.partyTradeIdentifier[0], tradeIdAsObject);
            return tradeHeader;
        }

        public static ValuationReport Generate(string valuationId, string baseParty,
                                               string tradeId, DateTime tradeDate,
                                               CapFloor capFloor, Market market, AssetValuation assetValuation)
        {
            var valuationReport = new ValuationReport
                                      {
                                          header = new NotificationMessageHeader
                                                       {
                                                           messageId = new MessageId
                                                                           {
                                                                               Value
                                                                                   =
                                                                                   valuationId
                                                                           }
                                                       },
                                          market = market
                                      };
            //  Associate id with the valuation
            //
            var tradeValuationItem = new TradeValuationItem();
            valuationReport.tradeValuationItem = new[] { tradeValuationItem };
            var trade = new Trade();
            TradeHeader tradeHeader = CreateTradeHeader(tradeDate, tradeId);
            trade.tradeHeader = tradeHeader;
            XsdClassesFieldResolver.TradeSetCapFloor(trade, capFloor);
            tradeValuationItem.Items = new object[] { trade };
            tradeValuationItem.valuationSet = new ValuationSet
                                                  {
                                                      baseParty = PartyReferenceFactory.Create(baseParty),
                                                      assetValuation = new[] {assetValuation}
                                                  };
            return valuationReport;
        }

        public static ValuationReport Generate(string valuationId, string baseParty,
                                               string tradeId, DateTime tradeDate,
                                               Swaption swaption, Market market, AssetValuation assetValuation)
        {
            var valuationReport = new ValuationReport
                                      {
                                          header = new NotificationMessageHeader
                                                       {
                                                           messageId = new MessageId
                                                                           {
                                                                               Value
                                                                                   =
                                                                                   valuationId
                                                                           }
                                                       },
                                          market = market
                                      };
            //  Associate id with the valuation
            //
            var tradeValuationItem = new TradeValuationItem();
            valuationReport.tradeValuationItem = new[] { tradeValuationItem };
            //  Generate trade header
            //
            var trade = new Trade();
            TradeHeader tradeHeader = CreateTradeHeader(tradeDate, tradeId);
            trade.tradeHeader = tradeHeader;
            XsdClassesFieldResolver.TradeSetSwaption(trade, swaption);
            tradeValuationItem.Items = new object[] { trade };
            tradeValuationItem.valuationSet = new ValuationSet
                                                  {
                                                      baseParty = PartyReferenceFactory.Create(baseParty),
                                                      assetValuation = new[] {assetValuation}
                                                  };
            return valuationReport;
        }


        public static ValuationReport Generate(string valuationId, string baseParty, Fra fra, Market market, AssetValuation assetValuation)
        {
            var valuationReport = new ValuationReport
                                      {
                                          header = new NotificationMessageHeader
                                                       {
                                                           messageId = new MessageId
                                                                           {
                                                                               Value
                                                                                   =
                                                                                   valuationId
                                                                           }
                                                       },
                                          market = market
                                      };
            //  Associate id with the valuation
            //
            var tradeValuationItem = new TradeValuationItem();
            valuationReport.tradeValuationItem = new[] { tradeValuationItem };
            var trade = new Trade();
            XsdClassesFieldResolver.TradeSetFra(trade, fra);
            tradeValuationItem.Items = new object[] { trade };
            tradeValuationItem.valuationSet = new ValuationSet
                                                  {
                                                      baseParty = PartyReferenceFactory.Create(baseParty),
                                                      assetValuation = new[] {assetValuation}
                                                  };
            return valuationReport;
        }


        public static ValuationReport Generate(string valuationId, string party1Name, string party2Name,
                                               bool isParty1Base, Trade trade, Market market,
                                               AssetValuation assetValuation)
        {
            var valuationReport = new ValuationReport
            {
                header = new NotificationMessageHeader
                {
                    messageId = new MessageId
                    {
                        Value = valuationId
                    }
                },
                market = market
            };
            //  Associate id with the valuation
            //
            var tradeValuationItem = new TradeValuationItem();
            valuationReport.tradeValuationItem = new[] { tradeValuationItem };
            string baseParty = isParty1Base ? party1Name : party2Name;
            Party party1 = PartyFactory.Create("Party1", party1Name);
            Party party2 = PartyFactory.Create("Party2", party2Name);
            valuationReport.party = new[] { party1, party2 };
            tradeValuationItem.Items = new object[] { trade };
            tradeValuationItem.valuationSet = new ValuationSet
            {
                baseParty = PartyReferenceFactory.Create(baseParty),
                assetValuation = new[] { assetValuation }
            };
            return valuationReport;
        }
    }
}