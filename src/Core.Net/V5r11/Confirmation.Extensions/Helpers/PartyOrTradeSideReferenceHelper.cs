using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
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