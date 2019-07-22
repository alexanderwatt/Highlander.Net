namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Useful discretized discount bond asset.
    /// </summary>
    public class DiscretizedDiscountBond : 
        DiscretizedAsset
    {
        ///<summary>
        ///</summary>
        ///<param name="method"></param>
        public DiscretizedDiscountBond(INumericalMethod method)
            : base(method) 
        {}

        public override void Reset(int size) 
        {
            Values = new double[size];
            for (int i = 0; i < Values.Length; i++)
                Values[i] = 1.0;
        }
    }
}