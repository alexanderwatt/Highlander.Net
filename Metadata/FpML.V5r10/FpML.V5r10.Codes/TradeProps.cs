namespace FpML.V5r10.Codes
{
    public enum TradeState
    {
        Undefined,
        Pricing,
        Confirmed,
        Verified,
        Matured,
        Test
    }

    public class TradeSourceType
    {
        public const string Murex = "Murex";
        public const string Imagine = "Imagine";
        public const string WallStreet = "WallStreet";
        public const string Orion = "Orion";
        public const string SpreadSheet = "SpreadSheet";
        public const string Calypso = "Calypso";
        public const string Merchant = "Merchant";
        public const string Caspar = "Caspar";
        public const string BoundaryRider = "BoundaryRider";
        public const string Credient = "Credient";
        public const string FSS = "FSS";
        public const string Sentry = "Sentry";
    }

    public class TradeProp
    {
        public const string DataGroup = "DataGroup";
        public const string TradeType = "TradeType";
        public const string IsParty1Base = "IsParty1Base";
        public const string IsXccySwap = "isXccySwap";
        public const string LongTradeId = "LongTradeId";
        public const string ProductType = "ProductType";
        public const string ProductTaxonomy = "ProductTaxonomy";
        public const string TradeState = "TradeState";
        public const string TradeId = "TradeId";
        public const string TradeDate = "TradeDate";
        public const string TradeSource = "TradeSource";
        public const string AsAtDate = "AsAtDate";
        public const string TradingBookId = "TradingBookId";
        public const string BaseParty = "BaseParty";
        public const string CounterPartyId = "CounterPartyId";
        public const string ReportingParty = "ReportingParty";
        public const string OriginatingPartyId = "OriginatingPartyId";
        public const string TradingBookName = "TradingBookName";//These are redundant
        public const string CounterPartyName = "CounterPartyName";//These are redundant
        public const string OriginatingPartyName = "OriginatingPartyName";
        public const string PartyId = "PartyId";
        public const string PartyName = "PartyName";
        public const string RequiredPricingStructures = "RequiredPricingStructures";
        public const string RequiredCurrencies = "RequiredCurrencies";
        public const string Party1 = "Party1";
        public const string Party2 = "Party2";
        public const string EffectiveDate = "EffectiveDate";
        public const string PaymentDate = "PaymentDate";
        public const string MaturityDate = "MaturityDate";
        public const string UniqueIdentifier = "UniqueIdentifier";
        public const string Schema = "Schema";
        public const string Platform = "Platform";
        public const string ValuationDate = "ValuationDate";
    }
}
