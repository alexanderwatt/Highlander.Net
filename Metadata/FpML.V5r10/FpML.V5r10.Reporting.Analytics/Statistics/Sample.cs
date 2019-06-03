

namespace Orion.Analytics.Statistics
{
    /// <summary>
    /// Weighted sample.
    /// </summary>
    public struct Sample 
    {
        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        public Sample(object value) : this(value, 1.0)
        {}
        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<param name="weight"></param>
        public Sample(object value, double weight)
        {
            Value = value;
            Weight = weight;
        }
        public double Weight;
        public object Value;
        ///<summary>
        ///</summary>
        ///<param name="sample"></param>
        ///<returns></returns>
        public static explicit operator double(Sample sample)
        {
            return (double)sample.Value;
        }
    }

    //! weighted sample
    /*! \ingroup mcarlo */
    public class Sample<T>
    {
        public T Value { get; set; }

        public double Weight { get; set; }

        public Sample(T value, double weight)
        {
            Value = value;
            Weight = weight;
        }
    }
}