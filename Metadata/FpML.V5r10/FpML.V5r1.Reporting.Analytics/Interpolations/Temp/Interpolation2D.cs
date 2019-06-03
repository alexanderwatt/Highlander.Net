#region Using directives

using System;
using System.Collections;

using nabCap.QR.Numerics2.LinearAlgebra;

#endregion

namespace nabCap.QR.Numerics2.Interpolations
{
	/// <summary>
	/// Abstract base class for 2-D interpolations.
	/// </summary>
	/// <remarks>
	/// Classes derived from this class will override <see cref="Value"/> to
	/// provide interpolated values from two sequences of length N and M,
	/// representing the discretized values of the x,y variables,
	/// and a NxM matrix representing the function tabulated z values.
	/// </remarks>
	public abstract class Interpolation2D 
	{

		/// <summary>
		/// Default constructor for this class.
		/// </summary>
		/// <overloads>
		/// Initialize a new 2-D Interpolation.
		/// </overloads>
		/// <remarks>
		/// You need to <see cref="Initialize"/> a class constructed by the
		/// default constructor.
		/// </remarks>
		protected Interpolation2D()
		{}

		/// <summary>
		/// Initialize a new Interpolation.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x and y must have been sorted.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists do not have the same size
		/// or do not provide enough points to interpolate.
		/// </exception>
		public Interpolation2D(IList x, IList y, Matrix z)
		{
			Initialize(x, y, z);
		}

		/// <summary>
		/// Initialize a class constructed by the default constructor.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x and y must have been sorted.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists and matrix do not have the
		/// same size or do not provide enough points to interpolate.
		/// </exception>
		public virtual void Initialize( IList x, IList y, Matrix z)
		{
			if( x.Count < 2 || y.Count < 2)
				throw new ArgumentException("TODO: not enough points to interpolate");
			if( x.Count != z.ColumnCount || y.Count != z.RowCount)
				throw new ArgumentException("TODO: lists and matrix must be of equivalent sizes.");

			_x = new double[x.Count];
			x.CopyTo(_x,0);
			_y = new double[y.Count];
			y.CopyTo(_y,0);
		}


		/// <summary>
		/// Interpolated value.
		/// </summary>
		/// <remarks>
		/// This method must be overridden to provide an implementation
		/// of the actual interpolation.
		/// </remarks>
		/// <returns>The interpolated value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
		public abstract double Value( double x, double y, bool allowExtrapolation );

		protected double[] _x = null;
		protected double[] _y = null;

		/// <summary>
		/// Locate index to be used for interpolation.
		/// </summary>
		/// <returns>An array containing the locate x and y indices.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
		protected int[] Locate(double x, double y, bool allowExtrapolation)
		{
			// check late initialization
			if( _x == null) 
				throw new InvalidOperationException("TODO: Interpolation has not been initialized.");

			int[] xy = new int[2];
			xy[0] = Locate(_x, x, allowExtrapolation, "x");
			xy[1] = Locate(_y, y, allowExtrapolation, "y");
			return xy;
		}


		private static int Locate( double[] values, double value, bool allowExtrapolation, string name)
		{
			if (value < values[0]) 
			{
				if( allowExtrapolation ) 
					return 0;
				else
                    throw new ArgumentOutOfRangeException("value", "TODO: Value outside range and extrapolation disabled.");

			} 
			else if (value > values[values.Length-1]) 
			{
				if( allowExtrapolation ) 
					return values.Length-2;
				else
					throw new ArgumentOutOfRangeException("value", "TODO: Value outside range and extrapolation disabled.");
			} 
			else 
			{
				int pos = Array.BinarySearch(values, 0, values.Length-1, 
					value + System.Double.Epsilon);
				
                if (0 == pos)
                {
                    //  Igor Sukhov 21.08.2007
                    //  exact match with the first element
                    //
                    return 0;
                }

                if( pos < 0) pos = ~pos; // next larger object

                return pos - 1;
			}
		}

	}
}
