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
    [Serializable]
    public class FloatingCashflowResults : CashflowResults, IFloatingCashflowResults
    {
        /// <summary>
        /// Gets the Delta1.
        /// </summary>
        /// <value>The Delta0.</value>
        public Decimal Delta0 { get; set; }

        /// <summary>
        /// Gets the LocalCurrencyDelta0.
        /// </summary>
        /// <value>The LocalCurrencyDelta0.</value>
        public Decimal LocalCurrencyDelta0 { get; set; }

        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenStrike { get; set; }

        /// <summary>
        /// Gets the index at maturity.
        /// </summary>
        /// <value>The index at maturity.</value>
        public decimal IndexAtMaturity { get; set; }

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public Decimal[] PCE { get; set; }

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public int[] PCETerm { get; set; }

        /// <summary>
        /// Gets the break even index.
        /// </summary>
        /// <value>The break even index.</value>
        public Decimal BreakEvenIndex { get; set; }
    }
}
