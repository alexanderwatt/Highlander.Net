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

namespace FpML.V5r3.Reporting.Helpers
{
    public static class AmountScheduleHelper
    {
        public static AmountSchedule Create(Currency value)
        {
            var amountSchedule = new AmountSchedule {currency = value};
            return amountSchedule;
        }

        public static AmountSchedule Create(Schedule schedule, Currency currency)
        {
            var result = new AmountSchedule
                                        {
                                            currency = currency,
                                            initialValue = schedule.initialValue,
                                            step = schedule.step
                                        };
            return result;
        }
    }
    public static class NonNegativeAmountScheduleHelper
    {
        public static NonNegativeAmountSchedule Create(Currency value)
        {
            var amountSchedule = new NonNegativeAmountSchedule { currency = value };
            return amountSchedule;
        }

        public static NonNegativeAmountSchedule Create(NonNegativeSchedule schedule, Currency currency)
        {
            var result = new NonNegativeAmountSchedule
            {
                currency = currency,
                initialValue = schedule.initialValue,
                step = schedule.step
            };
            return result;
        }
    }
}