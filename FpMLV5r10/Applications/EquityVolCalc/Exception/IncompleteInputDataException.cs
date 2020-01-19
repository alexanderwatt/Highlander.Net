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

using System;
using System.Runtime.Serialization;

namespace FpML.V5r10.EquityVolatilityCalculator.Exception
{
    /// <summary>
    /// When there is incomplete input data
    /// </summary>
    [Serializable]
    public class IncompleteInputDataException : System.Exception
    {
               const string CMessage = "Extrapolation Failure: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        public IncompleteInputDataException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IncompleteInputDataException(string message): base(string.Format(CMessage, message) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IncompleteInputDataException(string message, System.Exception innerException) : base(string.Format(CMessage, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteInputDataException"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected IncompleteInputDataException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
