namespace FpML.V5r3.Reporting
{
    public static class PartyOrTradeSideReferenceHelper
    {
        public static PartyOrTradeSideReference ToPartyOrTradeSideReference(string href)
        {
            PartyOrTradeSideReference partyOrTradeSideReference = new PartyOrTradeSideReference();
            partyOrTradeSideReference.href = href;

            return partyOrTradeSideReference;
        }
    }
}