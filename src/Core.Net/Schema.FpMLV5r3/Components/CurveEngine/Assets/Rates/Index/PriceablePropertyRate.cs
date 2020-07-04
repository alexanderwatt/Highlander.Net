﻿/*
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
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Index
{
    ///<summary>
    ///</summary>
    public abstract class PriceablePropertyRate : PriceableSimpleRateAsset , IPriceablePropertyRateAssetController
    {
        /// <summary>
        /// 
        /// </summary>
        public RelativeDateOffset ResetDateConvention { get; set; }

        /// <summary>
        /// Gets the index of the underlying rate.
        /// </summary>
        /// <value>The index of the underlying rate.</value>
        public RateIndex UnderlyingRateIndex { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateIndex"/> class.
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="amount">The amount.</param>
        /// <param name="rateIndex">Index of the rate.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="resetDateConvention">The reset date convention.</param>
        /// <param name="fixedRate">The fixed rate.</param>
        protected PriceablePropertyRate(DateTime baseDate, Decimal amount, RateIndex rateIndex,
                                     BusinessDayAdjustments businessDayAdjustments, RelativeDateOffset resetDateConvention,
                                     BasicQuotation fixedRate)
            : base(rateIndex.id, baseDate, amount, businessDayAdjustments, fixedRate)
        {
            UnderlyingRateIndex = rateIndex;
            if (PeriodEnum.D != resetDateConvention.period)
            {
                throw new System.Exception("Only day units are supported!");
            }
            ResetDateConvention = resetDateConvention;
            Id = rateIndex.id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableRateIndex"/> class.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="effectiveDate">The effective date.</param>
        /// <param name="riskMaturityDate">The risk maturity date.</param>
        protected PriceablePropertyRate(string id, Decimal amount, DateTime effectiveDate, DateTime riskMaturityDate)
            : base(id, amount, effectiveDate, riskMaturityDate)
        { } 
    }
}