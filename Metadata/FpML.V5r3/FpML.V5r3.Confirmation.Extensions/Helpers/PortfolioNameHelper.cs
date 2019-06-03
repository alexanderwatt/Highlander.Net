namespace nab.QDS.FpML.V47
{
    public class PortfolioNameHelper
    {
        public static PortfolioName Parse(string portfolioName)
        {
            var portfolio = new PortfolioName {Value = portfolioName};
            return portfolio;
        }
    }
}
