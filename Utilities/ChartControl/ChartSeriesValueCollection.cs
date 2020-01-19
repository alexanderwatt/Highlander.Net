#region References

using System.Collections;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// Represents a collection of values from the time series of the chart control.
    /// </summary>
    public class ChartSeriesValueCollection : CollectionBase
    {
        #region Fields

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartSeriesValueCollection.
        /// </summary>
        internal ChartSeriesValueCollection()
        {
            // do nothing
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines if the collection has been initialized.
        /// </summary>
        public bool IsEmpty { get; private set; } = true;

        #endregion

        #region Indexer
        /// <summary>
        /// Gets a specific item from the collection.
        /// </summary>
        public ChartSeriesValue this[int index] => (ChartSeriesValue)InnerList[index];

        #endregion

        #region Methods
        /// <summary>
        /// Adds an ChartSeriesValue to the end of the ChartSeriesValueCollection.
        /// </summary>
        /// <param name="seriesValue">The ChartSeriesValue to be added to the end of the ChartSeriesValueCollection. The value can be null.</param>
        public void Add(ChartSeriesValue seriesValue)
        {
            InnerList.Add(seriesValue);
            IsEmpty = false;
        }

        /// <summary>
        /// Adds the elements of an ChartSeriesValue array to the end of the ChartSeriesValueCollection.
        /// </summary>
        /// <param name="seriesValues">The ChartSeriesValue array whose elements should be added to the end of the ChartSeriesValueCollection. The array itself cannot be null, but it can contain elements that are null.</param>
        public void AddRange(ChartSeriesValue[] seriesValues)
        {
            InnerList.AddRange(seriesValues);
            IsEmpty = false;
        }

        /// <summary>
        /// Removes the first occurrence of a specific ChartSeriesValue from the ChartSeriesValueCollection.  
        /// </summary>
        /// <param name="seriesValue">The ChartSeriesValue object to be removed from the ChartSeriesValueCollection. The value can be null.</param>
        public void Remove(ChartSeriesValue seriesValue)
        {
            InnerList.Remove(seriesValue);
        }

        /// <summary>
        /// Removes the element at the specified index of the ChartSeriesValueCollection.  
        /// </summary>
        /// <param name="index">The zero-based index of the element to be removed.</param>
        public void Remove(int index)
        {
            InnerList.RemoveAt(index);
        }

        /// <summary>
        /// Copies the entire collection to a one-dimensional array of ChartSeriesValue objects.
        /// </summary>
        /// <param name="seriesValueArray">An array of ChartSeriesValue objects.</param>
        public void CopyTo(ChartSeriesValue[] seriesValueArray)
        {
            InnerList.CopyTo(seriesValueArray);
        }

        /// <summary>
        /// Copies the entire collection to a one-dimensional array of ChartSeriesValue objects, starting at the specified index of the target array.
        /// </summary>
        /// <param name="seriesValueArray">An array of ChartSeriesValue objects.</param>
        /// <param name="arrayIndex">An integer value representing the index of the target array from which to start copying.</param>
        public void CopyTo(ChartSeriesValue[] seriesValueArray, int arrayIndex)
        {
            InnerList.CopyTo(seriesValueArray, arrayIndex);
        }

        #endregion
    }
}