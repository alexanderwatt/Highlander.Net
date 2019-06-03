#region Using directives

using System;
using System.Collections;

#endregion

namespace nabCap.QR.Numerics2.Interpolations
{
	/// <summary>
	/// Log linear interpolation between discrete points.
	/// </summary>
	public class LogLinearInterpolation : LinearInterpolation
	{
		/// <summary>
		/// Default constructor for this class.
		/// </summary>
		/// <overloads>
		/// Initialize a new log linear interpolation.
		/// </overloads>
		/// <remarks>
		/// You need to <see cref="Initialize"/> a class constructed by the
		/// default constructor.
		/// </remarks>
		public LogLinearInterpolation()
		{}

		/// <summary>
		/// Initialize a new log linear interpolation.
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
		public LogLinearInterpolation(IList x, IList y) : base(x, y)
		{
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
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when y contains negative or 0.0 values.
		/// </exception>
		override public void Initialize( IList x, IList y)
		{
			double[] logY = new double[y.Count];
			for( int i=0; i<logY.Length; i++)
			{
				double yy = (double) y[i];
				if( yy <= 0.0 )
					throw new ArgumentOutOfRangeException("y", "TODO: negative or 0.0 values not allowed");
				logY[i] = Math.Log(yy);
			}
			base.Initialize(x, logY);
		}

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
			return Math.Exp( base.Value(x, allowExtrapolation) );
		}

	}
}
