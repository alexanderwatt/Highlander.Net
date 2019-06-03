
using System.Data;

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Option pricing results.
    /// </summary>
    public class OptionGreeks : DataColumns
    {
        ///<summary>
        ///</summary>
        ///<param name="r"></param>
        public OptionGreeks(DataRow r)
        {
            parseColumns(r);
        }
        ///<summary>
        ///</summary>
        static public DataColumn[] Columns
        {
            get { return reflectColumns(typeof(OptionGreeks)); }
        }

        [ AutoColumn("delta", typeof(double), AllowDBNull=true) ]
        public readonly double delta;

        [ AutoColumn("gamma", typeof(double), AllowDBNull=true) ]
        public readonly double gamma;

        [ AutoColumn("theta", typeof(double), AllowDBNull=true) ]
        public readonly double theta;

        [ AutoColumn("vega", typeof(double), AllowDBNull=true) ]
        public readonly double vega;

        [ AutoColumn("rho", typeof(double), AllowDBNull=true) ]
        public readonly double rho;

        [ AutoColumn("dividendRho", typeof(double), AllowDBNull=true) ]
        public readonly double dividendRho;
    }
}