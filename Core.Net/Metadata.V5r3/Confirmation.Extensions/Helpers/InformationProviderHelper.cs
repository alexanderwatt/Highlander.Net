using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public static class InformationProviderHelper
    {
        public static InformationProvider Create(string provider)
        {
            var informationProvider = new InformationProvider {Value = provider};

            return informationProvider;
        }
    }
}