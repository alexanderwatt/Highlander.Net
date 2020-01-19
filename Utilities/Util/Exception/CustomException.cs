/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Utilities.Exception
{
    #region CustomExceptions
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class DateToSmallException : System.Exception
    {   //This class implements all of the base class constructors
        //the purpose of this custom class is to facilitate accurate code testing
        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        public DateToSmallException()
            : base("The interpolation date is smaller than zeroDates[].")
        {
        }

        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        /// <param name="message"></param>
        public DateToSmallException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DateToSmallException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DateToSmallException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class DateToLargeException : System.Exception
    {   //This class implements all of the base class constructors
        //the purpose of this custom class is to facilitate accurate code testing
        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        public DateToLargeException()
            : base("The interpolation date is larger than zeroDates[].")
        {
        }

        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        /// <param name="message"></param>
        public DateToLargeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// The interpolation date is smaller than zeroDates[].
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public DateToLargeException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public DateToLargeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// UnsortedDatesException
    /// </summary>
    [Serializable]
    public class UnsortedDatesException : System.Exception
    {   //This class implements all of the base class constructors
        //the purpose of this custom class is to facilitate accurate code testing
        /// <summary>
        /// UnsortedDatesException
        /// </summary>
        public UnsortedDatesException()
            : base("Dates are Unsorted. Dates need to be in ascending order.")
        {
        }

        /// <summary>
        /// UnsortedDatesException
        /// </summary>
        /// <param name="message"></param>
        public UnsortedDatesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// UnsortedDatesException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public UnsortedDatesException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// UnsortedDatesException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public UnsortedDatesException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    /// <summary>
    /// UnequalArrayLengthsException
    /// </summary>
    public class UnequalArrayLengthsException : System.Exception
    {   //This class implements all of the base class constructors
        //the purpose of this custom class is to facilitate accurate code testing
        /// <summary>
        /// UnequalArrayLengthsException
        /// </summary>
        public UnequalArrayLengthsException()
            : base("Arrays of corresponding length were not passed where required.")
        {
        }

        /// <summary>
        /// UnequalArrayLengthsException
        /// </summary>
        /// <param name="message"></param>
        public UnequalArrayLengthsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// UnequalArrayLengthsException
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public UnequalArrayLengthsException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// UnequalArrayLengthsException
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public UnequalArrayLengthsException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }

    }

    #endregion
}
