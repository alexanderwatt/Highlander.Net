
using Orion.Analytics.Rates;

namespace Orion.Analytics.ForeignExchange
{
    ///<summary>
    ///</summary>
    public class FxForwardAnalytics : BasicRateAnalytics
    {
        ///<summary>
        ///</summary>
        ///<param name="spotRate"></param>
        ///<param name="baseStartDiscountfactor"></param>
        ///<param name="baseEndDiscountFactor"></param>
        ///<param name="crossStartDiscountFactor"></param>
        ///<param name="crossEndDiscountFactor"></param>
        ///<returns></returns>
        public static decimal EvaluateForwardRate(decimal spotRate, 
                                                  decimal baseStartDiscountfactor, decimal baseEndDiscountFactor,
                                                  decimal crossStartDiscountFactor, decimal crossEndDiscountFactor)
        {
            return (spotRate*baseStartDiscountfactor*crossEndDiscountFactor)/
                   (crossStartDiscountFactor*baseEndDiscountFactor);
        }
    }
}