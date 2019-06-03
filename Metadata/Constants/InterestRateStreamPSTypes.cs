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

namespace Orion.Constants
{
    ///<summary>
    /// The valid types of prcing structure.
    ///</summary>
    public enum InterestRateStreamPSTypes
    {
        ///<summary>
        /// A discount curve.
        ///</summary>
        DiscountCurve,
        
        ///<summary>
        /// A forecast curve.
        ///</summary>
        ForecastCurve,
        
        ///<summary>
        /// A reporting currency fx curve.
        ///</summary>
        ReportingCurrencyFxCurve,

        ///<summary>
        /// A discount curve.
        ///</summary>
        DiscountCurve2,

        ///<summary>
        /// A reporting currency fx curve.
        ///</summary>
        ReportingCurrencyFxCurve2,

        ///<summary>
        /// The vol surface or cube used of the forecast index
        ///</summary>
        ForecastIndexVolatilitySurface,

        ///<summary>
        /// A coomodity index curve.
        ///</summary>
        CommodityIndexCurve
    }

}
