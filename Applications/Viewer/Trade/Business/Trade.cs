using System;
using Core.Common;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using nab.QDS.FpML.Codes;

namespace Orion.WebViewer.Trade.Business
{
    public class Trade
    {
        public DateTime Date { get; private set; }
        public string FullTradeName { get; private set; }
        public string TradeType { get; private set; }
        public string ProductType { get; private set; }
        public string Source { get; private set; }
        public string TradeId { get; private set; }
        public string OriginatingPartyId { get; private set; }
        public string OriginatingPartyName { get; private set; }
        public string CounterpartyId { get; private set; }
        public string CounterpartyName { get; private set; }
        public string TradingBookId { get; private set; }
        public string TradingBookName { get; private set; }
        public string PayStreamType { get; private set; }
        public DateTime? PayMaturityDate { get; private set; }
        public decimal PayPV { get; private set; }
        public string ReceiveStreamType { get; private set; }
        public DateTime? ReceiveMaturityDate { get; private set; }
        public decimal ReceivePV { get; private set; }
        public decimal NetPV { get; private set; }
        public DateTime AsAtDate { get; private set; }
        public int TradeSvcRev { get; private set; }
        public nab.QDS.FpML.V47.Trade TradeStructure { get; private set; }

        public Trade(ICoreItem item)
        {
            NamedValueSet properties = item.AppProps;
            FullTradeName = item.Name;
            Date = properties.GetValue<DateTime>(TradeProp.TradeDate);
            TradeType = properties.GetValue<string>(TradeProp.TradeType, true);
            ProductType = properties.GetString(TradeProp.ProductType, "Undefined");
            Source = properties.GetString(TradeProp.TradeSource, "Orion");
            TradeId = properties.GetValue<string>(TradeProp.TradeId, true);
            OriginatingPartyId = properties.GetValue<string>(TradeProp.OriginatingPartyId, false);
            OriginatingPartyName = properties.GetString("OriginatingPartyName", "Party1");
            CounterpartyId = properties.GetValue<string>("CounterPartyId", false);
            CounterpartyName = properties.GetString("CounterPartyName", "Party2");
            TradingBookId = properties.GetValue<string>(TradeProp.TradingBookId, false);
            TradingBookName = properties.GetString("TradingBookName", "Test");
            PayStreamType = properties.GetString("PayStreamType", "");
            PayMaturityDate = properties.GetValue<DateTime?>("PayMaturityDate", null);
            PayPV = properties.GetValue<decimal>("PayPV", 0);
            ReceiveStreamType = properties.GetString("RecStreamType", "");
            ReceiveMaturityDate = properties.GetValue<DateTime?>("PayMaturityDate", null);
            ReceivePV = properties.GetValue<decimal>("RecPV", 0);
            NetPV = properties.GetValue<decimal>("NetPV", 0);
            AsAtDate = properties.GetValue<DateTime>(TradeProp.AsAtDate);
            TradeSvcRev = properties.GetValue<int>("TradeSvcRev");
            TradeStructure = (nab.QDS.FpML.V47.Trade)item.Data;
        }

        public string Fpml
        {
            get 
            { 
                return XmlSerializerHelper.SerializeToString(TradeStructure); 
            }
        }
    }
}
