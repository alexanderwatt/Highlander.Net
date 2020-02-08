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

namespace Highlander.Reporting.Analytics.V5r3.Distributions
{
    /// <summary>
    /// Summary description for MCG_MKL.
    /// </summary>
    public sealed class MCG31vsl : MCG
    {
        ///<summary>
        ///</summary>
        ///<param name="seed"></param>
        public MCG31vsl(int seed) : base(1132489760, 2147483647, seed)
        {}

        ///<summary>
        ///</summary>
        public MCG31vsl() : this(1)
        {}
    }
}