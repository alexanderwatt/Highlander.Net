/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class SwapFactory
    {
        /// <summary>
        /// </summary>
        /// <param name="stream1"></param>
        /// <param name="stream2"></param>
        /// <param name="productTypeTaxonomy"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream stream1, InterestRateStream stream2, string productTypeTaxonomy)
        {
            var result = new Swap
                {
                    swapStream = new[] {stream1, stream2},
                    productType = new[] {ProductTypeHelper.Create(productTypeTaxonomy) },
                };
            return result;
        }

        /// <summary>
        /// Creates floater (swap with single floating stream of coupons)
        /// </summary>
        /// <param name="singleStream"></param>
        /// <param name="productTypeTaxonomy"></param>
        /// <returns></returns>
        public static Swap Create(InterestRateStream singleStream, string productTypeTaxonomy)
        {
            var result = new Swap
                {
                    swapStream = new[] {singleStream},
                    productType = new[] {ProductTypeHelper.Create(productTypeTaxonomy) },
                };
            return result;
        }
    }
}