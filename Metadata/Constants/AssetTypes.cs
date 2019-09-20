/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;

#endregion

namespace Orion.Constants
{
    ///<summary>
    ///</summary>
    public enum AssetTypesEnum
    {
        BasisSwap,
        Bond,
        Deposit,
        XccyDepo,
        IRFuture,
        IRFutureOption,
        Caplet,
        Floorlet,
        BillCaplet,
        BillFloorlet,
        FxSpot,
        FxForward,
        FxFuture,
        CommoditySpot,
        CommodityForward,
        CommodityFuture,
        CommodityAverageForward,
        CommoditySpread,
        CommodityFutureSpread,
        BankBill,
        SimpleFra,
        Fra,
        SpreadDeposit,
        SpreadFra,
        BillFra,
        SimpleIRSwap,
        IRSwap,
        XccySwap,
        XccyBasisSwap,
        IRCap,
        Xibor,
        OIS,
        OISSwap,
        CPIndex,
        CPISwap,
        SimpleCPISwap,
        ZCCPISwap,
        ZeroRate,
        Equity,
        EquityForward,
        EquitySpread,
        BondSpot,
        BondForward,
        Repo,
        RepoSpread,
        ClearedIRSwap,
        ResettableXccyBasisSwap,
        IRFloor,
        IRPutFutureOption,
        IRCallFutureOption,
        Swaption,
        Period,
        Property,
        Lease
    }

    public class AssetTypesValue
    {
        public static bool TryParseEnumString(string assetIdString, out AssetTypesEnum id)
        {
            // note: we cannot use Enum.Parse() here, hence the loop...
            foreach (AssetTypesEnum tempId in Enum.GetValues(typeof (AssetTypesEnum)))
            {
                if (string.Compare(assetIdString, tempId.ToString(),
                                   StringComparison.OrdinalIgnoreCase) != 0) continue;
                id = tempId;
                return true;
            }
            id = AssetTypesEnum.Deposit; //The default type.
            return false;
        }

        public static AssetTypesEnum ParseEnumString(string idString)
        {
            if (!TryParseEnumString(idString, out var result))
                throw new ArgumentException($"Cannot convert '{idString}' to AssetTypesEnum");
            return result;
        }
    }
}