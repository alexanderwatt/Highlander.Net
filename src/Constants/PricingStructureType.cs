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

namespace Highlander.Constants
{
    /// <summary>
    /// The valid pricing structure types.
    /// </summary>
    public enum PricingStructureTypeEnum
    {
        /// <summary>
        /// RateCurve
        /// </summary>
        RateCurve,

        /// <summary>
        /// RateSpreadCurve
        /// </summary>
        RateSpreadCurve,

        /// <summary>
        /// RateBasisCurve
        /// </summary>
        RateBasisCurve,

        /// <summary>
        /// RateXccyCurve
        /// </summary>
        RateXccyCurve,

        /// <summary>
        /// DiscountCurve
        /// </summary>
        DiscountCurve,

        /// <summary>
        /// DiscountCurve
        /// </summary>
        DiscountSpreadCurve,

        /// <summary>
        /// XccySpreadCurve
        /// </summary>
        XccySpreadCurve,

        /// <summary>
        /// InflationCurve
        /// </summary>
        InflationCurve,

        /// <summary>
        /// FxCurve
        /// </summary>
        FxCurve,

        /// <summary>
        /// FxDerivedCurve
        /// </summary>
        FxDerivedCurve,

        /// <summary>
        /// FxVolatilityMatrix
        /// </summary>
        FxVolatilityMatrix,

        /// <summary>
        /// SurvivalProbabilityCurve
        /// </summary>
        SurvivalProbabilityCurve,

        /// <summary>
        /// PropertyCurve
        /// </summary>
        PropertyCurve,

        /// <summary>
        /// CommodityCurve
        /// </summary>
        CommodityCurve,

        /// <summary>
        /// CommoditySpreadCurve
        /// </summary>
        CommoditySpreadCurve,

        /// <summary>
        /// CommodityVolatilityMatrix
        /// </summary>
        CommodityVolatilityMatrix,

        /// <summary>
        /// RateVolatilityMatrix
        /// </summary>
        RateVolatilityMatrix,

        /// <summary>
        /// RateATMVolatilityMatrix
        /// </summary>
        RateATMVolatilityMatrix,

        /// <summary>
        /// RateVolatilityCube
        /// </summary>
        RateVolatilityCube,

        /// <summary>
        /// LPMCapFloorCurve
        /// </summary>
        LPMCapFloorCurve,

        /// <summary>
        /// LPMSwaptionCurve
        /// </summary>
        LPMSwaptionCurve,

        /// <summary>
        /// VolatilitySurface
        /// </summary>
        VolatilitySurface,

        /// <summary>
        /// VolatilityCube
        /// </summary>
        VolatilityCube,

        /// <summary>
        /// VolatilitySurface2
        /// </summary>
        VolatilitySurface2,

        /// <summary>
        /// EquityVolatilityMatrix
        /// </summary>
        EquityVolatilityMatrix,

        /// <summary>
        /// EquityWingVolatilityMatrix
        /// </summary>
        EquityWingVolatilityMatrix,

        /// <summary>
        /// RateCurve
        /// </summary>
        BondCurve,

        /// <summary>
        /// EquityCurve
        /// </summary>
        EquityCurve,

        /// <summary>
        /// EquitySpreadCurve
        /// </summary>
        EquitySpreadCurve,

        /// <summary>
        /// DiscountCurve
        /// </summary>
        BondDiscountCurve,

        /// <summary>
        /// BondFinancingCurve
        /// </summary>
        BondFinancingCurve,

        /// <summary>
        /// BondFinancingSpreadCurve
        /// </summary>
        BondFinancingBasisCurve,

        /// <summary>
        /// ClearedRateCurve
        /// </summary>
        ClearedRateCurve,

        /// <summary>
        /// CapVolatilityCurve
        /// </summary>
        CapVolatilityCurve,

        /// <summary>
        /// Vol Surface
        /// </summary>
        CapVolatilitySurface,

        /// <summary>
        /// Exchange Traded Curve
        /// </summary>
        ExchangeTradedCurve,

        /// <summary>
        /// GenericVolatilityCurve
        /// </summary>
        GenericVolatilityCurve,

        /// <summary>
        /// A SABR surface: ExpiryTermStrikeVolatilitySurface
        /// </summary>
        SABRSurface
    }
}
