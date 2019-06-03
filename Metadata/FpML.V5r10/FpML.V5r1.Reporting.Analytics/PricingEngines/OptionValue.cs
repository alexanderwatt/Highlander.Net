
// COM interop attributes
// some useful attributes
using System.Data;

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Option pricing results.
    /// </summary>
    public class OptionValue : DataColumns
    {
        ///<summary>
        ///</summary>
        ///<param name="r"></param>
        public OptionValue(DataRow r)
        {
            parseColumns(r);
        }

        ///<summary>
        ///</summary>
        static public DataColumn[] Columns
        {
            get { return reflectColumns(typeof(OptionValue)); }
        }

        [ AutoColumn("value", typeof(double), AllowDBNull=true) ]
        public readonly double value;
    }
}