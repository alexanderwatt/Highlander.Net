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

#region Using directives



#endregion

namespace FpML.V5r10.Reporting.Helpers
{
    public static class InterestRateStreamFactory
    {
        public static InterestRateStream CreateFixedRateStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            InterestRateStream fixedRateStream = CreateGenericStream(paymentDateRelativeTo);           
            return fixedRateStream;
        }

        public static InterestRateStream CreateFloatingRateStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            InterestRateStream floatingRateStream = CreateGenericStream(paymentDateRelativeTo);
            var resetDates = new ResetDates();
            floatingRateStream.resetDates = resetDates;
            floatingRateStream.resetDates.resetRelativeTo = ResetRelativeToEnum.CalculationPeriodEndDate;//default value                     
            return floatingRateStream;
        }

        private static InterestRateStream CreateGenericStream(PayRelativeToEnum paymentDateRelativeTo)
        {
            var stream = new InterestRateStream();
            var calculationPeriodDates = new CalculationPeriodDates();
            stream.calculationPeriodDates = calculationPeriodDates;
            var paymentDates = new PaymentDates();
            stream.paymentDates = paymentDates;
            paymentDates.payRelativeTo = paymentDateRelativeTo;
            var calculationPeriodAmount = new CalculationPeriodAmount();
            stream.calculationPeriodAmount = calculationPeriodAmount;
            var calculation = new Calculation();
            stream.calculationPeriodAmount.Item = calculation;
            return stream;
        }
    }
}