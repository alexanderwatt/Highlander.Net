
namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Class that contains time discretisations in both expiry and tenor for a Pedersen calibration. All values are in quarters.
    /// </summary>
    public class PedersenTimeGrid : TimeGrid
    {
        #region Constructor + Initialisation
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expiryGrid">Array containing the expiry discretisation.</param>
        /// <param name="tenorGrid">Array containing the tenor discretisation.</param>
        public PedersenTimeGrid(int[] expiryGrid, int[] tenorGrid)
        {
            if (expiryGrid[expiryGrid.GetLowerBound(0)] == 0)
            {
                ExpiryGrid=expiryGrid;
            }
            else
            {
                ExpiryGrid = new int[expiryGrid.GetLength(0)+1];
                expiryGrid.CopyTo(ExpiryGrid, 1);
            }

            if (tenorGrid[tenorGrid.GetLowerBound(0)] == 0)
            {
                TenorGrid = tenorGrid;
            }
            else
            {
                TenorGrid = new int[tenorGrid.GetLength(0) + 1];
                tenorGrid.CopyTo(TenorGrid, 1);
            }

            ExpiryCount = ExpiryGrid.GetUpperBound(0);
            TenorCount = TenorGrid.GetUpperBound(0);

            MaxExpiry = ExpiryGrid[ExpiryCount];
            MaxTenor = TenorGrid[TenorCount];
        }
        #endregion

        #region Private Fields

        /// <summary>
        /// Array containing the expiry discretisation in quarters. The first entry is always 0.
        /// </summary>
        public int[] ExpiryGrid { get; }

        /// <summary>
        /// Array containing the tenor discretisation in quarters. The first entry is always 0.
        /// </summary>
        public int[] TenorGrid { get; }

        /// <summary>
        /// Maximum expiry used.
        /// </summary>
        public int MaxExpiry { get; }

        /// <summary>
        /// Maximum tenor used.
        /// </summary>
        public int MaxTenor { get; }

        /// <summary>
        /// Number of time buckets in the expiry discretisation.
        /// </summary>
        public int ExpiryCount { get; }

        /// <summary>
        /// Number of time buckets in the tenor discretisation.
        /// </summary>
        public int TenorCount { get; }

        #endregion

    }
}