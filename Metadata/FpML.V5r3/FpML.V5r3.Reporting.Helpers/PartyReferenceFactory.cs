namespace FpML.V5r3.Reporting.Helpers
{
    public static class PartyReferenceFactory
    {
        public static PartyReference Create(string href)
        {
            var partyOrTradeSideReference = new PartyReference {href = href};

            return partyOrTradeSideReference;
        }
    }
}