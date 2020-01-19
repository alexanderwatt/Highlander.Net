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

#region Using directives

using Highlander.Reporting.ModelFramework.V5r3.Assets;

#endregion

namespace Highlander.Reporting.ModelFramework.V5r3
{
    /// <summary>
    /// Interface for concrete instruments.
    /// </summary>
    public interface IMarketInstrument// : IObservable
    {
        /// <summary>
        /// ISIN code of the instrument, when given..
        /// </summary>
        string Identifier
        {
            get;
        }

        /// <summary>
        /// A brief textual description of the instrument.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// This method force the recalculation of the instrument value 
        /// and other results which would otherwise be cached.
        /// </summary>
        /// <remarks>
        /// Explicit invocation of this method is <b>not</b> necessary 
        /// if the instrument registered itself as observer with the 
        /// structures on which such results depend. 
        /// It is strongly advised to follow this policy when possible.
        /// </remarks>
        void Recalculate(IAssetControllerData modeData);

        ///<summary>
        /// Gets the contained priceable asset.
        /// This is to allow the binary serialiser
        /// in the Local cache to recognize the type.
        ///</summary>
        ///<returns></returns>
        IPriceableAssetController GetPriceableAsset();
    }
}