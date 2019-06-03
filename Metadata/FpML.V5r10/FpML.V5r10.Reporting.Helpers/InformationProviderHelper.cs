namespace FpML.V5r10.Reporting.Helpers
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