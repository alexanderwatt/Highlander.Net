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

namespace Orion.Analytics.Stochastics.Pedersen
{
    /// <summary>
    /// Cascade algorithm specifications, contains the maximum allowed scaling factor when fitting caplets and swaptions data.
    /// </summary>
    public class CascadeParameters
    {
        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public CascadeParameters()
        {
            SwaptionMaxScale = 1.15;
            CapletMaxScale = 1.1;
        }

        /// <summary>
        /// Constructor with custom variables
        /// </summary>
        /// <param name="swaptionMaxScale"></param>
        /// <param name="capletMaxScale"></param>
        public CascadeParameters(double swaptionMaxScale, double capletMaxScale)
        {
            SwaptionMaxScale = swaptionMaxScale;
            CapletMaxScale = capletMaxScale;
        }

        #endregion

        #region Private Fields

        ///<summary>
        ///</summary>
        public double SwaptionMaxScale { get; }

        ///<summary>
        ///</summary>
        public double CapletMaxScale { get; }

        #endregion
    }
}