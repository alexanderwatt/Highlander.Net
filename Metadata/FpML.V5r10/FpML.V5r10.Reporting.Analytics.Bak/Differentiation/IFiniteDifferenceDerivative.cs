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

namespace Orion.Analytics.Differentiation
{
    /// <summary>
    /// Interface class from which all classes that encapsulate functionality
    /// for computation of numerical derivatives by Finite Difference are 
    /// derived.
    /// All classes that perform Finite Difference are derived from 
    /// this interface class to allow polymorphic behaviour in the manner in 
    /// which the derivative is computed, for example Centered Finite Difference.
    /// Concrete derived classes must implement the ComputeFirstDerivative
    /// method.
    /// </summary>
    public interface IFiniteDifferenceDerivative
    {
        #region Interface Class Methods to be Implemented

        /// <summary>
        /// Encapsulates the methodology, for example Centred Finite Difference,
        /// to compute numerical first derivatives by Finite Differences.
        /// </summary>
        /// <param name="index">Zero based index in the array of x and y
        /// values at which the derivative is required.</param>
        /// <returns>Numerical first derivative.</returns>
        decimal ComputeFirstDerivative(int index);

        #endregion Interface Class Methods to be Implemented
    }
}