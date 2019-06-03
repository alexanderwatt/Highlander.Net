namespace FpML.V5r3.Confirmation
{
    public static class RateSourcePageHelper
    {
        public static RateSourcePage Create(string sourcePage)
        {
            var rateSourcePage = new RateSourcePage {Value = sourcePage};
            return rateSourcePage;
        }
    }
}