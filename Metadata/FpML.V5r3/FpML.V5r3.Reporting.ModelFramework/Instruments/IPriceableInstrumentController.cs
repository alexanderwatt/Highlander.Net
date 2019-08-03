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

using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.Instruments
{
    /// <summary>
    /// TFpML - Denotes the FpML structure that gets built from the underlying Instrument
    /// </summary>
    /// <typeparam name="TFpML">The type of the fpML.</typeparam>
    public interface IPriceableInstrumentController<out TFpML> : IModelController<IInstrumentControllerData, AssetValuation>
    {
        /// <summary>
        /// Builds this instance and returns the underlying instrument associated with the controller
        /// </summary>
        /// <returns></returns>
        TFpML Build();
    }
}