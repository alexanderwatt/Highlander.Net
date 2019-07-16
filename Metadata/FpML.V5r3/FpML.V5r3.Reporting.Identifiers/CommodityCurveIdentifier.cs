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
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The CommodityCurveIdentifier.
    /// </summary>
    public class CommodityCurveIdentifier : PricingStructureIdentifier
    {

        /// <summary>
        /// The CommodityAsset.
        /// </summary>
        public string CommodityAsset { get; private set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public CommodityCurveIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        /// <summary>
        /// The CommodityCurveIdentifier.
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="curveName"></param>
        /// <param name="buildDateTime"></param>
        public CommodityCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : base(pricingStructureType, curveName, buildDateTime)
        {
            CommodityAsset = CurveName.Split('-')[1];
        }

        /// <summary>
        /// The CommodityCurveIdentifier.
        /// </summary>
        /// <param name="curveId"></param>
        public CommodityCurveIdentifier(string curveId)
            : base(curveId)
        {
            var comcurveId = CurveName.Split('-');
            if (comcurveId.Length != 2)
            {
            }
            else
            {
                CommodityAsset = CurveName.Split('-')[1];
            }
        }

        private void SetProperties()
        {
            CommodityAsset = PropertyHelper.ExtractCommodityAsset(Properties);
        }
    }
}