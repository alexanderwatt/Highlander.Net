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

using System;

namespace FpML.V5r11.Reporting
{
    public partial class Trade
    {
        public ItemChoiceType16 GetTradeTypeFromItem()
        {
            if (Item == null)
                throw new ArgumentNullException("Item");
            Type type = Item.GetType();
            if (type == typeof(BondOption)) return ItemChoiceType16.bondOption;
            if (type == typeof(Swap)) return ItemChoiceType16.swap;
            if (type == typeof(Fra)) return ItemChoiceType16.fra;
            if (type == typeof(FxSingleLeg)) return ItemChoiceType16.fxSingleLeg;
            if (type == typeof(FxSwap)) return ItemChoiceType16.fxSwap;
            if (type == typeof(FxOption)) return ItemChoiceType16.fxOption;
            if (type == typeof(Swaption)) return ItemChoiceType16.swaption;
            if (type == typeof(CapFloor)) return ItemChoiceType16.capFloor;
            if (type == typeof(TermDeposit)) return ItemChoiceType16.termDeposit;
            if (type == typeof(BulletPayment)) return ItemChoiceType16.bulletPayment;
            if (type == typeof(BondTransaction)) return ItemChoiceType16.bondTransaction;
            if (type == typeof(EquityTransaction)) return ItemChoiceType16.equityTransaction;
            if (type == typeof(FutureTransaction)) return ItemChoiceType16.futureTransaction;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.commodityForward;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.equityForward;
            //if (type == typeof(FutureTransaction)) return ItemChoiceType15.commoditySwap;
            //if (type == typeof(xxx)) return ItemChoiceType16.xxx;
            throw new ArgumentException("Unknown product type: " + type.Name);
        }
    }
}
