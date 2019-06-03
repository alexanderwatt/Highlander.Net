

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Interface for generic argument groups.
    /// </summary>
    public interface IArguments 
    {
        ///<summary>
        ///</summary>
        ///<returns></returns>
        string ToString();
        ///<summary>
        ///</summary>
        void Validate();
    };
}