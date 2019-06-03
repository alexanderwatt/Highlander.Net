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

namespace Orion.EquityCollarPricer.Exception
{
    /// <summary>
    /// When a field has been supplied with an invalid value
    /// </summary>
    public class InvalidValueException: System.Exception
    {
        /// <summary>
        /// Create a simple exception with no further explanation.
        /// </summary>
        public InvalidValueException()
        { }
        /// <summary>
        /// Create the exception with an explanation of the reason.
        /// </summary>
        /// <param name="message"></param>
        public InvalidValueException(string message) : base(message) { }
    }
}
