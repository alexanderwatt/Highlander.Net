
namespace Orion.Analytics.Distributions
{
    ///<summary>
    ///</summary>
    public interface IContinuousProbabilityDistribution
    {
        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        double ProbabilityDensity(double x);
        ///<summary>
        ///</summary>
        ///<param name="x"></param>
        ///<returns></returns>
        double CumulativeDistribution(double x);
        ///<summary>
        ///</summary>
        double Mean { get;}
        ///<summary>
        ///</summary>
        double Variance { get;}
        ///<summary>
        ///</summary>
        double Median { get;}
        ///<summary>
        ///</summary>
        double Minimum { get;}
        ///<summary>
        ///</summary>
        double Maximum { get;}
        ///<summary>
        ///</summary>
        double Skewness { get;}
    }
}