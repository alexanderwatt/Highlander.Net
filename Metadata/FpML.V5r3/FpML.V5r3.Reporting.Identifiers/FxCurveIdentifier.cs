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
using FpML.V5r3.Reporting;

#endregion

namespace Orion.Identifiers
{
    /// <summary>
    /// The FxCurveIdentifier.
    /// </summary>
    public sealed class FxCurveIdentifier : PricingStructureIdentifier
    {
        ///<summary>
        /// The reference curve type and name e.g FxCurve.AUD-USD
        ///</summary>
        public String ReferenceFxCurveTypeAndName { get; set; }

        ///<summary>
        /// The reference curve unique id e.g Orion.Market.QR_LIVE.FxCurve.AUD-USD
        ///</summary>
        public String ReferenceFxCurveUniqueId { get; set; }

        ///<summary>
        /// The reference curve type and name e.g FxCurve.GBP-USD
        ///</summary>
        public String ReferenceFxCurve2TypeAndName { get; set; }

        ///<summary>
        /// The reference curve unique id e.g Orion.Market.QR_LIVE.FxCurve.GBP-USD
        ///</summary>
        public String ReferenceFxCurve2UniqueId { get; set; }

        /// <summary>
        /// The QuotedCurrencyPair.
        /// </summary>
        public QuotedCurrencyPair QuotedCurrencyPair { get; private set; }

        public new Currency Currency => QuotedCurrencyPair.currency1;

        public Currency QuoteCurrency => QuotedCurrencyPair.currency2;

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public FxCurveIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        /// <summary>
        /// The FxCurveIdentifier.
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="curveName"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="buildDateTime"></param>
        public FxCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, QuoteBasisEnum quoteBasis, DateTime buildDateTime)
            : base(pricingStructureType, curveName, buildDateTime)
        {
            SetProperties(quoteBasis);
        }

        /// <summary>
        /// The FxCurveIdentifier.
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="curveName"></param>
        /// <param name="buildDateTime"></param>
        public FxCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime)
            : base(pricingStructureType, curveName, buildDateTime)
        {
            SetProperties(QuoteBasisEnum.Currency2PerCurrency1);
        }

        /// <summary>
        /// The FxCurveIdentifier.
        /// </summary>
        /// <param name="curveId"></param>
        public FxCurveIdentifier(string curveId)
            : base(curveId)
        {
            CurveName = curveId;
            Properties.Set(CurveProp.CurveName, CurveName);
            SetProperties(QuoteBasisEnum.Currency2PerCurrency1);
        }

        private void SetProperties(QuoteBasisEnum quoteBasis)
        {
            QuotedCurrencyPair = PropertyHelper.ExtractQuotedCurrencyPair(Properties);
            QuotedCurrencyPair.quoteBasis = quoteBasis;
            CurveName = $"{Currency.Value}-{QuoteCurrency.Value}";
            UniqueIdentifier = BuildUniqueId();
            Id = BuildId();
        }

        private void SetProperties()
        {
            //Check if the quotebasis was provided.
            var quoteBasis = PropertyHelper.ExtractQuoteBasis(Properties);
            SetProperties(quoteBasis);
            switch (PricingStructureType)
            {
                case PricingStructureTypeEnum.FxDerivedCurve:
                    ReferenceFxCurveTypeAndName = PropertyHelper.ExtractReferenceFxCurveName(Properties);
                    ReferenceFxCurveUniqueId = PropertyHelper.ExtractReferenceFxCurveUniqueId(Properties);
                    ReferenceFxCurve2TypeAndName = PropertyHelper.ExtractReferenceFxCurve2Name(Properties);
                    ReferenceFxCurve2UniqueId = PropertyHelper.ExtractReferenceFxCurve2UniqueId(Properties);
                    break;
            }
        }

    }
}