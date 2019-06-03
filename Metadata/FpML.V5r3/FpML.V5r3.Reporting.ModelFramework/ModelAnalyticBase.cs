using System;
using System.Collections.Generic;
using Orion.Util.Helpers;


namespace Orion.ModelFramework
{
    /// <summary>
    /// Base Analytic class from which all analytic models should be extended
    /// P - Denotes the parameters TYPE
    /// EnumT - Denotes the metric enum TYPE
    /// </summary>
    /// <typeparam name="TP"></typeparam>
    /// <typeparam name="TEnumT">The type of the num T.</typeparam>
    public abstract class ModelAnalyticBase<TP, TEnumT> : IModelAnalytic<TP, TEnumT> where TP : class
    {
        #region IModel Members
        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id => GetType().Name;

        #endregion

        #region IAnalyticModel<P> Members

        /// <summary>
        /// Gets the metrics.
        /// </summary>
        /// <value>The metrics.</value>
        public List<TEnumT> Metrics { get; private set; }

        /// <summary>
        /// Gets or sets the analytics parameters.
        /// </summary>
        /// <value>The analytics parameters.</value>
        public TP AnalyticParameters { get; set; }

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        protected ModelAnalyticBase()
        {
            SetMetrics();
        }

        /// <summary>
        /// Sets the metrics.
        /// </summary>
        public void SetMetrics()
        {
            Type enumType = typeof(TEnumT);
            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");
            Array enumValArray = Enum.GetValues(typeof(TEnumT));
            var metrics = new TEnumT[enumValArray.Length];
            enumValArray.CopyTo(metrics, 0);
            Metrics = new List<TEnumT>(metrics);
        }

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        public TR Calculate<TR, TC>(string[] metrics)
        {
            if (AnalyticParameters == null)
            {
                throw new ArgumentException("Analytic model parameters must be set before Calculating the analytics");
            }
            Type type = typeof(TC);
            if (!type.IsClass)
            {
                throw new ArgumentException("The generic type C must be a class");
            }
            if (type.GetConstructor(new Type[] { }) == null)
            {
                throw new ArgumentException("Failed to create instance of concrete class as there is no parameterless constructor");
            }
            var results = (TR)Activator.CreateInstance(typeof(TC));
            Calculate(metrics, ref results);
            return results;
        }

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        public TR Calculate<TR, TC>(List<TEnumT> metrics)
        {
            return Calculate<TR, TC>(metrics.ToArray());
        }

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        public TR Calculate<TR, TC>(TEnumT[] metrics)
        {
            return Calculate<TR, TC>(AnalyticParameters, metrics);
        }

        /// <summary>
        /// Calculates the specified analytic parameters.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <typeparam name="TC"></typeparam>
        /// <param name="analyticParameters">The analytic parameters.</param>
        /// <param name="metrics">The metrics.</param>
        /// <returns></returns>
        public TR Calculate<TR, TC>(TP analyticParameters, TEnumT[] metrics)
        {
            AnalyticParameters = analyticParameters;
            Type enumType = typeof(TEnumT);
            // Can't use type constraints on value types, so have to do check like this
            if (enumType.BaseType != typeof(Enum))
                throw new ArgumentException("T must be of type System.Enum");
            if (AnalyticParameters == null)
            {
                throw new ArgumentException("Analytic model parameters must be set before Calculating the analytics");
            }
            Type type = typeof(TC);
            if (!type.IsClass)
            {
                throw new ArgumentException("The generic type C must be a class");
            }
            if (type.GetConstructor(new Type[] { }) == null)
            {
                throw new ArgumentException("Failed to create instance of concrete class as there is no parameterless constructor");
            }
            var metricList = new List<TEnumT>(metrics);
            List<string> metricsArray = metricList.ConvertAll(item => item.ToString()
                );
            var results = (TR)Activator.CreateInstance(typeof(TC));
            Calculate(metricsArray.ToArray(), ref results);
            return results;
        }

        /// <summary>
        /// Calculates the specified metrics.
        /// </summary>
        /// <typeparam name="TR"></typeparam>
        /// <param name="metrics">The metrics.</param>
        /// <param name="results">The results.</param>
        private void Calculate<TR>(IEnumerable<string> metrics, ref TR results)
        {
            if (AnalyticParameters == null)
            {
                throw new ArgumentException("Analytic model parameters must be set before Calculating the analytics");
            }
            foreach (string metric in metrics)
            {
                object value = Calculate(metric);
                ObjectLookupHelper.SetPropertyValue(results, metric, value);
            }
        }

        /// <summary>
        /// Calculates the specified metric.
        /// </summary>
        /// <param name="metric">The metric.</param>
        /// <returns></returns>
        private object Calculate(string metric)
        {
            object value = ObjectLookupHelper.GetPropertyValue(this, metric);
            return value;
        }
        #endregion
    }
}