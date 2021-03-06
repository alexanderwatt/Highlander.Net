﻿/*
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

using System;
using FpML.V5r10.EquityVolatilityCalculator.Helpers;

namespace FpML.V5r10.EquityVolatilityCalculator
{
    /// <summary>
    /// Represents the different position types
    /// </summary>
    public enum PositionType
    {
        /// <summary>Unknown (Not Specified)</summary>
        NotSpecified = 0,
        /// <summary>Call Position</summary>
        Call = 1,
        /// <summary>Put Position</summary>
        Put = 2       
    }

    /// <summary>
    /// Defines a given position 
    /// </summary>
    [Serializable]
    public class OptionPosition
    {
        private string _contractId;
        private readonly PositionType _positionType = PositionType.NotSpecified;
        private Double _contractPrice;
        private readonly IVolatilityPoint _volatility = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionPosition"/> class.
        /// </summary>
        public OptionPosition() { }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public PositionType Type => _positionType;


        /// <summary>
        /// Gets or sets the contract price.
        /// </summary>
        /// <value>The contract price.</value>
        public Double ContractPrice
        {
            get => _contractPrice;
            set => _contractPrice = value;
        }

        /// <summary>
        /// Gets or sets the contract id.
        /// </summary>
        /// <value>The contract id.</value>
        public string ContractId
        {
            get => _contractId;
            set => _contractId = value;
        }

        /// <summary>
        /// Gets the volatility.
        /// </summary>
        /// <value>The volatility.</value>
        public IVolatilityPoint Volatility => _volatility;


        /// <summary>
        /// Initializes a new instance of the <see cref="OptionPosition"/> class.
        /// </summary>
        /// <param name="contractId">The contract id.</param>
        /// <param name="contractPrice">The contract price.</param>
        /// <param name="positionType">Type of the position.</param>
        /// <example>
        ///     <code>
        ///     // Creates an Call option instance
        ///     OptionPosition option = new OptionPosition("123", 456, PositionType.Call);
        ///     </code>
        /// </example>
        public OptionPosition(string contractId, Double contractPrice, PositionType positionType)
        {
            InputValidator.IsMissingField("ContractId", contractId, true);
            //InputValidator.NotZero("ContractPrice", contractPrice, true);
            _contractId = contractId;
            _contractPrice = contractPrice;
            _positionType = positionType;
        }

        /// <summary>
        /// Sets the volatility.
        /// </summary>
        /// <param name="volatility">The volatility.</param>
        public void SetVolatility(IVolatilityPoint volatility)
        {           
        }
    }
}
