namespace FpML.V5r10.Reporting.Models.Rates.Futures
{
    public enum RateFutureAssetAnalyticModelIdentifier
    {
        //Old approach where exch future is its own class
        ZB,
        IR,
        IB,
        ED,
        ER,
        RA,
        L,
        ES,
        EY,
        HR,
        BAX,
        W,
        B,
        CER,
        LME
    }

    public enum ExchangeIdentifierEnum
    {
        //New approach using an exchange class and not a futures class.
        XCME,
        IEPA,
        XLME,
        XASX,
        XCBT
    }
}
