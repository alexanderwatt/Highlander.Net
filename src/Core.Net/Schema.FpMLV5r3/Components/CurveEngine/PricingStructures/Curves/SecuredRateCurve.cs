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
using Highlander.Core.Common;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Constants;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class SecuredRateCurve : RateCurve, ISecuredRateCurve
    {
        private Asset UnderlyingAsset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuredRateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="underlyingAsset">THe underlying asset used for security.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public SecuredRateCurve(ILogger logger, ICoreCache cache, string nameSpace, Asset underlyingAsset, NamedValueSet properties, 
            QuotedAssetSet instrumentData, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, properties, instrumentData, fixingCalendar, rollCalendar)
        {
            UnderlyingAsset = underlyingAsset;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuredRateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public SecuredRateCurve(ILogger logger, ICoreCache cache, string nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, 
            NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar)
        {
            var refId = properties.GetValue<String>(BondProp.ReferenceBond, true);
            var refItem = cache.LoadItem<Bond>(nameSpace + '.' + "ReferenceData.FixedIncome." + refId);
            var refAsset = refItem.Data as Bond;
            UnderlyingAsset = refAsset; 
        }

        /// <summary>
        /// Returns the asst that the cash is secured against.
        /// </summary>
        /// <returns></returns>
        public Asset GetSecurity()
        {
            return UnderlyingAsset;
        }
    }
}