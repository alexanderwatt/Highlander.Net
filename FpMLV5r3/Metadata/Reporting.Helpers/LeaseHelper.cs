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

#region Using directives

using System;
using Highlander.Reporting.V5r3;

#endregion

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class LeaseHelper
    {
        public  static  Lease Create(string id, string propertyReference, DateTime nextReviewDate, DateTime baseDate, Period tenor, string currency,
            Period paymentFrequency, string businessDayConvention, string businessCentersAsString)
        {
            var lease = new Lease
            {
                startDate = new IdentifiedDate { id = "StartDate", Value = baseDate },
                leaseExpiryDate = new IdentifiedDate{id = "MaturityDate", Value = tenor.Add(baseDate)},
                leaseTenor = tenor,
                businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCentersAsString),
                currency = new IdentifiedCurrency{id = "PaymentCurrency", Value = currency },
                leaseType = "Standard",
                leaseIdentifier = id,
                propertyReference = propertyReference,
                nextReviewDate = new IdentifiedDate { id = "NextReviewDate", Value = nextReviewDate },
                paymentFrequency = paymentFrequency,
            };
            return lease;
        }

        public static LeaseNodeStruct CreateConfiguration(string id, string currency, string paymentFrequency, string businessDayConvention, string businessCentersAsString)
        {
            var node = new LeaseNodeStruct();
            var lease = new Lease
            {
                businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCentersAsString),
                currency = new IdentifiedCurrency { id = "PaymentCurrency", Value = currency },
                leaseType = "Standard",
                leaseIdentifier = id,
                paymentFrequency = PeriodHelper.Parse(paymentFrequency)
            };
            node.Lease = lease;
            return node;
        }
    }
}