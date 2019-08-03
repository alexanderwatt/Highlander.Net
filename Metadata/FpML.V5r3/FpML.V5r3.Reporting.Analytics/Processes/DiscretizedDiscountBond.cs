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

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Useful discretized discount bond asset.
    /// </summary>
    public class DiscretizedDiscountBond : DiscretizedAsset
    {
        ///<summary>
        ///</summary>
        ///<param name="method"></param>
        public DiscretizedDiscountBond(INumericalMethod method)
            : base(method) 
        {}

        public override void Reset(int size) 
        {
            Values = new double[size];
            for (int i = 0; i < Values.Length; i++)
                Values[i] = 1.0;
        }
    }
}