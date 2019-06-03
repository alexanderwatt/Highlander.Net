#region Using Directives

using System;

#endregion

namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// Class that encapsulates functionality to validate data against some
    /// criteria, for example that a particular value is positive.
    /// Anticipated use of the methods offered by this class are to validate
    /// arguments of functions, for example class constructor arguments.
    /// </summary>
    public static class DataQualityValidator
    {
        #region Method to Validate a Minimum Numerical Value 

        /// <summary>
        /// Validates that a numerical value is greater than or equal to some
        /// designated number.
        /// </summary>
        /// <param name="valueToValidate">The value to validate.</param>
        /// <param name="minimum">Minimum (lower bound) that must be
        /// attained by the value to validate.</param>
        /// <param name="errorMessage">Error message to throw in the event
        /// that the validation fails and the client has selected to
        /// throw an exception.</param>
        /// <param name="throwException">If set to <c>true</c> the function
        /// will throw an exception with the user defined error message
        /// when the validation fails.</param>
        /// <returns>True if the test is successful (minimum is satisfied)
        /// otherwise false.</returns>
        public static bool ValidateMinimum(decimal valueToValidate,
                                           decimal minimum,
                                           string errorMessage,
                                           bool throwException)
        {
            var isMinimumSatisfied =
                valueToValidate.CompareTo(minimum) >=0;
            if (throwException && !isMinimumSatisfied)
            {
                // Validation failed.
                // Client has selected option to throw an error.
                throw new ArgumentException(errorMessage);
            }
            return isMinimumSatisfied;
        }

        /// <summary>
        /// Validates that a numerical value is greater than or equal to some
        /// designated number.
        /// </summary>
        /// <param name="valueToValidate">The value to validate.</param>
        /// <param name="minimum">Minimum (lower bound) that must be
        /// attained by the value to validate.</param>
        /// <param name="errorMessage">Error message to throw in the event
        /// that the validation fails and the client has selected to
        /// throw an exception.</param>
        /// <param name="throwException">If set to <c>true</c> the function
        /// will throw an exception with the user defined error message
        /// when the validation fails.</param>
        /// <returns>True if the test is successful (minimum is satisfied)
        /// otherwise false.</returns>
        public static bool ValidateMinimum(double valueToValidate,
                                           double minimum,
                                           string errorMessage,
                                           bool throwException)
        {
            var isMinimumSatisfied =
                valueToValidate.CompareTo(minimum) >= 0;
            if (throwException && !isMinimumSatisfied)
            {
                // Validation failed.
                // Client has selected option to throw an error.
                throw new ArgumentException(errorMessage);
            }
            return isMinimumSatisfied;
        }

        #endregion 

        #region Method to Validate that a Numerical Value is Positive

        /// <summary>
        /// Validates that a numerical value is positive.
        /// </summary>
        /// <param name="valueToValidate">The value to validate.</param>
        /// <param name="errorMessage">Error message to throw in the event
        /// that the validation fails and the client has selected to
        /// throw an exception.</param>
        /// <param name="throwException">If set to <c>true</c> the function
        /// will throw an exception with the user defined error message
        /// when the validation fails.</param>
        /// <returns>
        /// True if the test is successful (value is positive) otherwise
        /// false.</returns>
        public static bool ValidatePositive(decimal valueToValidate,
                                            string errorMessage,
                                            bool throwException)
        {
            var isPositive =
                valueToValidate.CompareTo(0m) > 0;

            if (throwException && !isPositive)
            {
                // Validation failed.
                // Client has selected option to throw an error.
                throw new ArgumentException(errorMessage);
            }

            return isPositive;
        }

        /// <summary>
        /// Validates that a numerical value is positive.
        /// </summary>
        /// <param name="valueToValidate">The value to validate.</param>
        /// <param name="errorMessage">Error message to throw in the event
        /// that the validation fails and the client has selected to
        /// throw an exception.</param>
        /// <param name="throwException">If set to <c>true</c> the function
        /// will throw an exception with the user defined error message
        /// when the validation fails.</param>
        /// <returns>
        /// True if the test is successful (value is positive) otherwise
        /// false.</returns>
        public static bool ValidatePositive(double valueToValidate,
                                            string errorMessage,
                                            bool throwException)
        {
            var isPositive =
                valueToValidate.CompareTo(0d) > 0;

            if (throwException && !isPositive)
            {
                // Validation failed.
                // Client has selected option to throw an error.
                throw new ArgumentException(errorMessage);
            }

            return isPositive;
        }

        #endregion

        #region Method to Validate that a String is Not Empty

        /// <summary>
        /// Validates that a string is not empty.
        /// </summary>
        /// <param name="stringToValidate">The string to validate.</param>
        /// <param name="errorMessage">Error message to throw in the event
        /// that the validation fails and the client has selected to
        /// throw an exception.</param>
        /// <param name="throwException">If set to <c>true</c> the function
        /// will throw an exception with the user defined error message
        /// when the validation fails.</param>
        /// <returns>True if the test is successful (string is not empty)
        /// otherwise false.</returns>
        public static bool ValidateNonEmptyString(string stringToValidate,
                                                  string errorMessage,
                                                  bool throwException)
        {
            var isEmptyString = stringToValidate.Length == 0;

            if (throwException && isEmptyString)
            {
                // Validation failed.
                // Client has selected option to throw an error.
                throw new ArgumentException(errorMessage);
            }

            return !isEmptyString;
        }

        #endregion
    }
}