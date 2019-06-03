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

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Numerical method (Tree, Finite Differences) interface.
    /// </summary>
    public interface INumericalMethod
    {
        // TODO: Add ToString() method

        ///<summary>
        ///</summary>
        ITimeGrid TimeGrid
        { 
            get; 
        }

        /// <summary>
        /// Initialize a DiscretizedAsset object.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="time"></param>
        void Initialize(IDiscretizedAsset asset, double time);

        /// <summary>
        /// Roll-back a DiscretizedAsset object until a certain time.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="toTime"></param>
        void Rollback(IDiscretizedAsset asset, double toTime);
    }
}