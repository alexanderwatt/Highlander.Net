#region Using directives

using System;

#endregion

namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Gaussian random number generator.
    /// It uses the well-known Box-Muller transformation to return a
    /// normal distributed Gaussian deviate from a uniform deviate in (0,1).
    /// </summary>
    public sealed class BoxMullerGaussianRng : CachedContinousRng
    {

        /// <summary>
        /// Initialize a new BoxMullerGaussianRng.
        /// </summary>
        public BoxMullerGaussianRng( IBasicRng basicRng, double mean, double sigma)
        {
            _basicRng = basicRng;
            _mean = mean;
            _sigma = sigma;
        }

        private readonly IBasicRng _basicRng;
        private readonly double _mean;
        private readonly double _sigma;

        /// <summary>
        /// Draw random samples from the generator.
        /// </summary>
        /// <param name="r">
        ///		An <see cref="Array"/> to be filled with double-precision floating point 
        ///		numbers.</param>
        /// <param name="start">
        ///		The (zero-based) index of the first array element to fill.
        /// </param>
        /// <param name="length">
        ///		The number of random samples to generate.
        /// </param>
        protected override void Generate(double[] r, int start, int length)
        {
            // Box-Muller
            double x1,x2,rsq,ratio;
            var drand = new double[2];

            if( _mean == 0.0 && _sigma == 1.0 )
            {
                for( length+=start; start<length; )
                {
                    _basicRng.Next(drand);
                    x1 = 2.0 * drand[0] - 1.0;
                    x2 = 2.0 * drand[1] - 1.0;
                    rsq = x1*x1+x2*x2;
                    if( rsq>=1.0 || rsq==0.0 ) continue;
                    ratio = Math.Sqrt(-2.0*Math.Log(rsq)/rsq);
                    r[start++] = x1*ratio;
                    if( start >= length ) return;
                    r[start++] = x2*ratio;
                }
            } 
            else
            {
                for( length+=start; start<length; )
                {
                    _basicRng.Next(drand);
                    x1 = 2.0 * drand[0] - 1.0;
                    x2 = 2.0 * drand[1] - 1.0;
                    rsq = x1*x1+x2*x2;
                    if( rsq>=1.0 || rsq==0.0 ) continue;
                    ratio = Math.Sqrt(-2.0*Math.Log(rsq)/rsq);
                    // we could use VML later to transform to (_mean, _sigma)
                    r[start++] = _mean + x1*ratio * _sigma;
                    if( start >= length ) return;
                    r[start++] = _mean + x2*ratio * _sigma;
                }
            }
        }


    }
}