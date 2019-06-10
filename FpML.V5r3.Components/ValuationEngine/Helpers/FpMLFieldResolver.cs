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

#region Usings

using FpML.V5r3.Reporting;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    public class FpMLFieldResolver
    {
        public static void TradeSetFxSwap(Trade trade, FxSwap swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.fxSwap;
        }

        public static void TradeSetFxLeg(Trade trade, FxSingleLeg fxLeg)
        {
            trade.Item = fxLeg;
            trade.ItemElementName = ItemChoiceType15.fxSingleLeg;
        }

        public static void TradeSetFxOptionLeg(Trade trade, FxOption fxLeg)
        {
            trade.Item = fxLeg;
            trade.ItemElementName = ItemChoiceType15.fxOption;
        }

        public static void TradeSetBulletPayment(Trade trade, BulletPayment payment)
        {
            trade.Item = payment;
            trade.ItemElementName = ItemChoiceType15.bulletPayment;
        }

        public static void TradeSetTermDeposit(Trade trade, TermDeposit deposit)
        {
            trade.Item = deposit;
            trade.ItemElementName = ItemChoiceType15.termDeposit;
        }

        public static void TradeSetCapFloor(Trade trade, CapFloor capFloor)
        {
            trade.Item = capFloor;
            trade.ItemElementName = ItemChoiceType15.capFloor;
        }

        //public static void TradeSetFxSimpleOptionLeg(Trade trade, FxOption fxOptionLeg)
        //{
        //    trade.Item = fxOptionLeg;
        //    trade.ItemElementName = ItemChoiceType15.fxOption;
        //}
    }
}
