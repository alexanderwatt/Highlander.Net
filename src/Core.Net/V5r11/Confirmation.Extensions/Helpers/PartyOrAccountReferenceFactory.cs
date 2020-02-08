#region Using directives

using nab.QDS.FpML.V47;

#endregion

namespace nab.QDS.FpML.V47
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