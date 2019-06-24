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

using System.Collections.Generic;

#endregion

namespace FpML.V5r11.Reporting
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Product
    {
        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public virtual List<string> GetRequiredPricingStructures() 
        {
            // this class is abstract
            // - derived classes must implement this method
            return new List<string>();
        }

        /// <summary>
        /// Gets and sets the required pricing structures to value this leg.
        /// </summary>
        public virtual List<string> GetRequiredCurrencies()
        {
            // this class is abstract
            // - derived classes must implement this method
            return new List<string>();
        }
    }
}
