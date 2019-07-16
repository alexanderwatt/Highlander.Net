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

using System;

namespace FpML.V5r3.Reporting
{
    public partial class Trade
    {
        public ItemChoiceType15 GetTradeTypeFromItem()
        {
            if (Item == null)
                throw new ArgumentNullException($"Item");
            Type type = Item.GetType();
            if (type == typeof(BondOption)) return ItemChoiceType15.bondOption;
            if (type == typeof(Swap)) return ItemChoiceType15.swap;
            if (type == typeof(Fra)) return ItemChoiceType15.fra;
            if (type == typeof(FxSingleLeg)) return ItemChoiceType15.fxSingleLeg;
            if (type == typeof(FxSwap)) return ItemChoiceType15.fxSwap;
            if (type == typeof(FxOption)) return ItemChoiceType15.fxOption;
            if (type == typeof(Swaption)) return ItemChoiceType15.swaption;
            if (type == typeof(CapFloor)) return ItemChoiceType15.capFloor;
            if (type == typeof(TermDeposit)) return ItemChoiceType15.termDeposit;
            if (type == typeof(BulletPayment)) return ItemChoiceType15.bulletPayment;
            if (type == typeof(BondTransaction)) return ItemChoiceType15.bondTransaction;
            if (type == typeof(EquityTransaction)) return ItemChoiceType15.equityTransaction;
            if (type == typeof(FutureTransaction)) return ItemChoiceType15.futureTransaction;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.commodityForward;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.equityForward;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.commoditySwap;
            //if (type == typeof(xxx)) return ItemChoiceType16.xxx;
            throw new ArgumentException("Unknown product type: " + type.Name);
        }
    }
}
