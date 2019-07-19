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

using System;
using FpML.V5r10.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The CommodityCurveIdentifier.
    /// </summary>
    public class EquityCurveIdentifier : PricingStructureIdentifier
    {
        /// <summary>
        /// The CommodityAsset.
        /// </summary>
        public string EquityAsset { get; private set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public EquityCurveIdentifier(NamedValueSet properties)
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
        public EquityCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : base(pricingStructureType, curveName, buildDateTime)
        {
            var components = CurveName.Split('-');
            EquityAsset = components[1];
            Currency = CurrencyHelper.Parse(components[0]);
        }

        /// <summary>
        /// The CommodityCurveIdentifier.
        /// </summary>
        /// <param name="curveId"></param>
        public EquityCurveIdentifier(string curveId)
            : base(curveId)
        {
            var comcurveId = CurveName.Split('-');
            if (comcurveId.Length == 2)
            {
                EquityAsset = comcurveId[1];
                Currency = CurrencyHelper.Parse(comcurveId[0]);
            }
        }

        private void SetProperties()
        {
            EquityAsset = PropertyHelper.ExtractEquityAsset(Properties);
        }
    }
}