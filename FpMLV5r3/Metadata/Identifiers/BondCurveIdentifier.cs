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

using Highlander.Constants;
using Highlander.Utilities.NamedValues;

#endregion

namespace Highlander.Reporting.Identifiers.V5r3
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class BondCurveIdentifier : PricingStructureIdentifier
    {
        /// <summary>
        /// The CreditSeniority.
        /// </summary>
        public string CreditSeniority { get; private set; }

        /// <summary>
        /// The CreditInstrumentId.
        /// </summary>
        public string CreditInstrumentId { get; private set; }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public BondCurveIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        /////<summary>
        ///// An id for a ratecurve.
        /////</summary>
        /////<param name="pricingStructureType">The pricing structure type.</param>
        /////<param name="curveName">The curve name.</param>
        /////<param name="buildDateTime">The build date time.</param>
        /////<param name="algorithm">The algorithm.</param>
        //public BondCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime, string algorithm)
        //    : base(pricingStructureType, curveName, buildDateTime, algorithm)
        //{
        //    SetProperties(PricingStructureType);
        //}

        /////<summary>
        ///// An id for a ratecurve.
        /////</summary>
        /////<param name="pricingStructureType"></param>
        /////<param name="curveName"></param>
        /////<param name="buildDateTime"></param>
        //public BondCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
        //    : this(pricingStructureType, curveName, buildDateTime, "Default")
        //{}

        ///// <summary>
        ///// An id for a ratecurve.
        ///// </summary>
        ///// <param name="curveId"></param>
        ///// <param name="baseDate"></param>
        //public BondCurveIdentifier(string curveId, DateTime baseDate) 
        //    : base(curveId)
        //{
        //    BaseDate = baseDate;
        //    SetProperties(PricingStructureType);
        //}

        private void SetProperties()
        {
            switch (PricingStructureType)
            {
                case PricingStructureTypeEnum.BondDiscountCurve:
                case PricingStructureTypeEnum.BondCurve:
                    string creditInstrumentId = PropertyHelper.ExtractCreditInstrumentId(Properties);
                    string creditSeniority = PropertyHelper.ExtractCreditSeniority(Properties);
                    CreditInstrumentId = creditInstrumentId;
                    CreditSeniority = creditSeniority;
                    break;
            }
        }

        //private void SetProperties(PricingStructureTypeEnum pricingStructureType)
        //{
        //    if (pricingStructureType == PricingStructureTypeEnum.BondDiscountCurve)
        //    {
        //        var rateCurveId = CurveName.Split('-');
        //        var subordination = rateCurveId[rateCurveId.Length - 1];
        //        var indexName = rateCurveId[0];
        //        for (var i = 1; i < rateCurveId.Length - 1; i++)
        //        {
        //            indexName = indexName + '-' + rateCurveId[i];
        //        }
        //        CreditInstrumentId = InstrumentIdHelper.Parse(indexName);
        //        CreditSeniority = CreditSeniorityHelper.Parse(subordination);
        //    }
        //    if (pricingStructureType == PricingStructureTypeEnum.BondCurve)
        //    {
        //        var rateCurveId = CurveName.Split('-');
        //        var subordination = rateCurveId[rateCurveId.Length - 1];
        //        var indexName = rateCurveId[0];
        //        for (var i = 1; i < rateCurveId.Length - 1; i++)
        //        {
        //            indexName = indexName + '-' + rateCurveId[i];
        //        }
        //        CreditInstrumentId = InstrumentIdHelper.Parse(indexName);
        //        CreditSeniority = CreditSeniorityHelper.Parse(subordination);
        //    }
        //}
    }
}