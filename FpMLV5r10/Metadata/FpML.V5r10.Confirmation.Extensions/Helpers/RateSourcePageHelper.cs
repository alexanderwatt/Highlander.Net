using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class RateSourcePageHelper
    {
        public static RateSourcePage Create(string sourcePage)
        {
            RateSourcePage rateSourcePage = new RateSourcePage();
            rateSourcePage.Value = sourcePage;

            return rateSourcePage;
        }
    }
}