#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
{
    public static class PartyOrTradeSideReferenceFactory
    {
        public static PartyOrTradeSideReference Create(string href)
        {
            PartyOrTradeSideReference partyOrTradeSideReference = new PartyOrTradeSideReference();
            partyOrTradeSideReference.href = href;

            return partyOrTradeSideReference;
        }
    }
}