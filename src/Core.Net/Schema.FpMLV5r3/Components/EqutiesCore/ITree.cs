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

namespace Highlander.Equities
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITree : ICloneable
    {
        /// <summary>
        /// set the step size
        /// </summary>
        int GridSteps { get; set; }

        /// <summary>
        /// set the volatility
        /// </summary>
        double Sig { get; set; }

        /// <summary>
        /// set the spot
        /// </summary>
        double Spot { get; set; }

        /// <summary>
        /// set the time step size
        /// </summary>
        double Tau { get; set; }

        /// <summary>
        /// get forward rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetR(int idx);

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDiv(int idx);

        /// <summary>
        /// get div rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDivTime(int idx);

        /// <summary>
        /// get up item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetUp(int idx);

        /// <summary>
        /// get dn item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDn(int idx);

        /// <summary>
        /// 
        /// </summary>
        double FlatRate { get; }
      
        /// <summary>
        /// 
        /// </summary>
        bool FlatFlag { get;}

        /// <summary>
        /// get SpotMatrix item
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <returns></returns>
        double GetSpotMatrix(int idx, int jdx);

        /// <summary>
        /// Makes the grid.
        /// </summary>
        /// <param name="myZero">My zero.</param>
        /// <param name="myDivList">My div list.</param>
        void MakeGrid(ZeroCurve myZero, DivList myDivList);

        /// <summary>
        /// Makes the grid.
        /// </summary>
        void MakeGrid();
    }
}

