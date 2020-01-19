namespace FpML.V5r3.Confirmation
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