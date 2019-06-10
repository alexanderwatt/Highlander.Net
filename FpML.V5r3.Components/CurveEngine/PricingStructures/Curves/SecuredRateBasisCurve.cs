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
using Core.Common;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class SecuredRateBasisCurve : RateBasisCurve, ISecuredRateCurve
    {
        private Asset UnderlyingAsset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="underlyingAsset">THe underlying asset used for security.</param>
        /// <param name="referenceCurve">The reference parent curve id.</param>
        /// <param name="spreadAssets">The spreads by asset.</param>
        /// <param name="properties">The properties of the new spread curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public SecuredRateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace, Asset underlyingAsset, 
            IRateCurve referenceCurve, QuotedAssetSet spreadAssets, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, referenceCurve, spreadAssets, properties, fixingCalendar, rollCalendar)
        {
            UnderlyingAsset = underlyingAsset; 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateBasisCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="spreadFpmlData">The FPML data.</param>
        /// <param name="properties">The properties</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public SecuredRateBasisCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> spreadFpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, spreadFpmlData, properties, fixingCalendar, rollCalendar)
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