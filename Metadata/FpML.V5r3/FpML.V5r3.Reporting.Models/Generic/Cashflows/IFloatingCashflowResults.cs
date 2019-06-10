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

#endregion

namespace Orion.Models.Generic.Cashflows
{
    public enum FloatingCashflowMetrics
    {
                //Base metrics, fx rate insensitive.
            IndexAtMaturity
            , ImpliedQuote
            , MarketQuote
            , BreakEvenIndex
            , BreakEvenStrike
                //Base metrics, assumed to be in local currency. Do not require an fx rate.
            , Delta1
            , Delta0
            , HistoricalDelta0
            , ExpectedValue
            , CalculatedValue
            , HistoricalValue
            , NFV
            , NPV
            , SimpleCVA 
            , BucketedDelta1
            , BucketedDeltaVector
            , BucketedDeltaVector2
            , HistoricalDelta1

                //Base metrics explicitly in local currency. Do not require an fx rate.
            , LocalCurrencyDelta1
            , LocalCurrencyDelta0
            , LocalCurrencyFloatingNPV
            , LocalCurrencyExpectedValue
            , LocalCurrencyCalculatedValue
            , LocalCurrencyHistoricalValue
            , LocalCurrencyNFV
            , LocalCurrencyNPV
            , LocalCurrencySimpleCVA 
            , LocalCurrencyBucketedDelta1
            , LocalCurrencyBucketedDeltaVector
            , LocalCurrencyBucketedDeltaVector2
            , LocalCurrencyHistoricalDelta1
            , PCE
            , PCETerm
    }

    public interface IFloatingCashflowResults : ICashflowResults
    {
        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta0.</value>
        Decimal Delta0 { get; }

        /// <summary>
        /// Gets the LocalCurrencyDelta0.
        /// </summary>
        /// <value>The LocalCurrencyDelta0.</value>
        Decimal LocalCurrencyDelta0 { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenIndex { get; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        Decimal BreakEvenStrike { get; }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        Decimal IndexAtMaturity { get; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        Decimal[] PCE { get; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        int[] PCETerm { get; }
    }
}
