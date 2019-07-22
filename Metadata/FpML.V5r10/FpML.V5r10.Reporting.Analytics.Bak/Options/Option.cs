#region Using directives

using System;
using Math=System.Math;

#endregion

namespace Orion.Analytics.Options
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class Option
    {
        ///<summary>
        ///</summary>
        public enum Type
        {
            ///<summary>
            ///</summary>
            Call,
 
            ///<summary>
            ///</summary>
            Put,
 
            ///<summary>
            ///</summary>
            Straddle
        };

        ///<summary>
        ///</summary>
        ///<param name="optionType"></param>
        ///<param name="price"></param>
        ///<param name="strike"></param>
        ///<returns></returns>
        ///<exception cref="ArgumentOutOfRangeException"></exception>
        public static double ExercisePayoff(Type optionType, double price, double strike) 
        {
            switch(optionType)
            {
                case Type.Call:
                    {
                        return Math.Max(price - strike, 0.0);
                    }
                case Type.Put:
                    {
                        return Math.Max(strike-price, 0.0);
                    }
                case Type.Straddle:
                    {
                        return Math.Abs(strike - price);
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(optionType), optionType, "TODO: Unknown option type");
                    }
            }
        }
    }
}