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

#region Usings

using System;
using System.Collections.Generic;
using Orion.CalendarEngine.Dates;

#endregion

namespace Orion.CalendarEngine.Helpers
{
    /// <summary>
    /// Produces the relevant CB dates.
    /// </summary>
    public static class CentralBanksHelper
    {
        ///<summary>
        /// Returns the CentralBank for a provided currency string
        ///</summary>
        ///<param name="currency">The three letter currency code</param>
        ///<returns></returns>
        public static CentralBanks GetCentralBank(string currency)
        {
            switch (currency.ToUpper())
            {
                case "GBP":
                    return CentralBanks.BOE;
                case "USD":
                    return CentralBanks.FOMC;
                case "EUR":
                    return CentralBanks.ECB;
                case "AUD":
                    return CentralBanks.RBA;
                case "NZD":
                    return CentralBanks.BNZ;
                case "JPY":
                    return CentralBanks.BOJ;
                case "HKD":
                    return CentralBanks.BHK;
            }
            string message = $"No central bank found for currency '{currency}'";
            throw new ArgumentOutOfRangeException(nameof(currency), message);
        }

        /// <summary>
        /// Creates the date time vector for the request Central Bank.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="validCentralBank">The requested central bank.</param>
        /// <param name="centralBankDateRuleMonths">The rules.</param>
        /// <param name="lastDate">The last date required.</param>
        /// <returns>An array of relevant dates.</returns>
        public static DateTime[] GetCBDates(DateTime baseDate, string validCentralBank,
                                             int centralBankDateRuleMonths, DateTime lastDate)//TODO splice in the quarterly dates afterwards..
        {
            var cb = (CentralBanks)Enum.Parse(typeof(CentralBanks), validCentralBank, true);
            var result = GetCentralBankDays(baseDate, cb, centralBankDateRuleMonths, lastDate);
            return result;
        }

        /// <summary>
        /// Creates the date time vector for the request Central Bank.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="centralBank">The requested central bank.</param>
        /// <param name="centralBankDateRuleMonths">The rules.</param>
        /// <param name="lastDate">The last date required.</param>
        /// <returns>An array of relevant dates.</returns>
        public static DateTime[] GetCentralBankDays(DateTime baseDate, CentralBanks centralBank,
                                                    int centralBankDateRuleMonths, DateTime lastDate)
        {
            ICentralBankDate date = null;
            switch (centralBank)
            {
                case CentralBanks.RBA:
                    date = new RBADate();
                    break;
                case CentralBanks.BNZ:
                    date = new BNZDate();
                    break;
                case CentralBanks.BOE:
                    date = new BOEDate();
                    break;
                case CentralBanks.BOJ:
                    date = new BOJDate();
                    break;
                case CentralBanks.ECB:
                    date = new ECBDate();
                    break;
                case CentralBanks.FOMC:
                    date = new FOMCDate();
                    break;
                case CentralBanks.BHK:
                    date = new BHKDate();
                    break;
            }
            if (date != null)
            {
                var dates1 = date.GetCentralBankDays(baseDate, centralBankDateRuleMonths);
                var dates2 = date.GetCentralBankDays(dates1[dates1.Count - 1], lastDate, false);
                //DateTime[] dates3 = new DateTime[dates1.Length + dates2.Length];
                var result = new List<DateTime>();
                // OMFG
                //
                result.AddRange(dates1);
                result.AddRange(dates2);
                return result.ToArray();
            }
            return new DateTime[] {};
        }
    }
}