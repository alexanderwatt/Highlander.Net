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

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Time grid interface.
    /// </summary>
    public interface ITimeGrid // public std::SparseVector<Time> 
    {
        // TODO: Add a ToString() method

        ///<summary>
        ///</summary>
        int Count
        {
            get;
        }

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        double this[int i]
        {
            get;
        }

        ///<summary>
        ///</summary>
        ///<param name="time"></param>
        ///<returns></returns>
        int FindIndex(double time);

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        double Dt(int i);
    }
}