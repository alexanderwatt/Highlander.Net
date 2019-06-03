namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Summary description for MCG.
    /// </summary>
    public class MCG : BasicRng
    {
        private readonly long _m;
        private readonly long _a;
        private long _current;
        private readonly double _norm;

        ///<summary>
        ///</summary>
        ///<param name="seed"></param>
        public MCG(int seed) : this(1132489760, 2147483647, seed)
        {}

        protected MCG(int a, int m, int seed)
        {
            _a = a;
            _m = m;
            _norm = 1.0/(m+1.0);

            if (seed == 0)
                seed = 1;	/* default seed is 1 */
			
            _current = seed % m;
        }

        uint NextUInt32()
        {
            _current = _a*_current % _m;
            return (uint)_current;
        }

        /// <summary>
        /// Draw a random sample for the generator.
        /// </summary>
        /// <returns>
        /// A double-precision floating point number greater than or equal to 0.0, 
        /// and less than 1.0.
        /// </returns>
        protected override double Sample()
        {
            return NextUInt32() * _norm;
        }
    }
}