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

#region Using Directives

using System;
using System.Collections.Generic;
using Highlander.Codes.V5r3;
using Highlander.Constants;
using Highlander.Reporting.Analytics.V5r3.Dates;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Helpers;

#endregion

namespace Highlander.CurveEngine.V5r3.Assets.Rates.Bonds
{
    /// <summary>
    /// PriceableBondAsset
    /// </summary>
    public class PriceableBond : PriceableBondAsset
    {
        /// <summary>
        /// 
        /// </summary>
        protected DayCountFraction YieldDC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected DayCountFraction RepoDC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected ExDividendEnum Xdt { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected string SettC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected BondSettlementEnum ValueC { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected short TickSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected BondAnalytics.YieldCalcMethod Ycm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected short YieldCompFreq { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected short AccIntRounding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected DateTime FirstAccDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected DateTime FirstCoupDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int RollDay { get; set; }
        /// <summary>
        /// 
        /// </summary>
        protected DateTime DealDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected DateTime ValueDate { get; }

        /// <summary>
        /// 
        /// </summary>
        protected DateTime PrevCoupDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected DateTime NextCoupDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal AccruedInterest { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected double NextCoupTimeFrac { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal RepaymentPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal CleanPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected decimal DirtyPrice { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected double Yield { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Double[] CashFlows { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Double[] Coupons { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Double[] Principals { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected BondTimeVar Btv { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected BondPriceVar Bpv { get; set; }

        /// <summary>
        /// PriceableBond
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instrumentid"></param>
        /// <param name="bondType"></param>
        /// <param name="dealDate"></param>
        /// <param name="valueDate"></param>
        /// <param name="ccy"></param>
        /// <param name="calendar"></param>
        /// <param name="coupFreq"></param>
        /// <param name="accrualDC"></param>
        /// <param name="yieldDC"></param>
        /// <param name="repoDC"></param>
        /// <param name="xdt"></param>
        /// <param name="couponType"></param>
        /// <param name="couponRate"></param>
        /// <param name="settC"></param>
        /// <param name="maturity"></param>
        /// <param name="valueC"></param>
        /// <param name="tickSize"></param>
        /// <param name="issuerName"></param>
        /// <param name="ycm"></param>
        /// <param name="yieldCompFreq"></param>
        /// <param name="accIntRounding"></param>
        public PriceableBond(string name, string instrumentid, string bondType,
                             DateTime dealDate, DateTime valueDate, Currency ccy, IBusinessCalendar calendar, 
                             Period coupFreq, DayCountFraction accrualDC, DayCountFraction yieldDC, DayCountFraction repoDC,
                             ExDividendEnum xdt, CouponTypeEnum couponType, decimal couponRate, string settC, DateTime maturity,
                             BondSettlementEnum valueC, short tickSize, string issuerName, BondAnalytics.YieldCalcMethod ycm,
                             short yieldCompFreq, short accIntRounding)
            : base(dealDate, 100.0m, ccy, null, null, null, null)
        {
            Currency = ccy;
            ValueDate = valueDate;
            DealDate = dealDate;
            Frequency = int.Parse(coupFreq.periodMultiplier); //convert to using the bond fpml paymentfrequency field.
            AccIntRounding = accIntRounding;
            CouponType = couponType;
            CouponRate = couponRate;
            CouponDayCount = accrualDC;
            BondType = EnumHelper.Parse<BondTypesEnum>(bondType);
            PaymentDateCalendar = calendar;
            RepoDC = repoDC;
            SettC = settC;
            TickSize = tickSize;
            ValueC = valueC;
            Xdt = xdt;
            Ycm = ycm;
            YieldCompFreq = yieldCompFreq;
            YieldDC = yieldDC;
            RollDay = maturity.Day; //provide an input for this.
            ValueDate = SettlementDate;//default condition if not specified.
            Id = name;
            Issuer = issuerName;//Does not handle PartyReference type -> only string!           
            MaturityDate = maturity;
            CouponFrequency = coupFreq;
            CouponType = CouponTypeEnum.Fixed;
            InstrumentIds = new List<InstrumentId> {InstrumentIdHelper.Parse(instrumentid)};
            Build();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CalcSettlementDate()
        {
            var rollDays = PeriodHelper.Parse(SettC); //TODO add dayType - Business/Calendar etc.
            SettlementDate = PaymentDateCalendar.Advance(DealDate, OffsetHelper.FromInterval(rollDays, DayTypeEnum.Calendar), BusinessDayConventionEnum.FOLLOWING);      
        }

        /// <summary>
        /// Build the bond.
        /// </summary>
        public void Build()
        {
            CalcSettlementDate();
            //DateTimeScheduler.GetBondCouponDates(ValueDate, SettlementDate, FirstAccDate, FirstCoupDate, LastRegCoupDate, 
            //Maturity, Calendar, CoupFreq, RollDay, Xdt);
        }

        /// <summary>
        /// Price the bond without rebuilding all the cashflows.
        /// </summary>
        public void Price()
        {
            if (!IsBuilt)
                Build(); //Price stuff here.
        }
            

        //public static void SimpleYieldParams(PriceableBond bond, DayCountType yieldDC, out double A, out double B)
        //{
        //  double T, C, R;
        //  if (bond.m_coupons || bond.m_principals)
        //  {
        //    C = 0;
        //    var lastDate = bond.SettlementDate;
        //    for (int i = bond.CoupNum; i < bond.m_nFlows; lastDate = bond.m_flow[i++].m_date)
        //    {
        //      double ct = DCF(lastDate, bond.m_flow[i].m_date, yieldDC) * 
        //        (bond.m_coupons? bond.m_coupons[i]: bond.m_coupon);
        //      if (bond.m_principals) ct *= bond.m_principals[i] / 100;
        //      C += ct;
        //    }
        //    C /= T;
        //  }
        //  else C = bond.m_coupon;
        //  R = bond.m_principals? bond.m_principals[bond.m_coupNum] - 
        //    bond.m_principals[bond.m_nFlows]: bond.m_repaymentPrice;
        //  A = -100 / T;
        //  B = 100 * (C + R / T);
        //  if (bond.m_principals) B /= bond.m_principals[bond.m_coupNum] / 100;
        //} 

        
        /*
        public static double NextCoupDP(Bond bond, double v, DayCountType yieldDC)
        {
          int i = bond.m_nFlows - 1;
          double dirtyPrice = bond.m_flow[i].m_amount;
          if (bond._lastRegCoupDate && bond._coupNum < i)
          {
            dirtyPrice = Math.Pow(v, bond.FinalCoupTimeFrac(bond.m_lastRegCoupDate, bond.m_maturityDate, yieldDC)) *
              dirtyPrice + bond.m_flow[--i].m_amount;
          }
          while (i > bond.m_coupNum) dirtyPrice = v * dirtyPrice + bond.m_flow[--i].m_amount;
          return dirtyPrice;
        }
 */               
        



        /*       public static double FirstCoupYF(DateTime date, bool isXD, int rollDay, 
            DateTime firstAccDate, DateTime firstCoupDate, 
            DayCountType accrualDC, int coupFreq)
        {
          if (date <= _firstAccDate) return 0; // Acc int is zero until after 1st acc date
          if (_accrualDC == DC_Act365L) return (date - firstAccDate) / (365. + IsLeapYear(firstCoupDate));
          if (_accrualDC == DC_ActAct)
          {
            double f = BondActAct(firstCoupDate, date, coupFreq, rollDay);
            if (_isXD) return f;
            return f - BondActAct(firstCoupDate, firstAccDate, coupFreq, rollDay);
          }

          if (accrualDC == DC_Act365JGB)
          {
        // Acc int is Act/365 in first coup pd, plus an extra day ...
            if (date < firstCoupDate) return (date + 1 - firstAccDate) / 365.;
        // But 1st coup amt, which we get by accruing to 1st coup date, we get phantom coupons ...
            return -Act365JGB(firstCoupDate, firstAccDate - 1, coupFreq, rollDay);
          }
          double f = DCF(firstAccDate, date, accrualDC);
          if (accrualDC == DC_30I_360) // Italian accrual capped at coupon amt
          {
            double maxAcc = DCF(firstAccDate, firstCoupDate, DC_30I_360) - 1./ 360;
            if (f > maxAcc) f = maxAcc;
          }
          return f;
        }
*/

        /// <summary>
        /// 
        /// </summary>
        public int AccrualDays
        {
            get
            {
                if (SettlementDate <= FirstAccDate) return 0;
                var pcd = IsXD ? NextCoupDate : PrevCoupDate;
                return DayCounterHelper.Parse(CouponDayCount.Value).DayCount(pcd, ValueDate);
            }

        }

        #region Overrides of PriceableBondAssetController

        public override DateTime GetRiskMaturityDate()
        {
            return MaturityDate;
        }

        public override decimal CalculateImpliedQuote(IInterpolatedSpace interpolatedSpace)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}