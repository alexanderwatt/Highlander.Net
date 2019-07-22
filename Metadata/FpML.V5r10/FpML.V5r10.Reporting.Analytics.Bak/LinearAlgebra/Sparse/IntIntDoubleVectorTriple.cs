
using System;

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Dual indices and data paired together. Used by some of the faster 
	/// matrices. major contains the index for each data-entry, while minor is either 
	/// uses as row- or column-delimiters. Within two minor-indices, the major-indices
	/// must be sorted for most applications.</summary>
	public class IntIntDoubleVectorTriple
	{
		/// <summary> Major indices, as long as data</summary>
		public int[] Major;

		/// <summary> Minor indices, shorter than data</summary>
		public int[] Minor;

		/// <summary> Double-precision data</summary>
		public double[] Data;

		/// <summary> Constructor for IntIntDoubleVectorTriple.</summary>
		public IntIntDoubleVectorTriple(int[] major, int[] minor, double[] data)
		{
			Major = major;
			Minor = minor;
			Data = data;
		}

		/// <summary> Deep copy of the object</summary>
		public virtual IntIntDoubleVectorTriple Clone()
		{
			int[] newMajor = new int[Major.Length], newMinor = new int[Minor.Length];
			double[] newData = new double[Data.Length];
			Array.Copy(Major, 0, newMajor, 0, Major.Length);
			Array.Copy(Minor, 0, newMinor, 0, Minor.Length);
			Array.Copy(Data, 0, newData, 0, Data.Length);
			return new IntIntDoubleVectorTriple(newMajor, newMinor, newData);
		}

		/// <summary> Copy of the index/data pair between minor[i] and minor[i+1]</summary>
		public virtual IntDoubleVectorPair Copy(int i)
		{
			int length = Minor[i + 1] - Minor[i];
			int[] index = new int[length];
			double[] newData = new double[length];
			Array.Copy(Major, Minor[i], index, 0, length);
			Array.Copy(Data, Minor[i], newData, 0, length);
			return new IntDoubleVectorPair(index, newData);
		}
	}
}