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

using System;
using System.Runtime.Serialization;

namespace Orion.Equity.VolatilityCalculator.Exception
{
    /// <summary>
    /// Occurs when a lead stock does not have a full volatility surface
    /// </summary>
    [Serializable]
    public class IncompleteLeadSurfaceException : System.Exception
    {
        const string CMessage = "Lead stock surface is not complete: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        public IncompleteLeadSurfaceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public IncompleteLeadSurfaceException(string message): base(string.Format(CMessage, message) )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public IncompleteLeadSurfaceException(string message, System.Exception innerException) : base(string.Format(CMessage, message), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncompleteLeadSurfaceException"/> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected IncompleteLeadSurfaceException(SerializationInfo info,  StreamingContext context): base(info, context)
        {
        }

    }
}
