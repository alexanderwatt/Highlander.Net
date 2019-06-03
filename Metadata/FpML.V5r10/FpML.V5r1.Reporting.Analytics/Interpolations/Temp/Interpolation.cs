#region Using directives

using System;
using System.Collections;

#endregion

//TODO: make the Interpolation method classes into generec classes 
//(That will improve performance and code readability)

namespace nabCap.QR.Numerics2.Interpolations
{
	/// <summary>
	/// Abstract base class for 1-D interpolations.
	/// </summary>
	/// <remarks>
	/// Classes derived from this class will override <see cref="Value"/> to
	/// provide interpolated values from two sequences of equal length,
	/// representing discretized values of a variable and a function of
	/// the former, respectively.
	/// </remarks>
	public abstract class Interpolation 
	{

		/// <summary>
		/// Default constructor for this class.
		/// </summary>
		/// <overloads>
		/// Initialize a new Interpolation.
		/// </overloads>
		/// <remarks>
		/// You need to <see cref="Initialize"/> a class constructed by the
		/// default constructor.
		/// </remarks>
		protected Interpolation()
		{}

		/// <summary>
		/// Initialize a new Interpolation.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x must have been sorted.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists do not have the same size
		/// or do not provide enough points to interpolate.
		/// </exception>
		public Interpolation(IList x, IList y)
		{
			Initialize(x,y);
		}

		/// <summary>
		/// Initialize a class constructed by the default constructor.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x must have been sorted.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists do not have the same size
		/// or do not provide enough points to interpolate.
		/// </exception>
		public virtual void Initialize( IList x, IList y)
		{
			if( x.Count < 2)
				throw new ArgumentException("TODO: not enough points to interpolate");
			if( x.Count != y.Count )
				throw new ArgumentException("TODO: lists must be of equivalent size.");

			_x = new double[x.Count];
			x.CopyTo(_x,0);
		}

		protected double[] _x = null;

		/// <summary>
		/// Interpolated value.
		/// </summary>
		/// <remarks>
		/// This method must be overridden to provide an implementation
		/// of the actual interpolation.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="allowExtrapolation"></param>
		/// <returns>The interpolated value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
		public abstract double Value( double x, bool allowExtrapolation );


		/// <summary>
		/// Locate index to be used for interpolation.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="allowExtrapolation"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
		protected int Locate(double x, bool allowExtrapolation)
		{
			// check late initialization
			if( _x == null) 
				throw new InvalidOperationException( "Interpolation has not been initialized.");

			if (x < _x[0]) 
			{
				if( allowExtrapolation ) 
					return 0;
				else
					throw new ArgumentOutOfRangeException("x", "Value outside range and extrapolation disabled.");

			} 
			else if (x > _x[_x.Length-1]) 
			{
				if( allowExtrapolation ) 
					return _x.Length - 2;
				else
					throw new ArgumentOutOfRangeException("x", "Value outside range and extrapolation disabled.");
			} 
			else 
			{
				int pos = Array.BinarySearch(_x, 0, _x.Length-1, x + System.Double.Epsilon);
				if( pos < 0) pos = ~pos; // next larger object}
				return pos - 1;
			}
		}



	}
}
