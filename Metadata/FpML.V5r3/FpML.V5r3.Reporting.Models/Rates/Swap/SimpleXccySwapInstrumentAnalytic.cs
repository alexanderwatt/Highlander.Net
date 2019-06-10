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

namespace Orion.Models.Rates.Swap
{
    public class SimpleXccySwapInstrumentAnalytic : SimpleIRSwapInstrumentAnalytic
    {
        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns></returns>
        public override  Decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            //TODO need to handle the case of a discount swap. There is no parameter flag yet for this.
            //Also need to add in the fx exchange rate.
            if (AnalyticParameters.IsPayFixedInd)
            {
                if (AnalyticParameters.PayStreamAccrualFactor != 0)
                {
                    var npv = AnalyticParameters.PayStreamFloatingNPV - AnalyticParameters.ReceiveStreamNPV;
                    result = npv / AnalyticParameters.PayStreamAccrualFactor / 10000;
                }
            }
            else
            {
                if (AnalyticParameters.ReceiveStreamAccrualFactor != 0)
                {
                    var npv = AnalyticParameters.ReceiveStreamFloatingNPV - AnalyticParameters.PayStreamNPV;
                    result = npv / AnalyticParameters.ReceiveStreamAccrualFactor / 10000;
                }
            }
            return result;
        }
    }
}