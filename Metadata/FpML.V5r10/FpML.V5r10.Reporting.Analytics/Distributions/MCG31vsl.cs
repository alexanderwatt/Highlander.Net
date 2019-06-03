namespace Orion.Analytics.Distributions
{
    /// <summary>
    /// Summary description for MCG_MKL.
    /// </summary>
    public sealed class MCG31vsl : MCG
    {
        ///<summary>
        ///</summary>
        ///<param name="seed"></param>
        public MCG31vsl(int seed) : base(1132489760, 2147483647, seed)
        {}

        ///<summary>
        ///</summary>
        public MCG31vsl() : this(1)
        {}
    }
}