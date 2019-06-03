#region Using directives

using System;
using FpML.V5r10.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class BondCurveIdentifier : PricingStructureIdentifier
    {
        /// <summary>
        /// The CreditSeniority.
        /// </summary>
        public CreditSeniority CreditSeniority { get; private set; }

        /// <summary>
        /// The CreditInstrumentId.
        /// </summary>
        public InstrumentId CreditInstrumentId { get; private set; }

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

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="pricingStructureType">The pricing strucutre type.</param>
        ///<param name="curveName">The curve name.</param>
        ///<param name="buildDateTime">The build date time.</param>
        ///<param name="algorithm">The algorithm.</param>
        public BondCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime, string algorithm)
            : base(pricingStructureType, curveName, buildDateTime, algorithm)
        {
            SetProperties(PricingStructureType);
        }

        ///<summary>
        /// An id for a ratecurve.
        ///</summary>
        ///<param name="pricingStructureType"></param>
        ///<param name="curveName"></param>
        ///<param name="buildDateTime"></param>
        public BondCurveIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime buildDateTime) 
            : this(pricingStructureType, curveName, buildDateTime, "Default")
        {}

        /// <summary>
        /// An id for a ratecurve.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="baseDate"></param>
        public BondCurveIdentifier(string curveId, DateTime baseDate) 
            : base(curveId)
        {
            BaseDate = baseDate;
            SetProperties(PricingStructureType);
        }

        private void SetProperties()
        {
            switch (PricingStructureType)
            {
                case PricingStructureTypeEnum.BondDiscountCurve:
                case PricingStructureTypeEnum.BondCurve:
                    string creditInstrumentId = PropertyHelper.ExtractCreditInstrumentId(Properties);
                    string creditSeniority = PropertyHelper.ExtractCreditSeniority(Properties);
                    CreditInstrumentId = InstrumentIdHelper.Parse(creditInstrumentId);
                    CreditSeniority = CreditSeniorityHelper.Parse(creditSeniority);
                    break;
            }
        }

        private void SetProperties(PricingStructureTypeEnum pricingStructureType)
        {
            if (pricingStructureType == PricingStructureTypeEnum.BondDiscountCurve)
            {
                var rateCurveId = CurveName.Split('-');
                var subordination = rateCurveId[rateCurveId.Length - 1];
                var indexName = rateCurveId[0];
                for (var i = 1; i < rateCurveId.Length - 1; i++)
                {
                    indexName = indexName + '-' + rateCurveId[i];
                }
                CreditInstrumentId = InstrumentIdHelper.Parse(indexName);
                CreditSeniority = CreditSeniorityHelper.Parse(subordination);
            }
            if (pricingStructureType == PricingStructureTypeEnum.BondCurve)
            {
                var rateCurveId = CurveName.Split('-');
                var subordination = rateCurveId[rateCurveId.Length - 1];
                var indexName = rateCurveId[0];
                for (var i = 1; i < rateCurveId.Length - 1; i++)
                {
                    indexName = indexName + '-' + rateCurveId[i];
                }
                CreditInstrumentId = InstrumentIdHelper.Parse(indexName);
                CreditSeniority = CreditSeniorityHelper.Parse(subordination);
            }
        }
    }
}