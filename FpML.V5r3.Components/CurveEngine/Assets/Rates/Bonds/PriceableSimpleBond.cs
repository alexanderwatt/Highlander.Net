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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Schedulers;
using Orion.CalendarEngine.Schedulers;
using Orion.Constants;
using Orion.ModelFramework;
using Orion.Util.Helpers;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.CurveEngine.Helpers;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableSimpleBond : PriceableBondAsset
    {      
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="bond">The bond</param>
        /// <param name="settlementDate">The settlement date.</param>
        /// <param name="exDivDate">The ex dividend date.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="quoteType">The quote type</param>
        public PriceableSimpleBond(DateTime baseDate, Bond bond, DateTime settlementDate, DateTime exDivDate,
        BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar paymentCalendar, BasicQuotation marketQuote, BondPriceEnum quoteType)
            : base(baseDate, bond.faceAmount, bond.currency, null, null, businessDayAdjustments, marketQuote, quoteType)
        {            
            Id = bond.id;
            var tempId = Id.Split('-');
            var bondId = tempId[0];
            if (tempId.Length > 2)
            {
                bondId = tempId[2];
            }                
            Issuer = (string)bond.Item;//Does not handle PartyReference type -> only string!
            Description = "Not Defined";
            if (bond.description != null)
            {
                Description = bond.description;
            }
            MaturityDate = bond.maturity;
            CouponDayCount = new DayCountFraction { Value = bond.dayCountFraction.Value };
            if (bond.parValueSpecified)
            {
                ParValue = bond.parValue;
            }
            if (bond.couponRateSpecified)
            {
                CouponRate = bond.couponRate;
            }
            CouponFrequency = new Period
            {
                period = bond.paymentFrequency.period,
                periodMultiplier = bond.paymentFrequency.periodMultiplier
            };
            CouponType = CouponTypeEnum.Fixed;
            if(bond.clearanceSystem != null)
            {
                ClearanceSystem = bond.clearanceSystem.Value;
            }
            if (bond.exchangeId != null)
            {
                Exchange = bond.exchangeId.Value;
            }
            if(bond.seniority != null)
            {
                Seniority = EnumHelper.Parse<CreditSeniorityEnum>(bond.seniority.Value);
            }
            if(bond.instrumentId != null)
            {
                InstrumentIds = new List<InstrumentId>();
                foreach (var identifier in bond.instrumentId.Select(id => InstrumentIdHelper.Parse(id.Value)))
                {
                    InstrumentIds.Add(identifier);
                }
            }
            //This handles the case of a bond forward used in curve building.
            if (MaturityDate > BaseDate)
            {
                var rollConvention =
                    RollConventionEnumHelper.Parse(MaturityDate.Day.ToString(CultureInfo.InvariantCulture));
                Frequency = FrequencyHelper.ToFrequency(bond.paymentFrequency);
                SettlementDate = settlementDate;
                UnAdjustedPeriodDates = DateScheduler.GetUnadjustedCouponDatesFromMaturityDate(SettlementDate,
                                                                                               MaturityDate,
                                                                                               CouponFrequency,
                                                                                               rollConvention,
                                                                                               out _,
                                                                                               out var nextCouponDate);
                LastCouponDate = UnAdjustedPeriodDates[0];
                NextCouponDate = nextCouponDate;
                AdjustedPeriodDates =
                    AdjustedDateScheduler.GetAdjustedDateSchedule(UnAdjustedPeriodDates,
                                                                  PaymentBusinessDayAdjustments.businessDayConvention,
                                                                  paymentCalendar).ToArray();
                AdjustedPeriodDates[0] = SettlementDate;
                NextExDivDate = exDivDate;
                IsXD = IsExDiv();
            }
            BondCurveName = CurveNameHelpers.GetBondCurveName(Currency.Value, bondId);
            SwapDiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="nodeStruct">The bond nodeStruct</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="quoteType">THe quote Type</param>
        public PriceableSimpleBond(DateTime baseDate, BondNodeStruct nodeStruct, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar, 
            BasicQuotation marketQuote, BondPriceEnum quoteType)
            : base(baseDate, nodeStruct.Bond.faceAmount, nodeStruct.Bond.currency, nodeStruct.SettlementDate, nodeStruct.ExDivDate, nodeStruct.BusinessDayAdjustments, marketQuote, quoteType)
        {
            Id = nodeStruct.Bond.id;
            var tempId = Id.Split('-');
            var bondId = tempId[0];
            if (tempId.Length > 2)
            {
                bondId = tempId[2];
            } 
            SettlementDateCalendar = settlementCalendar;
            Issuer = (string)nodeStruct.Bond.Item;//Does not handle PartyReference type -> only string!
            Description = "Not Defined";
            //IsYTMQuote = true;
            if (nodeStruct.Bond.description != null)
            {
                Description = nodeStruct.Bond.description;
            }
            MaturityDate = nodeStruct.Bond.maturity;
            CouponDayCount = new DayCountFraction {Value = nodeStruct.Bond.dayCountFraction.Value};
            CouponFrequency = new Period
                                  {
                                      period = nodeStruct.Bond.paymentFrequency.period,
                                      periodMultiplier = nodeStruct.Bond.paymentFrequency.periodMultiplier
                                  };
            if (nodeStruct.Bond.couponRateSpecified)
            {
                CouponRate = nodeStruct.Bond.couponRate;
            }
            if (nodeStruct.Bond.parValueSpecified)
            {
                ParValue = nodeStruct.Bond.parValue;
            }
            if (nodeStruct.Bond.clearanceSystem != null)
            {
                ClearanceSystem = nodeStruct.Bond.clearanceSystem.Value;
            }
            if (nodeStruct.Bond.exchangeId != null)
            {
                Exchange = nodeStruct.Bond.exchangeId.Value;
            }
            CouponType = CouponTypeEnum.Fixed;
            if (nodeStruct.Bond.seniority != null)
            {
                Seniority = EnumHelper.Parse<CreditSeniorityEnum>(nodeStruct.Bond.seniority.Value, true);
            }
            if (nodeStruct.Bond.instrumentId != null)
            {
                InstrumentIds = new List<InstrumentId>();
                foreach (var identifier in nodeStruct.Bond.instrumentId.Select(id => InstrumentIdHelper.Parse(id.Value)))
                {
                    InstrumentIds.Add(identifier);
                }
            }
            //This handles the case of a bond forward used in curve building.
            if (MaturityDate > BaseDate)
            {
                var rollConvention =
                    RollConventionEnumHelper.Parse(MaturityDate.Day.ToString(CultureInfo.InvariantCulture));
                Frequency = FrequencyHelper.ToFrequency(nodeStruct.Bond.paymentFrequency);
                //Get the settlement date
                SettlementDate = GetSettlementDate(baseDate, settlementCalendar, nodeStruct.SettlementDate);
                //Generate the necessary dates.
                //TODO Should the settlement date and the underlying bond be calculated on the fly when calculation occurs?
                UnAdjustedPeriodDates = DateScheduler.GetUnadjustedCouponDatesFromMaturityDate(SettlementDate,
                                                                                               MaturityDate,
                                                                                               CouponFrequency,
                                                                                               rollConvention,
                                                                                               out _,
                                                                                               out var nextCouponDate);
                LastCouponDate = UnAdjustedPeriodDates[0];
                NextCouponDate = nextCouponDate;
                AdjustedPeriodDates =
                    AdjustedDateScheduler.GetAdjustedDateSchedule(UnAdjustedPeriodDates,
                                                                  nodeStruct.BusinessDayAdjustments
                                                                            .businessDayConvention, paymentCalendar)
                                         .ToArray();
                AdjustedPeriodDates[0] = SettlementDate; //TODO check this!
                NextExDivDate = GetNextExDivDate();
                IsXD = IsExDiv();
            }
            BondCurveName = CurveNameHelpers.GetBondCurveName(Currency.Value, bondId);
            SwapDiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true); 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableSimpleBond"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="bond">The bond</param>
        /// <param name="settlementDateOffset">The settlement date offset.</param>
        /// <param name="exDivDateOffset">The ex dividend offset.</param>
        /// <param name="businessDayAdjustments">The business day adjustments.</param>
        /// <param name="settlementCalendar">The settlement Calendar.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="marketQuote">The market quote.</param>
        /// <param name="quoteType">The quote type can be: dirty price, clean price of YTM.</param>
        public PriceableSimpleBond(DateTime baseDate, Bond bond, RelativeDateOffset settlementDateOffset, RelativeDateOffset exDivDateOffset,
        BusinessDayAdjustments businessDayAdjustments, IBusinessCalendar settlementCalendar, IBusinessCalendar paymentCalendar,
        BasicQuotation marketQuote, BondPriceEnum quoteType)
            : base(baseDate, bond.faceAmount, bond.currency, settlementDateOffset, exDivDateOffset, businessDayAdjustments, marketQuote, quoteType)
        {
            Id = bond.id;
            var tempId = Id.Split('-');
            var bondId = tempId[0];
            if (tempId.Length > 2)
            {
                bondId = tempId[2];
            } 
            Issuer = (string)bond.Item;//Does not handle PartyReference type -> only string!
            SettlementDateCalendar = settlementCalendar;
            Description = "Not Defined";
            if (bond.description != null)
            {
                Description = bond.description;
            }
            MaturityDate = bond.maturity;
            CouponDayCount = new DayCountFraction { Value = bond.dayCountFraction.Value };
            CouponFrequency = new Period
            {
                period = bond.paymentFrequency.period,
                periodMultiplier = bond.paymentFrequency.periodMultiplier
            };
            if (bond.couponRateSpecified)
            {
                CouponRate = bond.couponRate;
            }
            if (bond.parValueSpecified)
            {
                ParValue = bond.parValue;
            }
            CouponType = CouponTypeEnum.Fixed;
            if (bond.clearanceSystem != null)
            {
                ClearanceSystem = bond.clearanceSystem.Value;
            }
            if (bond.exchangeId != null)
            {
                Exchange = bond.exchangeId.Value;
            }
            if (bond.seniority != null)
            {
                Seniority = EnumHelper.Parse<CreditSeniorityEnum>(bond.seniority.Value);
            }
            if (bond.instrumentId != null)
            {
                InstrumentIds = new List<InstrumentId>();
                foreach (var identifier in bond.instrumentId.Select(id => InstrumentIdHelper.Parse(id.Value)))
                {
                    InstrumentIds.Add(identifier);
                }
            }
            //This handles the case of a bond forward used in curve building.
            if (MaturityDate > BaseDate)
            {
                var rollConvention =
                    RollConventionEnumHelper.Parse(MaturityDate.Day.ToString(CultureInfo.InvariantCulture));
                Frequency = FrequencyHelper.ToFrequency(bond.paymentFrequency);
                SettlementDate = GetSettlementDate(baseDate, settlementCalendar, settlementDateOffset);
                UnAdjustedPeriodDates = DateScheduler.GetUnadjustedCouponDatesFromMaturityDate(SettlementDate,
                                                                                               MaturityDate,
                                                                                               CouponFrequency,
                                                                                               rollConvention,
                                                                                               out _,
                                                                                               out var nextCouponDate);
                LastCouponDate = UnAdjustedPeriodDates[0];
                NextCouponDate = nextCouponDate;
                AdjustedPeriodDates =
                    AdjustedDateScheduler.GetAdjustedDateSchedule(UnAdjustedPeriodDates,
                                                                  PaymentBusinessDayAdjustments.businessDayConvention,
                                                                  paymentCalendar).ToArray();
                AdjustedPeriodDates[0] = SettlementDate;
                NextExDivDate = GetNextExDivDate();
                IsXD = IsExDiv();
            }
            BondCurveName = CurveNameHelpers.GetBondCurveName(Currency.Value, bondId);
            SwapDiscountCurveName = CurveNameHelpers.GetDiscountCurveName(Currency.Value, true); 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interpolatedSpace"></param>
        /// <returns></returns>
        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            return Quote.value;//TODO This will only work for ytm quotes.
        }

        /// <summary>
        /// Gets the adjusted termination date.
        /// </summary>
        /// <returns></returns>
        public override DateTime GetRiskMaturityDate()
        {
            return MaturityDate;
        }
    }
}