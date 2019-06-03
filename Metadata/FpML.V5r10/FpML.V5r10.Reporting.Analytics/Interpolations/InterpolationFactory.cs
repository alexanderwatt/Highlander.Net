#region Using directives

using System.Collections.Specialized;
using System.Reflection;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Helpers;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// InterpolationFactory
    /// </summary>
    public class InterpolationFactory
    {
        #region Static Members

        private static readonly HybridDictionary InterpolationRegistry = new HybridDictionary();
        private const string DefaultAssembly = "FpML.V5r3.Reporting.Analytics";

        static InterpolationFactory()
        {
            // Add all classes that implement the IScenario interface from the default assembly
            RegisterAssembly(DefaultAssembly);
        }

        InterpolationFactory(string name)
        {
            Create(name);
        }

        /// <summary>
        /// Register any implementations of the IScenario interface
        /// that are in the specified assembly
        /// </summary>
        /// <param name="assemblyName">The assembly name to use when invoking</param>
        public static void RegisterAssembly(string assemblyName)
        {
            foreach (var interrogatedType in Assembly.Load(assemblyName).GetTypes())
            {
                var interpolationInterfaceType = interrogatedType.GetInterface(typeof(IInterpolation).Name);
                if (interpolationInterfaceType != null)
                {
                    InterpolationRegistry[interrogatedType.Name] = new InterpolationRegistryValue(assemblyName, interrogatedType.FullName);
                }
            }
        }

        /// <summary>
        /// Remove a class from the Interpolation Factory
        /// </summary>
        /// <param name="className">The name of the class to unregiter</param>
        public static void UnregisterInterpolation(string className)
        {
            if (InterpolationRegistry.Contains(className))
                InterpolationRegistry.Remove(className);
        }

        /// <summary>
        /// Empty this factory of all registered classes
        /// </summary>
        public static void UnregisterAllInterpolations()
        {
            InterpolationRegistry.Clear();
        }

        /// <summary>
        /// List all currently registered scenario classes
        /// </summary>
        /// <returns></returns>
        public static string[] ListInterpolations()
        {
            var allInterpolations = new string[InterpolationRegistry.Keys.Count];
            InterpolationRegistry.Keys.CopyTo(allInterpolations, 0);
            return allInterpolations;
        }

        /// <summary>
        /// Create an instance of the IScenario interface
        /// </summary>
        /// <param name="name">The class name to create</param>
        /// <returns></returns>
        public static IInterpolation Create(string name)
        {
            return ClassFactory<IInterpolation>.Create(DefaultAssembly, name);
        }


        /// <summary>
        /// Used to create the interpolation.
        /// </summary>
        /// <param name="baseCurve"></param>
        /// <returns></returns>
        public static IInterpolation CreateRateSpread(IRateCurve baseCurve)
        {
            var spreadInterpolation = new RateBasisSpreadInterpolation(baseCurve);
            return spreadInterpolation;
        }

        /// <summary>
        /// Used to create the interpolation.
        /// </summary>
        /// <param name="baseCurve"></param>
        /// <returns></returns>
        public static IInterpolation CreateCommoditySpread(ICommodityCurve baseCurve)
        {
            var spreadInterpolation = new CommodityBasisSpreadInterpolation(baseCurve);
            return spreadInterpolation;
        }

        ///<summary>
        ///</summary>
        ///<param name="termCurve"></param>
        ///<returns></returns>
        public static IInterpolation Create(TermCurve termCurve)
        {
            string interpolationName = "LinearInterpolation";
            if (null != termCurve.interpolationMethod)
            {
                interpolationName = termCurve.interpolationMethod.Value;
            }
            //WTF??? - hardcoded string literal for the assebmly name ????
            //
            return ClassFactory<IInterpolation>.Create(DefaultAssembly, interpolationName);
        }

        #endregion

        #region Inner Classes

        class InterpolationRegistryValue
        {
            public InterpolationRegistryValue(string assembly, string name)
            {
            }
        }

        #endregion
    }
}