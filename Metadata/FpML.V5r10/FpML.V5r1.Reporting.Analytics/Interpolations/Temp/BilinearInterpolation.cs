#region Using directives

using System;
using System.Collections;

using nabCap.QR.Numerics2.LinearAlgebra;

#endregion

namespace nabCap.QR.Numerics2.Interpolations
{
	/// <summary>
	/// Bilinear interpolation between discrete points.
	/// </summary>
	public class BilinearInterpolation : Interpolation2D
	{
        /// <summary>
        /// Default constructor for this class.
        /// </summary>
        /// <overloads>
        /// Initialize a new bilinear interpolation.
        /// </overloads>
        /// <remarks>
        /// You need to <see cref="Initialize"/> a class constructed by the
        /// default constructor.
        /// </remarks>
        public BilinearInterpolation()
        {
        }

	    /// <summary>
		/// Initialize a new bilinear interpolation.
		/// </summary>
		/// <remarks>
		/// The sequence of values for x and y must have been sorted.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <exception cref="ArgumentException">
		/// Thrown when the passed lists and matrix do not have the same size
		/// or do not provide enough points to interpolate.
		/// </exception>
		public BilinearInterpolation(IList x, IList y, Matrix z) : base(x, y, z)
		{}

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
		public override void Initialize( IList x, IList y, Matrix z)
		{
			base.Initialize(x, y, z);
			_z = z;
		}

		private Matrix _z = null;


		/// <summary>
		/// Interpolated value.
		/// </summary>
		/// <remarks>
		/// This method must be overridden to provide an implementation
		/// of the actual interpolation.
		/// </remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="allowExtrapolation"></param>
		/// <returns>The interpolated value.</returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Thrown when extrapolation has not been allowed and the passed value
		/// is outside the allowed range.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Thrown when the class was not properly initialized.
		/// </exception>
		public override double Value( double x, double y, bool allowExtrapolation )
		{
			int[] xy = Locate(x, y, allowExtrapolation);
            int ix = xy[0];
            int iy = xy[1];

			double z1 = _z[ iy  , ix];
			double z2 = _z[ iy  , ix+1];
			double z3 = _z[ iy+1, ix];
			double z4 = _z[ iy+1, ix+1];

			double t=( x - _x[ix] ) / ( _x[ix+1] - _x[ix] );
			double u=( y - _y[iy] ) / ( _y[iy+1] - _y[iy] );

			return (1.0-t) * (1.0-u) * z1+
			          t    * (1.0-u) * z2+
			       (1.0-t) *    u    * z3+
			          t    *    u    * z4;
		}

	}
}
