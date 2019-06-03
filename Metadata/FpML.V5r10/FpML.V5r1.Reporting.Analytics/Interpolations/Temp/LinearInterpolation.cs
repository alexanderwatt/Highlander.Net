#region Using directives

using System;
using System.Collections;

#endregion

namespace nabCap.QR.Numerics2.Interpolations
{
	/// <summary>
	/// Linear interpolation between discrete points.
	/// </summary>
	public class LinearInterpolation : Interpolation
	{
		/// <summary>
		/// Default constructor for this class.
		/// </summary>
		/// <overloads>
		/// Initialize a new linear interpolation.
		/// </overloads>
		/// <remarks>
		/// You need to <see cref="Initialize"/> a class constructed by the
		/// default constructor.
		/// </remarks>
		public LinearInterpolation() 
		{}

		/// <summary>
		/// Initialize a new linear interpolation.
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
		public LinearInterpolation(IList x, IList y) : base(x, y)
		{
		}

		/// <summary>
		/// Initialize a class constructed by the default constructor.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x must have been sorted.
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists do not have the same size
		/// or do not provide enough points to interpolate.
		/// </exception>
		public override void Initialize( IList x, IList y)
		{
			base.Initialize(x, y);
			_y = y;
		}

		protected IList _y = null;

		/// <summary>
		/// Interpolated value.
		/// </summary>
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
		override public double Value( double x, bool allowExtrapolation )
		{
			int position = Locate(x, allowExtrapolation);
			double yl = (double)_y[position];
			double dy = (double)_y[position+1] - yl;
			double xl = _x[position];
			double dx = _x[position+1] - xl;
			return yl + ( x-xl ) * dy / dx;
		}

	}
}
