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

namespace Highlander.Numerics.LinearAlgebra.Sparse
{
	/// <summary> Matrix with external storage and custom matrix/vector product.</summary>
	public interface IShellMatrix : IMatrix
	{
		/// <summary> <c>z = alpha*A*x + beta*y</c>. x can not be the same as y or z.</summary>
		/// <returns> z </returns>
		IVector MultAdd(double alpha, IVector x, double beta, IVector y, IVector z);

		/// <summary>
		/// <c>z = alpha*A<sup>T</sup>*x + beta*y</c>. x can not be the same as y or z.
		/// </summary>
		/// <returns> z </returns>
		IVector TransMultAdd(double alpha, IVector x, double beta, IVector y, IVector z);
	}
}