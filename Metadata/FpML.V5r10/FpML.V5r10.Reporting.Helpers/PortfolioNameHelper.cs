namespace FpML.V5r10.Reporting.Helpers
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
