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

namespace FpML.V5r10.EquityVolatilityCalculator.Exception
{
    /// <summary>
    /// An exception thrown when an operation attempts to create a duplicate in a collection with set semantics 
    /// (<see cref="P:C5.IExtensible`1.AllowsDuplicates"/> is false) or attempts to create a duplicate key in a dictionary.
    /// <para>With collections this can only happen with Insert operations on lists, since the Add operations will
    /// not try to create duplicates and either ignore the failure or report it in a bool return value.
    /// </para>
    /// <para>With dictionaries this can happen with the <see cref="M:C5.IDictionary`2.Add(`0,`1)"/> metod.</para>
    /// </summary>
    [Serializable]
    public class DuplicateNotAllowedException : System.Exception
    {
        /// <summary>
        /// Create a simple exception with no further explanation.
        /// </summary>
        public DuplicateNotAllowedException() : base() { }
        /// <summary>
        /// Create the exception with an explanation of the reason.
        /// </summary>
        /// <param name="message"></param>
        public DuplicateNotAllowedException(string message) : base(message) { }
    }

}
