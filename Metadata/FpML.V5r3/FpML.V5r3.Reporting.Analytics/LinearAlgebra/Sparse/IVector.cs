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

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Basic vector interface</summary>
	public interface IVector
	{
		///<summary>
		///</summary>
		int Length { get; }
	}

	/// <summary> Data-access without index-access. Note that matrices storing data as vectors
	/// may implement this interface, despite its name.</summary>
	public interface IVectorAccess
	{
		/// <summary>Gets a vector containing all the data of the matrix or vector.
		/// No ordering may be assumed.</summary>
		double[] Data { get; }
	}

	/// <summary> Vector consisting of sub-vectors</summary>
	public interface IBlockAccessVector : IVector
	{
		/// <summary> Returns an array of the same size as this vector. Each element is zeroed.
		/// The purpose of this method is to avoid excessive memory allocation and
		/// de-allocation. Note that this is not thread safe.</summary>
		double[] Array { get; }

		///<summary>
		///</summary>
		int BlockCount { get; }

		/// <summary> Returns the vector in block i</summary>
		IVector GetBlock(int i);

		/// <summary> Returns the indices of block i</summary>
		int[] GetBlockIndices(int i);

		/// <summary> Sets block i to the given vector with indices</summary>
		void SetBlock(int i, int[] index, IVector x);
	}

	/// <summary> Vector with a sparse view.</summary>
	public interface ISparseAccessVector : ISparse, IVector
	{
		/// <summary>Gets a sparse view of the vector. The vector backs the view directly,
		/// so changes in it changes the vector.</summary>
		IntDoubleVectorPair Vector { get; set; }
	}

	/// <summary> Vector with elemental access operations. The block-wise operations will
	/// typically be faster than the elementwise operations, and the extractions
	/// methods present here are not likely to be as fast as the custom accessors
	/// the vector may implement.</summary>
	public interface IElementalAccessVector : IVector
	{
		/// <summary> x(index) += value</summary>
		void AddValue(int index, double val);

		/// <summary> x(index) = value</summary>
		void SetValue(int index, double val);

		/// <summary> Returns x(index)</summary>
		double GetValue(int index);

		/// <summary> x(index) += values, blockwise</summary>
		void AddValues(int[] index, double[] values);

		/// <summary> x(index) = values, blockwise</summary>
		void SetValues(int[] index, double[] values);

		/// <summary> Returns the block x(index)</summary>
		double[] GetValues(int[] index);
	}

	/// <summary> Vector with fast array access.</summary>
	public interface IDenseAccessVector : IDense, IVector
	{
		/// <summary> Gets or sets a dense array representation of the vector. </summary>
		/// <remarks>The array is backed directly by the vector (changing it changes 
		/// the vector). Also, when set, no deep copy is performed.</remarks>
		double[] Vector { get; set; }
	}
}