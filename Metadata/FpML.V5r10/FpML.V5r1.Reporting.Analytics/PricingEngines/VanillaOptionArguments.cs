
#region Using directives

using System.ComponentModel;			// some useful attributes
using System.Data;
//using Orion.Analytics.Options;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Vanilla option arguments.
    /// </summary>
    [ Description("Vanilla option arguments.") ]
    public class VanillaOptionArguments : DataColumns
    {
        ///<summary>
        ///</summary>
        ///<param name="r"></param>
        public VanillaOptionArguments(DataRow r)
        {
            _optionType = 0; // make warning disappear
            parseColumns(r);
        }
        ///<summary>
        ///</summary>
        static public DataColumn[] Columns
        {
            get { return reflectColumns(typeof(VanillaOptionArguments)); }
        }
        ///<summary>
        ///</summary>
        ///<param name="r"></param>
        static public void Validate(DataRow r) 
        {
            validateColumns(typeof(VanillaOptionArguments), r);
        }

        [ AutoColumn("optionType", typeof(int), 
            Min=(double)Option.Type.Call, 
            Max=(double)Option.Type.Straddle) ]
        private readonly int _optionType;
        // we need a getter to cast the type correctly.
        ///<summary>
        ///</summary>
        public Option.Type optionType 
        {
            get { return (Option.Type)_optionType; }
        }

        [ AutoColumn("underlying", typeof(double), 
            Min=0.0+double.Epsilon) ]
        public readonly double underlying;

        [ AutoColumn("strike", typeof(double), Min=0.0) ]
        public readonly double strike;

        [ AutoColumn("dividendYield", typeof(double), AllowDBNull=true) ]
        public readonly double dividendYield;

        [ AutoColumn("riskFreeRate", typeof(double)) ]
        public readonly double riskFreeRate;

        [ AutoColumn("residualTime", typeof(double), Min=0.0) ]
        public readonly double residualTime;

        [ AutoColumn("volatility", typeof(double), Min=0.0) ]
        public readonly double volatility;
    }
}