using System.Collections.Generic;
using Orion.Util.Helpers;

namespace HLV5r3.Helpers
{
    ///<summary>
    ///</summary>
    public static class RangeTermsHelper
    {
        /// <summary>
        /// Creates the array for item.
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="terms">The terms.</param>
        /// <returns></returns>
        public static TOut[] CreateArrayForItem<TIn, TOut>(string itemName, TIn[] terms)
        {
            var result = new List<TOut>();
            foreach (TIn streamTerm in terms)
            {
                if (ObjectLookupHelper.ObjectPropertyExists(streamTerm, itemName))
                {
                    result.Add((TOut)ObjectLookupHelper.GetPropertyValue(streamTerm, itemName));
                }
            }
            return result.ToArray();
        }
    }
}