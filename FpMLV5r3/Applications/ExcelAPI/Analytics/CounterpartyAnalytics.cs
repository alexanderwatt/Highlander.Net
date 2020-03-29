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
using System.Runtime.InteropServices;
using Highlander.Reporting.Analytics.V5r3.Counterparty;
using Highlander.Utilities.Helpers;
using HLV5r3.Helpers;
using Microsoft.Win32;
using ApplicationHelper = HLV5r3.Helpers.ApplicationHelper;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// Class to calculate return on equity 
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("1EBCA9F7-ECAF-4B87-A249-372DFDEB198C")]
    public class Counterparty
    {
        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion

        #region Constructor

        #endregion

        #region Functions

        /// <summary>
        /// Calculates the ROE from given inputs.
        /// </summary>
        /// <param name="revenue"></param>
        /// <param name="sp"></param>
        /// <param name="cost"></param>
        /// <param name="rore"></param>
        /// <param name="dfCapital"></param>
        /// <param name="dfMarket"></param>
        /// <param name="taxRate"></param>
        /// <param name="frankingRate"></param>
        /// <param name="regCap"></param>
        /// <param name="ffp"></param>
        /// <param name="fxRate"></param>
        /// <returns></returns>
        public decimal[,] CalculateROE(decimal[,] revenue, decimal[] sp, decimal[] cost, decimal[] rore,
            decimal[] dfCapital, decimal[] dfMarket, decimal taxRate, decimal frankingRate,
            decimal[] regCap, decimal ffp, decimal fxRate)
        {
            var result = ROEAnalytics.CalculateROE(ArrayHelper.MatrixToArray(revenue), sp, cost, rore, dfCapital,
                                       dfMarket, taxRate, frankingRate, regCap, ffp, fxRate);
            return ArrayHelper.ArrayToVerticalMatrix(result);
        }

        /// <summary>
        /// Calculates the cost of capital.
        /// </summary>
        /// <param name="revenueBuckets"></param>
        /// <param name="costCapital"></param>
        /// <returns></returns>
        public decimal[,] GetCostOfCapitalDFs(int[,] revenueBuckets, decimal costCapital)
        {
            var result = ROEAnalytics.GetCostOfCapitalDFs(ArrayHelper.MatrixToArray(revenueBuckets), costCapital);
            return ArrayHelper.ArrayToVerticalMatrix(result);
        }

        /// <summary>
        /// Calculates the ROE from given inputs.
        /// </summary>
        /// <param name="revenue"></param>
        /// <param name="sp"></param>
        /// <param name="cost"></param>
        /// <param name="rorc"></param>
        /// <param name="dfCapital"></param>
        /// <param name="taxRate"></param>
        /// <param name="frankingRate"></param>
        /// <param name="riskCap"></param>
        /// <param name="ffp"></param>
        /// <param name="fxRate"></param>
        /// <returns></returns>
        public decimal[,] CalculateRAROC(decimal[,] revenue, decimal[,] sp, decimal[,] cost, decimal[,] rorc, decimal[,] dfCapital,
            decimal taxRate, decimal frankingRate, decimal[,] riskCap, decimal ffp, decimal fxRate)
        {
            var result = RAROCAnalytics.CalculateRAROC(ArrayHelper.MatrixToArray(revenue), ArrayHelper.MatrixToArray(sp), ArrayHelper.MatrixToArray(cost), 
                                        ArrayHelper.MatrixToArray(rorc), ArrayHelper.MatrixToArray(dfCapital),
                                         taxRate, frankingRate, ArrayHelper.MatrixToArray(riskCap), ffp, fxRate);
            return ArrayHelper.ArrayToVerticalMatrix(result);
        }

        ///// <summary>
        ///// Method to calculate unexpected loss
        ///// <param name="mdp"></param>
        ///// <param name="lgd"></param>
        ///// <param name="epe"></param>
        ///// <param name="epeSq"></param>
        ///// <param name="df"></param>
        ///// <returns>unexpected loss vector</returns>
        ///// </summary>
        //[ExcelName("UnexpectedLoss")] 
        //public static Decimal[,] UnexpectedLoss(Decimal[,] mdp, Decimal lgd, Decimal[,] epe, Decimal[,] epeSq,
        //                                        Decimal[,] df)
        //{
        //    return UnexpectedLoss(mdp, lgd, epe, epeSq,
        //                                          df);
        //}

        #endregion
    }
}