using System.Collections;

namespace Orion.Analytics.Solvers
{
    /// <summary>
    /// Helper to apply a unary function to a list of values.
    /// </summary>
    /// <remarks>
    /// The unary function can either implement 
    /// <see cref="UnaryFunction"/> or is a delegate
    /// of type <see cref="IUnaryFunction"/>.
    /// </remarks>
    /// <seealso cref="UnaryFunction"/>
    /// <seealso cref="IUnaryFunction">UnaryFunction delegate</seealso>
    public static class Functional
    {
        /// <summary>
        /// Apply the given unary function to a list of values.
        /// </summary>
        /// <param name="f">
        /// The unary function implementing <see cref="IUnaryFunction"/>.
        /// </param>
        /// <param name="x">
        /// The given function is applied to all values in the given
        /// <see cref="IList"/>.
        /// </param>
        public static void Apply(IUnaryFunction f, IList x) 
        {
            for( int i=0; i< x.Count; i++)
                x[i] = f.Value((double)x[i]);
        }

        /// <summary>
        /// Apply the given unary function to a list of values.
        /// </summary>
        /// <param name="f">
        /// The <see cref="UnaryFunction"/> delegate applied to all
        /// values of x.
        /// </param>
        /// <param name="x">
        /// The given function is applied to all values in the given
        /// <see cref="IList"/>.
        /// </param>
        public static void Apply(UnaryFunction f, IList x) 
        {
            for( int i=0; i< x.Count; i++)
                x[i] = f((double)x[i]);
        }
    }
}