#region Using directives



#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public static class PartyOrAccountReferenceFactory
    {
        public static PartyOrAccountReference Create(string href)
        {
            var partyOrTradeSideReference = new PartyOrAccountReference {href = href};
            return partyOrTradeSideReference;
        }
    }
}