#region Using directives

using System.Collections.Specialized;
using System.Reflection;
using Orion.ModelFramework;
using Orion.ModelFramework.Helpers;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// InterpolationFactory
    /// </summary>
    public class InterpolationFactory
    {
        #region Static Members

        private readonly static HybridDictionary InterpolationRegistry = new HybridDictionary();
        private const string DefaultAssembly = "FpML.V5r3.Analytics";

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
        static public void RegisterAssembly(string assemblyName)
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
        static public void UnregisterInterpolation(string className)
        {
            if (InterpolationRegistry.Contains(className))
                InterpolationRegistry.Remove(className);
        }

        /// <summary>
        /// Empty this factory of all registered classes
        /// </summary>
        static public void UnregisterAllInterpolations()
        {
            InterpolationRegistry.Clear();
        }

        /// <summary>
        /// List all currently registered scenario classes
        /// </summary>
        /// <returns></returns>
        static public string[] ListInterpolations()
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
        static public IInterpolation Create(string name)
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
            //WTF??? - hardcoded string literal for the assebmly name ????
            //
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
            //WTF??? - hardcoded string literal for the assebmly name ????
            //
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
            private readonly string _assembly;
            private readonly string _fullName;

            public string Assembly
            { get { return _assembly; } }

            public string FullName
            { get { return _fullName; } }

            public InterpolationRegistryValue(string assembly, string name)
            {
                _assembly = assembly;
                _fullName = name;
            }
        }

        #endregion

    }
}