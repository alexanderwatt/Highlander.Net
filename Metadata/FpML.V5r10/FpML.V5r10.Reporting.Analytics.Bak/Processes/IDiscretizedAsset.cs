
#region Using directives

using System.Collections;

#endregion

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Discretized asset interface used by numerical methods.
    /// </summary>
    /// <remarks>
    /// This interface is a representation of the price of a
    /// derivative at a specific time. It is roughly an array of values, 
    /// each value being associated to a state of the underlying stochastic
    /// variables. For the moment, it is only used when working with trees, 
    /// but it should be quite easy to make a use of it in finite-differences
    /// methods. 
    /// </remarks>
    public interface IDiscretizedAsset
    {
        ///<summary>
        ///</summary>
        double Time
        {
            get; set; 
        }

        ///<summary>
        ///</summary>
        double[] Values 
        {
            get; set;
        }

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        double this[int i]
        { 
            get;
        }

        ///<summary>
        ///</summary>
        INumericalMethod Method
        { 
            get;
        }

        ///<summary>
        ///</summary>
        ///<param name="size"></param>
        void Reset(int size);

        ///<summary>
        ///</summary>
        void AdjustValues();

        ///<summary>
        ///</summary>
        ///<param name="times"></param>
        void AddTimes(/*std::list<Time>&*/ IList times);
	
    }
}