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


#region Using directives

using FpML.V5r10.Reporting.ModelFramework.Assets;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework.Instruments
{
    /// <summary>
    /// Abstract instrument class.
    /// </summary>
    /// <remarks>
    /// This class is purely abstract and defines the interface of 
    /// concrete instruments which will be derived from this one.
    /// </remarks>
    public class MarketInstrument : IMarketInstrument
    {
        #region Members

        /// <summary>
        /// ISIN code of the instrument, when given..
        /// </summary>
        public string Identifier { get; set;}

        public string Description { get; set; }

        ///<summary>
        /// Stores the priceable asset.
        ///</summary>
        public IPriceableAssetController PriceableAsset { get; set; }

        ///<summary>
        ///</summary>
        public bool Calculated { get; private set; }

        #endregion

        ///<summary>
        ///</summary>
        public MarketInstrument()
            : this("", "") 
        {}

        ///<summary>
        ///</summary>
        ///<param name="identifier"></param>
        ///<param name="description"></param>
        public MarketInstrument(string identifier, string description)
        {
            Identifier = identifier;
            Description = description;
            Calculated = false;
        }

        ///<summary>
        ///</summary>
        ///<param name="description"></param>
        ///<param name="priceableAsset"></param>
        public MarketInstrument(string description, IPriceableAssetController priceableAsset)
        {
            Identifier = priceableAsset.Id;
            Description = description;
            PriceableAsset = priceableAsset;
            Calculated = false;
        }

        /// <summary>
        /// This method performs all needed calculations by calling
        /// the <i><b>performCalculations</b></i> method.
        /// </summary>
        /// <remarks>
        /// Instruments cache the results of the previous calculation. 
        /// Such results will be returned upon later invocations of 
        /// <i><b>calculate</b></i>. When the results depend on 
        /// arguments such as term structures which could change
        /// between invocations, the instrument must register itself 
        /// as observer of such objects for the calculations to be
        /// performed again when they change.
        /// </remarks>
        protected void Calculate(IAssetControllerData modeData)
        {
            if (Calculated) return;
            PriceableAsset.Calculate(modeData);

            Calculated = true;
        }

        #region Implementation of IInstrument


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
        public void Recalculate(IAssetControllerData modeData)
        {
            PriceableAsset.Calculate(modeData);

            Calculated = true;
        }


        /// <summary>
        /// This method returns the stored priceable asset controller.
        /// In this way a MarketInstrument can wrap the controller
        /// for storage in the local cache.
        /// </summary>
        /// <returns>The IPriceableAssetController</returns>
        public IPriceableAssetController GetPriceableAsset()
        {
            return PriceableAsset;
        }

        #endregion

    }
}