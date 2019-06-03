#region Using Directives

using System;
using System.Collections;

using Orion.ModelFramework;

#endregion

namespace Orion.Analytics.Interpolations.Points
{
    /// <summary>
    /// The interface <c>IPoint</c> is used in 
    /// mathematical functions applied to curves and surfaces.
    /// <seealso cref="IPoint.Coords"/>
    /// This returns the dimension value for that point.
    /// In financial points we are interested mostly in curves and surface used for financial valuation.
    /// Unlike mathematical points, financial points at a minimum contain an x-coordinate and a function value.
    /// For ease of use, this is labelled as a one-dimensional point.
    /// </summary>
    public abstract class Point : IPoint
    {
        /// <summary>
        /// The private readonly <c>double</c> array that defines the point.
        /// </summary>
        protected double[] _pointArray;


        /// <summary>
        /// The ctor for the two dimensional point.
        /// </summary>
        /// <param name="array"><c>double</c> The array.</param>
        protected Point(double[] array)
        {
            _pointArray = array;
        }

        ///// <summary>
        ///// Converts a HL point into an Extreme point.
        ///// </summary>
        ///// <returns></returns>
        //public Extreme.Mathematics.Curves.Point GetExtremePoint()
        //{
        //    var point = new Extreme.Mathematics.Curves.Point(GetX(), FunctionValue);
        //    return point;
        //}

        /// <summary>
        /// Sets the point array.
        /// </summary>
        /// <returns><c>double</c> point array.</returns>
        public double[] Pointarray
        {
            get { return _pointArray; }
            set { _pointArray = value; }
        }

        /// <summary>
        /// Get/Set the value at the point.
        /// </summary>
        public double FunctionValue
        {
            get { return _pointArray[_pointArray.Length - 1]; }
            set { _pointArray[_pointArray.Length - 1] = value; }
        }

        /// <summary>
        /// This returns the coordinate values for a point.
        /// <seealso cref="IPoint"/>The interface <c>IPoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// <seealso cref="Orion.Analytics"/>A function is applied to a collection of points.
        /// </summary>
        /// <returns><c>double[]</c> the point coordinate values.</returns>       
        /// <remarks>The interface method <c>point.GetDimensionValues()</c>
        /// can handle a multi-dimensional point, but with a minimum of one dimension.</remarks>
        public virtual IList Coords
        {
            get
            {
                return _pointArray;
            }
        }

        /// <summary>
        /// Gets the x co-ordinate of the 1D point.
        /// </summary>
        /// <returns></returns>
        public virtual double GetX()
        {
            return _pointArray[0]; 
        }

        /// <summary>
        /// this returns the numkber of dimensions of the point.
        /// <seealso cref="IPoint"/>the interface <c>ipoint</c> is used in 
        /// mathematical functions applied to curves and surfaces. 
        /// </summary>
        /// <returns><c>int</c> the number of <c>int</c> dimensions.</returns>
        public int GetNumDimensions()
        {
            return _pointArray.Length - 1;
        }

        /// <summary>
        ///                     Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        ///                     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This instance is less than <paramref name="obj" />. 
        ///                     Zero 
        ///                     This instance is equal to <paramref name="obj" />. 
        ///                     Greater than zero 
        ///                     This instance is greater than <paramref name="obj" />. 
        /// </returns>
        /// <param name="obj">
        ///                     An object to compare with this instance. 
        ///                 </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. 
        ///                 </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            if (GetType() != obj.GetType())
            {
                throw new Exception("Different objects that can not be compared.");
            }
            var y = (Point) obj;

            var numDimensions = GetNumDimensions();

            if (numDimensions != y.GetNumDimensions())
            {
                throw new NotImplementedException();
            }

            for (var i = 0; i < numDimensions; i++)
            {
                if ((double)Coords[i] < (double)y.Coords[i])
                {
                    return -1;
                }
 //               break;
            }
            return 1;
        }
    }
}
