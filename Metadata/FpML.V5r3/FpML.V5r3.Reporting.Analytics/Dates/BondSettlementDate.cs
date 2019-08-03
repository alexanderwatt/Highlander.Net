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
using FpML.V5r3.Reporting;
using FpML.V5r3.Reporting.Helpers;
using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Dates
{
    /// <summary>
    /// Settlement/value date delay convention
    /// </summary>
    public enum BondSettlementEnum
    {
        /// <summary>no settlement delay</summary>
        SC_dealDate = 1,
        /// <summary>one business day</summary>
        SC_1bd,
        /// <summary>"two business days"</summary>
        SC_2bd,
        /// <summary>one business day plus one calendar day</summary>
        SC_1b1cd,
        /// <summary>three business days</summary>
        SC_3bd,
        /// <summary>three calendar days</summary>
        SC_3d,
        /// <summary>three business days plus one calendar day</summary>
        SC_3b1cd,
        /// <summary>four business days</summary>
        SC_4bd,
        /// <summary>five business days</summary>
        SC_5bd,
        /// <summary>seven calendar days</summary>
        SC_7d,
        /// <summary>seven calendar days plus one business day</summary>
        SC_7c1bd,
        /// <summary>three calendar days</summary>
        SC_3cd,
        /// <summary>seven business days</summary>
        SC_7bd,
        /// <summary>3 business days, but 6 b.d. lockout period before coupons</summary>
        SC_3bd_6bdLO,
        /// <summary>4 business days, but 6 b.d. lockout period before coupons</summary>
        SC_4bd_6bdLO//,
        // /// <summary>Canadian settlement: 2 b.d. for less than 3y to maturity, otherwise 3 b.d.</summary>
        // SC_Canada,
        // /// <summary>Austrian: next Monday but one</summary>
        //SC_Austria,
        // /// <summary>Australian: next b.d. if less than 5y to maturity, otherwise 7 c.d.</summary>
        // SC_Australia,
        // /// <summary>Australian: next b.d. if less than 5y to maturity, otherwise 7 c.d.</summary>
        // SC_SouthAfrica
    }

    /// <summary>
    /// A bond settle helper.
    /// </summary>
    public static class BondSettlementDate
    {

        /// <summary>
        /// Deal settle.
        /// </summary>
        /// <param name="dealDate"></param>
        /// <param name="settC"></param>
        /// <param name="hols"></param>
        /// <returns></returns>
        public static DateTime SettFromDeal(DateTime dealDate, BondSettlementEnum settC,
                                            IBusinessCalendar hols)
        {
            switch (settC)
            {
                case BondSettlementEnum.SC_dealDate: return dealDate;
                case BondSettlementEnum.SC_1bd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(1), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_2bd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(2), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_1b1cd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(1), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING).AddDays(1);
                case BondSettlementEnum.SC_3bd:
                case BondSettlementEnum.SC_3bd_6bdLO: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(3), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_3b1cd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(3), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING).AddDays(1);
                case BondSettlementEnum.SC_4bd:
                case BondSettlementEnum.SC_4bd_6bdLO: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(4), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_5bd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(5), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_7bd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(7), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_3d: return hols.Roll(dealDate.AddDays(3), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_7d: return hols.Roll(dealDate.AddDays(7), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_7c1bd: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(7), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
                case BondSettlementEnum.SC_3cd: return hols.Roll(dealDate.AddDays(3), BusinessDayConventionEnum.FOLLOWING);
                    //               case BondSettlementHelper.BondSettlementEnum.SC_Canada:
                    //                   return 

                default: return hols.Advance(dealDate, OffsetHelper.FromInterval(IntervalHelper.FromDays(1), DayTypeEnum.Business), BusinessDayConventionEnum.FOLLOWING);
            }
            /*           switch (settC)
                       {
                           case SC_Canada:
                               CalendarDate(ym, mm, dm, maturityDate);
                               return TCs->NextBizDay(dealDate, 2 + (dealDate <= UnadjustedDate(ym - 3, mm, dm)));

                           case SC_Austria: return TCs->NextBizDay(dealDate + 15 - DayOfWeek(dealDate), 0);

                           case SC_Australia:
                               CalendarDate(ym, mm, dm, maturityDate);
                               return TCs->NextBizDay(dealDate, dealDate > UnadjustedDate(ym, mm - 6, dm) ? 1 : 3);

                           case SC_SouthAfrica:
                               settDate = dealDate + 15 - DayOfWeek(dealDate + 4);
                               if (!TCs->IsBizDay(settDate + 1)) settDate--;
                               return settDate;
                       } 
                       return dealDate;*/
        }

        /// <summary>
        /// Formats the type.
        /// </summary>
        /// <param name="SettC"></param>
        /// <returns></returns>
        public static string FormatBondSettlement(BondSettlementEnum SettC)
        {
            switch (SettC)
            {
                case BondSettlementEnum.SC_dealDate: return "Deal";
                case BondSettlementEnum.SC_1bd: return "1bd";
                case BondSettlementEnum.SC_2bd: return "2bd";
                case BondSettlementEnum.SC_1b1cd: return "1bd1cd";
                case BondSettlementEnum.SC_3bd: return "3bd";
                case BondSettlementEnum.SC_3b1cd: return "3bd1cd";
                case BondSettlementEnum.SC_4bd: return "4bd";
                case BondSettlementEnum.SC_5bd: return "5bd";
                case BondSettlementEnum.SC_7bd: return "7bd";
                case BondSettlementEnum.SC_3d: return "3cd";
                case BondSettlementEnum.SC_3cd: return "3cd";
                case BondSettlementEnum.SC_7d: return "7cd";
                case BondSettlementEnum.SC_7c1bd: return "7cd1bd";
                    //              case BondSettlementHelper.BondSettlementEnum.SC_Canada: return "Canadian";
                    //               case BondSettlementHelper.BondSettlementEnum.SC_Austria: return "Austrian";
                    //                case BondSettlementHelper.BondSettlementEnum.SC_Australia: return "Australian";
                    //                case BondSettlementHelper.BondSettlementEnum.SC_SouthAfrica: return "South African";
                case BondSettlementEnum.SC_3bd_6bdLO: return "3bd (6bd LO)";
                case BondSettlementEnum.SC_4bd_6bdLO: return "4bd (6bd LO)";
                default: return "";
            }
        }

        //TODO - need to complete form other inputs.
        /*       BondSettlementHelper.BondSettlementEnum ParseSettConv(string settlementText)
               {
                   switch (settlementText.ToLower())
                   {
                       case "canada": return BondSettlementHelper.BondSettlementEnum.SC_Canada;
                       case "sa":
                       case "south african": return BondSettlementHelper.BondSettlementEnum.SC_SouthAfrica;
                       case "autrian":return BondSettlementHelper.BondSettlementEnum.SC_Austria;
                       case "australian": return BondSettlementHelper.BondSettlementEnum.SC_Australia;
                       case "deal": return BondSettlementHelper.BondSettlementEnum.SC_dealDate;
                       default: return BondSettlementHelper.BondSettlementEnum.SC_dealDate;
                   }
               } */
        /* Converting yield compounding frequencies. Use coupon frequency of zero to mean continuously
   compounded yield ... */

    }
}