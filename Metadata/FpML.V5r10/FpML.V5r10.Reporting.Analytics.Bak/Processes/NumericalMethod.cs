
// COM interop attributes
// some useful attributes

namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Numerical method (Tree, Finite Differences) abstract base class.
    /// </summary>
    public abstract class NumericalMethod : INumericalMethod
    {
        ///<summary>
        ///</summary>
        ///<param name="timeGrid"></param>
        protected NumericalMethod(ITimeGrid timeGrid) 
        {
            this.TimeGrid = timeGrid;
        }

        public ITimeGrid TimeGrid { get; }

        /// <summary>
        /// Initialize a DiscretizedAsset object.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="time"></param>
        public abstract void Initialize(IDiscretizedAsset asset, double time);

        /// <summary>
        /// Roll-back a DiscretizedAsset object until a certain time.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="toTime"></param>
        public abstract void Rollback(IDiscretizedAsset asset, double toTime);

    }
}