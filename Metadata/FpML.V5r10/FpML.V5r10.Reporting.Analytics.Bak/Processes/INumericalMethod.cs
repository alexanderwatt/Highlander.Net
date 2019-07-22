namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Numerical method (Tree, Finite Differences) interface.
    /// </summary>
    public interface INumericalMethod
    {
        // TODO: Add ToString() method

        ///<summary>
        ///</summary>
        ITimeGrid TimeGrid
        { 
            get; 
        }

        /// <summary>
        /// Initialize a DiscretizedAsset object.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="time"></param>
        void Initialize(IDiscretizedAsset asset, double time);

        /// <summary>
        /// Roll-back a DiscretizedAsset object until a certain time.
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="toTime"></param>
        void Rollback(IDiscretizedAsset asset, double toTime);
    }
}