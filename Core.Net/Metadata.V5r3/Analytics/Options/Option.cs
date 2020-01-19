/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using Math=System.Math;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Options
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