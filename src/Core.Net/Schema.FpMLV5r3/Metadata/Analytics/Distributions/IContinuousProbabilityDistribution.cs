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

namespace Highlander.Reporting.Analytics.V5r3.Distributions
{
    ///<summary>
    ///</summary>
    public interface IContinuousProbabilityDistribution
    {
        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        double ProbabilityDensity(double x);
        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        double CumulativeDistribution(double x);
        ///<summary>
        ///</summary>
        double Mean { get;}
        ///<summary>
        ///</summary>
        double Variance { get;}
        ///<summary>
        ///</summary>
        double Median { get;}
        ///<summary>
        ///</summary>
        double Minimum { get;}
        ///<summary>
        ///</summary>
        double Maximum { get;}
        ///<summary>
        ///</summary>
        double Skewness { get;}
    }
}