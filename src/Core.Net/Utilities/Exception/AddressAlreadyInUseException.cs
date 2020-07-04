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

namespace Highlander.Utilities.Exception
{
    /// <summary>
    /// An exception thrown when an operation attempts to connect to an address that is already in use.
    /// </summary>
    [Serializable]
    public class AddressAlreadyInUseException : System.Exception
    {
        /// <summary>
        /// Create a simple exception with no further explanation.
        /// </summary>
        public AddressAlreadyInUseException()
        { }

        /// <summary>
        /// Create the exception with an explanation of the reason.
        /// </summary>
        /// <param name="message"></param>
        public AddressAlreadyInUseException(string message) : base(message) { }
    }

}
