using System;
using System.Collections;

namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// Concrete subclass of IEqualityComparer that implements the method Equals
    /// to compare two instances of the class TwoDimensionalKey.
    /// </summary>
    public class TwoDimensionalKeyEqualityComparer: IEqualityComparer
    {
        #region Base Class Methods Equals and GetHashCode

        /// <summary>
        /// Implementation of the Equals method for the class TwoDimensionalKey.
        /// The purpose of the method is to provide the functionality to compare
        /// two instances of the class TwoDimensionalKey for equality.
        /// Two instances of the class TwoDimensionalKey are equal if their first
        /// key parts are equal and if their second key parts are equal.
        /// </summary>
        /// <param name="key1">First key in the comparison.</param>
        /// <param name="key2">Second key in the comparison.</param>
        /// <returns>True if the two keys are equal, otherwiese false.</returns>
        bool IEqualityComparer.Equals(object key1, object key2)
        {
            if(key1 == null || key2 == null)
            {
                const string errorMessage = "Invalid(null) two dimensional key found";

                throw new ArgumentException(errorMessage);
            }

            // Check first key parts.
            var instrument1 = 
                ((TwoDimensionalKey) key1).FirstKeyPart;
            var instrument2 = 
                ((TwoDimensionalKey) key2).FirstKeyPart;

            var areFirstKeyPartsEqual = (instrument1 == instrument2);

            // Check second key parts.
            var currency1 =
                ((TwoDimensionalKey) key1).SecondKeyPart;
            var currency2 =
                ((TwoDimensionalKey)key2).SecondKeyPart;

            var areSecondKeyPartsEqual = (currency1 == currency2);

            return (areFirstKeyPartsEqual && areSecondKeyPartsEqual);
        }


        /// <summary>
        /// Returns a hash code for the specified object.
        /// Base class implementation is used, as the subclass provides no 
        /// specific implementation.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"></see> for which 
        ///  a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        /// <exception cref="T:System.ArgumentNullException">The type of obj is
        ///  a reference type and obj is null.</exception>
        public int GetHashCode(object obj)
        {
            return GetHashCode(obj);
        }

        #endregion Base Class Methods Equals and GetHashCode
    }
}