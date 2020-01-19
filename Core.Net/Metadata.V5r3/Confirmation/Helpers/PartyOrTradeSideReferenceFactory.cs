#region Using directives



#endregion

namespace FpML.V5r3.Reporting
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