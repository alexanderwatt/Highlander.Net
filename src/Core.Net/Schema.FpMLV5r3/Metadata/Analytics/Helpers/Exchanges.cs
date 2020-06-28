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

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    /// <summary>
    /// Imm month codes.
    /// </summary>
    public enum FuturesCodesEnum
    {
        ///<summary>
        ///</summary>
        Unknown = 0,
        ///<summary>
        ///</summary>
        F,
        ///<summary>
        ///</summary>
        G,
        ///<summary>
        ///</summary>
        H,
        ///<summary>
        ///</summary>
        J,
        ///<summary>
        ///</summary>
        K,
        ///<summary>
        ///</summary>
        M,
        ///<summary>
        ///</summary>
        N,
        ///<summary>
        ///</summary>
        Q,
        ///<summary>
        ///</summary>
        U,
        ///<summary>
        ///</summary>
        V,
        ///<summary>
        ///</summary>
        X,
        ///<summary>
        ///</summary>
        Z
    }


    /// <summary>
    /// Methods for determining futures expiry dates.
    /// </summary>
    public enum FuturesExchangeCodeEnum
    {
        /// <summary>"3rd Wednesday (International Money Markets rule)"<summary>
        IMM = 1,
        /// <summary>"3rd Wednesday minus 2 London business days (Montreal Exchange, CAD BA's)"<summary>
        ME,
        /// <summary>"Wed 10th-16th + 1 Auckland b.d. (NZ Futures & Options Exchange, NZD BA Bills)"<summary>
        NZFOE,
        /// <summary>"2nd Friday (Sydney Futures Exchange, AUD BA Bills)"<summary>
        SFE,
        /// <summary>"Thu 22nd-28th + 1 Johannesburg b.d. (South African Futures Exchange, ZAR Bank Bills)"<summary>
        SAFEX,
        /// <summary>"Mon before 3rd Wed adj fwd for good MAT b.d., plus 1 MATIF b.d."<summary>
        MATIF,
        /// <summary>"3rd Wednesday minus 2 Copenhagen b.d. (Kobenhavn Fondsbors/FUTOP)"<summary>
        FUTOP,
        /// <summary>"3rd Wednesday plus 2 Kuala Lumpur b.d. (Malaysian Monetary Exchange)"<summary>
        MME
    }


    ///<summary>
    ///</summary>
    public class Exchanges
    {

        /// <summary>
        /// Converts to a string representation of a futures day rule.
        /// </summary>
        /// <param name="futuresDayRule"></param>
        /// <returns>The string representation</returns>
        public static string ToString(FuturesExchangeCodeEnum futuresDayRule)
        {
            switch (futuresDayRule)
            {
                case FuturesExchangeCodeEnum.IMM: return "IMM";
                case FuturesExchangeCodeEnum.ME: return "ME";
                case FuturesExchangeCodeEnum.NZFOE: return "NZFOE";
                case FuturesExchangeCodeEnum.SFE: return "SFE";
                case FuturesExchangeCodeEnum.SAFEX: return "SAFEX";
                case FuturesExchangeCodeEnum.MATIF: return "MATIF";
                case FuturesExchangeCodeEnum.FUTOP: return "FUTOP";
                case FuturesExchangeCodeEnum.MME: return "MME";
                default: return "Unknown futures day rule";
            }
        }

        /// <summary>
        /// Parses a string to a futures day rule.
        /// </summary>
        /// <param name="futuresDayRule"></param>
        /// <returns>A futures day rule.</returns>
        public static FuturesExchangeCodeEnum Parse(string futuresDayRule)
        {
            switch (futuresDayRule.ToUpper())
            {
                case "IMM": return FuturesExchangeCodeEnum.IMM;
                case "ME": return FuturesExchangeCodeEnum.ME;
                case "NZFOE": return FuturesExchangeCodeEnum.NZFOE;
                case "SFE": return FuturesExchangeCodeEnum.SFE;
                case "SAFEX": return FuturesExchangeCodeEnum.SAFEX;
                case "MATIF": return FuturesExchangeCodeEnum.MATIF;
                case "FUTOP": return FuturesExchangeCodeEnum.FUTOP;
                case "MME": return FuturesExchangeCodeEnum.MME;
                default: return FuturesExchangeCodeEnum.IMM;
            }
        }
    }
}

/*
public class IMMDate
{
    // Calculate futures expiry settlement day ...

    public DateTime IMMDate(int year, int month, FuturesDayRule fdr)
    {
        switch (fdr)
        {
            case FuturesDayRule.FDR_ME:    // 3rd Wednesday minus 2 London business days (Montreal Exchange)
                DateTime dte = new DateTime(year, month, 21);
                return London.Instance.Advance(DateTime.FromOADate(dte.ToOADate() - (dte.ToOADate() + 3) % 7), new Period(-2, TimeUnit.Days), BusinessDayConvention.Preceding);
            case FuturesDayRule.FDR_NZFOE: // Wed 10th-16th + 1 Auckland b.d. (NZ Futures & Options Exchange)
                dte = new DateTime(year, month, 16);
                return Wellington.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 3) % 7), new Period(0, TimeUnit.Days), BusinessDayConvention.Following);
            case FuturesDayRule.FDR_SFE:   // 2nd Friday (Sydney Futures Exchange)
                dte = new DateTime(year, month, 14);
                return Sydney.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 1) % 7), new Period(0, TimeUnit.Days), BusinessDayConvention.Following);
            case FuturesDayRule.FDR_SAFEX: // 3rd Thursday + 1 Johannesburg b.d. (South African Futures Exchange)
                dte = new DateTime(year, month, 21);
                return Johannesburg.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 2) % 7), new Period(-1, TimeUnit.Days), BusinessDayConvention.Preceding);
            case FuturesDayRule.FDR_MATIF: // BD on or immediately after Mon before 3rd Wed + 1 b.d.
                dte = new DateTime(year, month, 21);
                return Target.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 3) % 7 - 2), new Period(1, TimeUnit.Days), BusinessDayConvention.Following);
            case FuturesDayRule.FDR_FUTOP:
                dte = new DateTime(year, month, 21);
                return Target.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 3) % 7), new Period(-2, TimeUnit.Days), BusinessDayConvention.Preceding);
            case FuturesDayRule.FDR_MME: //Add KL hoiday Calendar.
                dte = new DateTime(year, month, 21);
                return Target.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 3) % 7), new Period(2, TimeUnit.Days), BusinessDayConvention.Following);
            default: //Convert to Chicago.
                dte = new DateTime(year, month, 21);
                return NewYork.Instance.Advance(DateTime.FromOADate(Convert.ToInt32(dte.ToOADate()) - (Convert.ToInt32(dte.ToOADate()) + 3) % 7), new Period(0, TimeUnit.Days), BusinessDayConvention.Following);
        }
    }

        
}*/
