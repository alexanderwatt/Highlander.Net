/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;

#endregion

namespace Orion.CurveEngine.PricingStructures.Helpers
{
    ///<summary>
    ///</summary>
    public class VolatilitySurfaceHelper2
    {
        ///<summary>
        ///</summary>
        ///<param name="expiryByStrikeSurface"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        // {null, 0.25, 0.50, 0.75, 1.00},//strike row
        // {"1y", 0.11, 0.12, 0.13, 0.14},//
        // {"2y", 0.21, 0.22, 0.23, 0.24},//
        // {"3y", 0.31, 0.32, 0.33, 0.34},//
        // {"4y", 0.41, 0.42, 0.43, 0.44},//
        // {"5y", 0.51, 0.52, 0.53, 0.54},//
        // {"6y", 0.61, 0.62, 0.63, 0.64},//expiry - 6
        // {"7y", 0.71, 0.72, 0.73, 0.74} //expiry - 7
        public static List<PricingStructurePoint> ExtractDataPoints(object[,] expiryByStrikeSurface)
        {
            var result = new List<PricingStructurePoint>();

            // extract 
            //
            for (int expiryIndex = expiryByStrikeSurface.GetLowerBound(0) + 1; 
                 expiryIndex <= expiryByStrikeSurface.GetUpperBound(0); 
                 ++expiryIndex)
            {
                object expiryAsObject = expiryByStrikeSurface[expiryIndex, 0];

                for (int strikeIndex = expiryByStrikeSurface.GetLowerBound(1) + 1;
                     strikeIndex <= expiryByStrikeSurface.GetUpperBound(1);
                     ++strikeIndex)
                {
                    object strikeAsObject = expiryByStrikeSurface[0, strikeIndex];
                    object volatilityAsObject = expiryByStrikeSurface[expiryIndex, strikeIndex];

                    var pricingStructurePoint = new PricingStructurePoint
                                                    {
                                                        coordinate = new[]
                                                                         {
                                                                             new PricingDataPointCoordinate()
                                                                         },

                                                        valueSpecified = true,
                                                        value = Convert.ToDecimal(volatilityAsObject)
                                                    };

                    // value
                    //

                    // expiry
                    //
                    pricingStructurePoint.coordinate[0].expiration = new[]
                                                                         {
                                                                             new TimeDimension()
                                                                         };

                    pricingStructurePoint.coordinate[0].expiration[0].Items = new object[]{1};
                    pricingStructurePoint.coordinate[0].expiration[0].Items[0] = PeriodHelper.Parse(expiryAsObject.ToString());

                    // strike
                    //
                    pricingStructurePoint.coordinate[0].strike = new[]
                                                                     {
                                                                         Convert.ToDecimal(strikeAsObject)
                                                                     };



                    result.Add(pricingStructurePoint);
                }
            }


            return result;
        }
    }
}