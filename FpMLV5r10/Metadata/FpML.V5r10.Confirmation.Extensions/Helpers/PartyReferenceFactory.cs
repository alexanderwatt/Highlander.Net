using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class PartyReferenceFactory
    {
        public static PartyReference Create(string href)
        {
            PartyReference partyOrTradeSideReference = new PartyReference();
            partyOrTradeSideReference.href = href;

            return partyOrTradeSideReference;
        }
    }
}