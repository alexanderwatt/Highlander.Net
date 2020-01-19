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

namespace Highlander.Numerics.Utilities
{
    /// <summary>
    /// Static class that encapsulates the concept of a financial instrument,
    /// for example Swaption, CapFloor.
    /// The class is a central storage for the names of valid financial
    /// instruments associated with various analytics components.
    /// </summary>
    public static class InstrumentType
    {
        #region Enumerated Type of all Valid Instruments

        /// <summary>
        /// The instrument.
        /// </summary>
        public enum Instrument
        {
            /// <summary>
            /// Cap/Floor
            /// </summary>
            CapFloor,
            /// <summary>
            /// Swaption
            /// </summary>
            Swaption,
            /// <summary>
            /// Call/Put
            /// </summary>
            CallPut

            
        }

        #endregion Enumerated Type of all Valid Instruments
    }
}