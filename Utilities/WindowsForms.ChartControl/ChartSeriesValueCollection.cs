#region References
using System;
using System.Collections;

#endregion

namespace Orion.WindowsForms.ChartControl
{
    /// <summary>
    /// Represents a collection of values from the time series of the chart control.
    /// </summary>
    public class ChartSeriesValueCollection : CollectionBase
    {
        #region Fields
        private bool isEmpty = true;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class ChartSeriesValueCollection.
        /// </summary>
        internal ChartSeriesValueCollection() : base()
        {
            // do nothing
        }

        #endregion

        #region Properties
        /// <summary>
        /// Determines if the collection has been initialized.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return this.isEmpty;
            }
        }

        #endregion

        #region Indexer
        /// <summary>
        /// Gets a specific item from the collection.
        /// </summary>
        public ChartSeriesValue this[int index]
        {
            get
            {
                return (ChartSeriesValue)base.InnerList[index];
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Adds an ChartSeriesValue to the end of the ChartSeriesValueCollection.
        /// </summary>
        /// <param name="seriesValue">The ChartSeriesValue to be added to the end of the ChartSeriesValueCollection. The value can be null.</param>
        public void Add(ChartSeriesValue seriesValue)
        {
            base.InnerList.Add(seriesValue);
            this.isEmpty = false;
        }

        /// <summary>
        /// Adds the elements of an ChartSeriesValue array to the end of the ChartSeriesValueCollection.
        /// </summary>
        /// <param name="seriesValues">The ChartSeriesValue array whose elements should be added to the end of the ChartSeriesValueCollection. The array itself cannot be null, but it can contain elements that are null.</param>
        public void AddRange(ChartSeriesValue[] seriesValues)
        {
            base.InnerList.AddRange(seriesValues);
            this.isEmpty = false;
        }

        /// <summary>
        /// Removes the first occurrence of a specific ChartSeriesValue from the ChartSeriesValueCollection.  
        /// </summary>
        /// <param name="seriesValue">The ChartSeriesValue object to be removed from the ChartSeriesValueCollection. The value can be null.</param>
        public void Remove(ChartSeriesValue seriesValue)
        {
            base.InnerList.Remove(seriesValue);
        }

        /// <summary>
        /// Removes the element at the specified index of the ChartSeriesValueCollection.  
        /// </summary>
        /// <param name="index">The zero-based index of the element to be removed.</param>
        public void Remove(int index)
        {
            base.InnerList.RemoveAt(index);
        }

        /// <summary>
        /// Copies the entire collection to a one-dimensional array of ChartSeriesValue objects.
        /// </summary>
        /// <param name="seriesValueArray">An array of ChartSeriesValue objects.</param>
        public void CopyTo(ChartSeriesValue[] seriesValueArray)
        {
            this.InnerList.CopyTo(seriesValueArray);
        }

        /// <summary>
        /// Copies the entire collection to a one-dimensional array of ChartSeriesValue objects, starting at the specified index of the target array.
        /// </summary>
        /// <param name="seriesValueArray">An array of ChartSeriesValue objects.</param>
        /// <param name="arrayIndex">An integer value representing the index of the target array from wich to start copying.</param>
        public void CopyTo(ChartSeriesValue[] seriesValueArray, int arrayIndex)
        {
            this.InnerList.CopyTo(seriesValueArray, arrayIndex);
        }

        #endregion
    }
}