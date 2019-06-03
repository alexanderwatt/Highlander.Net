using System.Collections.Generic;

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Helper class for arrays
    /// </summary>
    static public class ArrayHelper
    {
        /// <summary>
        /// Converts a List to a typed array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        static public T[] ListToArray<T>(List<T> list)
        {
            T[] array = { };
            if (list.Count > 0)
            {
                array = new T[list.Count];
                list.CopyTo(array);
            }
            return array;
        }

        /// <summary>
        /// Converts an array to a typed List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        static public List<T> ArrayToList<T>(T[] array)
        {
            List<T> list=null;
            if (array != null && array.Length > 0)
            {
                list = new List<T>(array);
            }
            return list;
        }
    }
}
