using System.Collections.Generic;
using Orion.Analytics.Statistics;

namespace Orion.Analytics.Stochastics.MonteCarlo
{
    /// <summary>
    /// General purpose Monte Carlo model for path samples.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the general structure of a Monte Carlo 
    /// calculations, namely
    /// <list type="bullet">
    /// <item><description>
    /// the generation of a number of paths,
    /// </description></item>
    /// <item><description>
    /// the pricing of the derivative on each path and
    /// </description></item>
    /// <item><description>
    /// the averaging of the results to yield the actual derivative price.
    /// </description></item>
    /// </list>
    /// <para>
    /// As outlined above, the first two steps are delegated to a 
    /// <see cref="PathGenerator"/> and a <see cref="PathPricer"/>. 
    /// The third step is also delegated to an object which
    /// accumulates weighted values and returns the statistic properties of the
    /// set of such values. One such class is <see cref="RiskStatistics"/>
    /// provided by the <see cref="Orion.Analytics"/> assembly.
    /// </para>
    /// <para>
    /// The concern of the Monte Carlo model is therefore to act as a glue
    /// between such three components and can be expressed by the following
    /// pseudo-code:
    /// <code>
    /// given pathGenerator, pathPricer, accumulator;
    /// for i in number of samples {
    ///		path,weight = pathGenerator.next();
    ///		price = pathPricer(path);
    ///		accumulator.add(price,weight);
    ///	}
    ///	</code>
    ///	</para>
    ///	<para>
    ///	The Monte Carlo model also provides the user with the possibility to
    ///	take advantage of control-variate techniques.
    ///	Such techniques consist in pricing a portfolio from which the price of
    ///	the derivative can be deduced, but with a lower variance than the
    ///	derivative alone. 
    ///	</para>
    ///	<para>
    ///	In our current implementation, static-hedge control variate is used,
    ///	namely, the formed portfolio is long of the derivative we need to price
    ///	and short of a similar derivative whose price can be calculated
    ///	analytically. The value of the portfolio on a given path will of course
    ///	be given by the difference of the values of the two derivatives on such
    ///	path. However, due to the similarity between the derivatives, the
    ///	portfolio price will have a lower variance than either derivative alone
    ///	since any variation in the price of the latter will be partly compensated
    ///	by a similar variation in the price of the other.
    ///	Lastly, given the portfolio price, the price of the derivative we are
    ///	interested in can be deduced by adding the analytic value of the other.
    ///	</para>
    ///	<para>
    ///	In order to use such technique, the user must provide the model with a
    ///	path pricer for the additional option and the value of the latter.
    ///	The action of the Monte Carlo model is in this case expressed as
    ///	in the following pseudo-code:
    ///	<code>
    ///	given pathGenerator, pathPricer, cvPathPricer, cvPrice, accumulator;
    ///	for i in number of samples {
    ///		path,weight  = pathGenerator.next();
    ///		portfolioPrice = pathPricer(path) - cvPathPricer(path);
    ///		accumulator.add(portfolioPrice+cvPrice,weight);
    ///	}
    ///	</code>
    /// </para>
    /// </remarks>
    public class MonteCarloModel 
    {
        /// <overloads>
        /// Initialize a new MonteCarloModel.
        /// </overloads>
        /// <summary>
        /// Initialize a new MonteCarloModel without using control variate techniques
        /// and using the default <see cref="RiskStatistics"/> accumulator.
        /// </summary>
        /// <param name="pathGenerator">The <see cref="PathGenerator"/> generating single or multiple random walks.</param>
        /// <param name="pathPricer">The <see cref="PathPricer"/> calculating the option price on each sample.</param>
        public MonteCarloModel(PathGenerator pathGenerator,
                               PathPricer pathPricer)
            : this(pathGenerator, pathPricer, new RiskStatistics())
        {}

        /// <summary>
        /// Initialize a new MonteCarloModel without using control variate techniques.
        /// </summary>
        /// <param name="pathGenerator">The <see cref="PathGenerator"/> generating single or multiple random walks.</param>
        /// <param name="pathPricer">The <see cref="PathPricer"/> calculating the option price on each sample.</param>
        /// <param name="sampleAccumulator">A sample accumulator to record statistics.</param>
        public MonteCarloModel(PathGenerator pathGenerator,
                               PathPricer pathPricer, RiskStatistics sampleAccumulator)
            : this(pathGenerator, pathPricer, sampleAccumulator, null, 0.0)
        {}

        /// <summary>
        /// Initialize a new MonteCarloModel optionally using control variate techniques.
        /// </summary>
        /// <remarks>
        /// In order to use the control variate technique, the user should provide the
        /// additional control option, namely the <paramref name="cvPathPricer"/> and
        /// the <paramref name="cvOptionValue"/>.
        /// </remarks>
        /// <param name="pathGenerator">
        /// The <see cref="PathGenerator"/> generating single or multiple random walks.
        /// </param>
        /// <param name="pathPricer">
        /// The <see cref="PathPricer"/> calculating the option price on each sample.
        /// </param>
        /// <param name="sampleAccumulator">
        /// A sample accumulator to record statistics.
        /// </param>
        /// <param name="cvPathPricer">
        /// The control variate <see cref="PathPricer"/> or 
        /// null (<b>Nothing</b> in Visual Basic).
        /// </param>
        /// <param name="cvOptionValue">
        /// The option value in case of using a <paramref name="cvPathPricer"/>.
        /// </param>
        public MonteCarloModel(PathGenerator pathGenerator,
                               PathPricer pathPricer, RiskStatistics sampleAccumulator,
                               PathPricer cvPathPricer, double cvOptionValue)
        {
            _pathGenerator = pathGenerator;
            _pathPricer = pathPricer;
            SampleAccumulator = sampleAccumulator;
            _cvPathPricer = cvPathPricer;
            _cvOptionValue = cvOptionValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MonteCarloModel"/> class.
        /// </summary>
        /// <param name="pathGenerator">The path generator.</param>
        /// <param name="pathPricer">The path pricer list.</param>
        /// <param name="sampleAccumulatorList">The sample accumulators.</param>
        public MonteCarloModel(PathGenerator pathGenerator, MultiVarPathPricer pathPricer, List<RiskStatistics> sampleAccumulatorList)
        {
            _pathGenerator = pathGenerator;
            _multiVarPathPricer = pathPricer;
            _sampleAccumulatorList = sampleAccumulatorList;
        }

        private readonly PathGenerator _pathGenerator;
        private readonly PathPricer _pathPricer;
        private readonly PathPricer _cvPathPricer;
        private readonly double _cvOptionValue;

        //multvar monte private vars
        private readonly MultiVarPathPricer _multiVarPathPricer;
        private readonly List<RiskStatistics> _sampleAccumulatorList;       

        /// <summary>
        /// The sample acuumulator.
        /// </summary>
        public RiskStatistics SampleAccumulator { get; }       

        /// <summary>
        /// Generate samples and calculate option prices.
        /// </summary>
        /// <param name="samples">Number of samples to generate.</param>
        public void AddSamples(int samples)
        {
            for(int i=0; i<samples; i++) 
            {
                Sample path = _pathGenerator.Next();
                double price = _pathPricer.Value((Path[])path.Value);
                if (_cvPathPricer!=null)
                    price += _cvOptionValue - 
                             _cvPathPricer.Value((Path[])path.Value);
                SampleAccumulator.Add(price, path.Weight);
            }
        }

        /// <summary>
        /// Added to return multiple variable functions on one path realisation
        /// This is useful in returning simulation based sensitivities for example
        /// </summary>
        /// <param name="samples">The samples.</param>
        public void AddMultiVarSamples(int samples)
        {          
            for (int i = 0; i < samples; i++)
            {
                Sample path = _pathGenerator.Next();                               
                double[] price = _multiVarPathPricer.Value((Path[])path.Value);                          
                for (int idx = 0; idx < price.Length; idx++)
                {
                    if (i == 0)
                        _sampleAccumulatorList.Add(new RiskStatistics());                                        
                    _sampleAccumulatorList[idx].Add(price[idx], path.Weight);
                }                                
            }          
        }
    }
}