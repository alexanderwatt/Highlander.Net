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

namespace Orion.Equity.VolatilityCalculator.Pricing
{
    public interface ITree : ICloneable
    {
        //set the step size
        int Gridsteps { get; set; }

        //set the volatility
        double Sig { get; set; }

        //set the spot
        double Spot { get; set; }

        //set the time step size
        double Tau { get; set; }

        //get forward rate item
        double GetR(int idx);

        //get div rate item
        double GetDiv(int idx);

        //get div rate item
        double GetDivtime(int idx);

        //get up item
        double GetUp(int idx);

        //get dn item
        double GetDn(int idx);

        double FlatRate { get; }

        bool FlatFlag { get; }

        //get SpotMatrix item
        double GetSpotMatrix(int idx, int jdx);

        /// <summary>
        /// Makes the grid.
        /// </summary>
        void MakeGrid();

        //object Clone();
    }
}
