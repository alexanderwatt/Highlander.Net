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

using System;
using System.Collections.Generic;
using Orion.Util.Helpers;

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public interface ICashflowResults
    {
        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        Decimal BucketedDelta1 { get; }

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        Decimal[] BucketedDeltaVector { get; }


        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        Decimal[] BucketedDeltaVector2 { get; }

        /// <summary>
        /// Gets the total delta in the base currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        Decimal LocalCurrencyAnalyticalDelta { get; }

        /// <summary>
        /// Gets the total delta in the reporting currency.
        /// </summary>
        /// <value>The total analytical delta: delta0 and delta1.</value>
        Decimal AnalyticalDelta { get; }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        Decimal Delta1 { get; }

        /// <summary>
        /// Gets the total gamma in the base currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        Decimal LocalCurrencyAnalyticalGamma { get; }

        /// <summary>
        /// Gets the total gamma in the reporting currency.
        /// </summary>
        /// <value>The total analytical gamma: gamma0, gamma1 and delta0delta1.</value>
        Decimal AnalyticalGamma { get; }

        /// <summary>
        /// Gets the second derivative of the forecast rate.
        /// </summary>
        /// <value>The Gamma1.</value>
        Decimal Gamma1 { get; }

        /// <summary>
        /// Gets the local currency gamma1.
        /// </summary>
        /// <value>The Delta1.</value>
        Decimal LocalCurrencyGamma1 { get; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        Decimal ExpectedValue { get; }

        /// <summary>
        /// Gets the CalculatedValue.
        /// </summary>
        /// <value>The CalculatedValue.</value>
        decimal CalculatedValue { get; }

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        decimal HistoricalValue { get; }

        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        Decimal NFV { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        Decimal NPV { get; }

        /// <summary>
        /// Gets the risk npv.
        /// </summary>
        /// <value>The risk npv.</value>
        IList<Pair<string, decimal>> RiskNPV { get; }
        
        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        Decimal SimpleCVA { get; }

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        Decimal HistoricalDelta1 { get; }

        /// <summary>
        /// Gets the bucketed delta1.
        /// </summary>
        /// <value>The bucketed delta1.</value>
        Decimal LocalCurrencyBucketedDelta1 { get; }

        /// <summary>
        /// Gets the vector of bucketed delta1.
        /// </summary>
        /// <value>The vector of bucketed delta1.</value>
        Decimal[] LocalCurrencyBucketedDeltaVector { get; }

        /// <summary>
        /// Gets the bucketed delta vector2.
        /// </summary>
        /// <value>The bucketed delta vector_2.</value>
        Decimal[] LocalCurrencyBucketedDeltaVector2 { get; }

        /// <summary>
        /// Basically the expected value.
        /// </summary>
        Decimal LocalCurrencyCalculatedValue { get; }

        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta1.</value>
        Decimal LocalCurrencyDelta1 { get; }

        /// <summary>
        /// Gets the expected value.
        /// </summary>
        /// <value>The expected value.</value>
        Decimal LocalCurrencyExpectedValue { get; }

        /// <summary>
        /// Gets the non-discounted historical value.
        /// </summary>
        /// <value>The non-discounted historical value.</value>
        decimal LocalCurrencyHistoricalValue { get; }

        /// <summary>
        /// Gets the net future value of realised cash flows.
        /// </summary>
        /// <value>The net future value of realised cash flows.</value>
        Decimal LocalCurrencyNFV { get; }

        /// <summary>
        /// Gets the npv.
        /// </summary>
        /// <value>The npv.</value>
        Decimal LocalCurrencyNPV { get; }

        /// <summary>
        /// Gets the cva for the individual cashflow..
        /// </summary>
        /// <value>The cva.</value>
        Decimal LocalCurrencySimpleCVA { get; }

        /// <summary>
        /// Gets the historical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The historical delta1.</value>
        Decimal LocalCurrencyHistoricalDelta1 { get; }

        /// <summary>
        /// Gets the reporting currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        IDictionary<string, Decimal>  Delta1PDH { get; }

        /// <summary>
        /// Gets the reporting currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        IDictionary<string, Decimal> Delta0PDH { get; }

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta1.</value>
        IDictionary<string, Decimal> LocalCurrencyDelta1PDH { get; }

        /// <summary>
        /// Gets the local currency spectrum numerical derivative with respect to the discount Rate.
        /// </summary>
        /// <value>The delta0.</value>
        IDictionary<string, Decimal> LocalCurrencyDelta0PDH { get; }
    }
}