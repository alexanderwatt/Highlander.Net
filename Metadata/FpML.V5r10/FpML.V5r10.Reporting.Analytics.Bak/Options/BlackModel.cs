#region Using directives

using System;
using Orion.Analytics.Distributions;

#endregion

namespace Orion.Analytics.Options
{
    ///<summary>
    ///</summary>
    public static class BlackModel
    {
        ///<summary>
        ///</summary>
        ///<param name="rate"></param>
        ///<param name="strikeRate"></param>
        ///<param name="volatility"></param>
        ///<param name="timeToExpiry"></param>
        ///<returns></returns>
        public static double GetSwaptionValue(double rate, double strikeRate, double volatility, double timeToExpiry)
        {
            double d1 = (Math.Log(rate / strikeRate) + 0.5 * Math.Pow(volatility, 2) * timeToExpiry) / volatility * timeToExpiry;
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiry);
            // Compute the cumulative Normal distribution evaluated at
            // d1 and d2.
            double phid1 = new NormalDistribution().CumulativeDistribution(d1);
            double phid2 = new NormalDistribution().CumulativeDistribution(d2);          
            // Compute and return the price of a Black vanilla payer swaption.
            double price = (rate * phid1 - strikeRate * phid2);
            return price;
        }
        
        ///<summary>
        ///</summary>
        ///<param name="floatRate"></param>
        ///<param name="strikeRate"></param>
        ///<param name="volatility"></param>
        ///<param name="timeToExpiry"></param>
        ///<returns></returns>
        public static double GetCapletValue(double floatRate, double strikeRate, double volatility, double timeToExpiry)
        {
            double d1 = (Math.Log(floatRate / strikeRate) + 0.5 * Math.Pow(volatility, 2) * timeToExpiry) / volatility * timeToExpiry;
            double d2 = d1 - volatility * Math.Sqrt(timeToExpiry);
            // Compute the cumulative Normal distribution evaluated at
            // d1 and d2.
            double phid1 = new NormalDistribution().CumulativeDistribution(d1);
            double phid2 = new NormalDistribution().CumulativeDistribution(d2);          
            // Compute and return the price of a Black vanilla payer swaption.
            double price = (floatRate * phid1 - strikeRate * phid2);
            return price;
        }
    }
}