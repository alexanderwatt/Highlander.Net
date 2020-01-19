using System.Collections.Generic;
using Highlander.Utilities.NamedValues;

namespace Highlander.Metadata.Common
{
    public static class AlgorithmHelper
    {
        ///  <summary>
        ///  Examples of values are:
        /// <Property name = "Tolerance" > 1E-10</Property >
        /// < Property name="Bootstrapper">SimpleRateBootstrapper</Property>
        /// <Property name = "BootstrapperInterpolation" > LinearRateInterpolation</Property >
        /// < Property name="CurveInterpolation">LinearInterpolation</Property>
        /// <Property name = "UnderlyingCurve" > ZeroCurve</Property >
        /// < Property name="CompoundingFrequency">Continuous</Property>
        /// <Property name = "ExtrapolationPermitted" > true </Property >
        /// < Property name="DayCounter">ACT/365.FIXED</Property>
        ///  </summary>
        ///  <returns></returns>
        public static Algorithm CreateAlgorithm(NamedValueSet algorithmProperties)
        {
            var properties = new List<Property>
            {
                new Property {name = "Tolerance", Value = algorithmProperties.GetString("Tolerance", "1E-10")},
                new Property {name = "Bootstrapper", Value = algorithmProperties.GetString("Bootstrapper", "FastBootstrapper")},
                new Property {name = "BootstrapperInterpolation", Value = algorithmProperties.GetString("BootstrapperInterpolation", "LinearRateInterpolation")},
                new Property {name = "CurveInterpolation", Value = algorithmProperties.GetString("CurveInterpolation", "LinearInterpolation")},
                new Property {name = "UnderlyingCurve", Value = algorithmProperties.GetString("UnderlyingCurve", "ZeroCurve")},
                new Property {name = "CompoundingFrequency", Value = algorithmProperties.GetString("CompoundingFrequency", "Quarterly")},
                new Property {name = "ExtrapolationPermitted", Value = algorithmProperties.GetString("ExtrapolationPermitted", "true")},
                new Property {name = "DayCounter", Value = algorithmProperties.GetString("DayCounter", "ACT/365.FIXED")},
                new Property {name = "DateGenerationRule", Value = algorithmProperties.GetString("DateGenerationRule", "12")}
            };
            var algorithm = new Algorithm { Properties = properties.ToArray() };
            return algorithm;
        }

        ///  <summary>
        ///  Examples of values are:
        /// <Property name = "Tolerance" > 1E-10</Property >
        /// < Property name="Bootstrapper">SimpleRateBootstrapper</Property>
        /// <Property name = "BootstrapperInterpolation" > LinearRateInterpolation</Property >
        /// < Property name="CurveInterpolation">LinearInterpolation</Property>
        /// <Property name = "UnderlyingCurve" > ZeroCurve</Property >
        /// < Property name="CompoundingFrequency">Continuous</Property>
        /// <Property name = "ExtrapolationPermitted" > true </Property >
        /// < Property name="DayCounter">ACT/365.FIXED</Property>
        ///  </summary>
        ///  <param name="tolerance">A decimal value.</param>
        ///  <param name="bootstrapper">E.g. FastBootstrapper</param>
        ///  <param name="bootstrapperInterpolation">LogLinearInterpolation</param>
        ///  <param name="curveInterpolation">LogLinearInterpolation</param>
        ///  <param name="underlyingCurve">DiscountFactorCurve or ZeroCurve</param>
        ///  <param name="compoundingFrequency">Continuous</param>
        ///  <param name="extrapolation">true</param>
        ///  <param name="dayCounter">Typically ACT/365.FIXED</param>
        ///  <returns></returns>
        public static Algorithm CreateAlgorithm(string tolerance,
            string bootstrapper, string bootstrapperInterpolation, string curveInterpolation,
            string underlyingCurve, string compoundingFrequency, string extrapolation, string dayCounter)
        {
            var properties = new List<Property>
            {
                new Property {name = "Tolerance", Value = tolerance},
                new Property {name = "Bootstrapper", Value = bootstrapper},
                new Property {name = "BootstrapperInterpolation", Value = bootstrapperInterpolation},
                new Property {name = "CurveInterpolation", Value = curveInterpolation},
                new Property {name = "UnderlyingCurve", Value = underlyingCurve},
                new Property {name = "CompoundingFrequency", Value = compoundingFrequency},
                new Property {name = "ExtrapolationPermitted", Value = extrapolation},
                new Property {name = "DayCounter", Value = dayCounter}
            };
            var algorithm = new Algorithm { Properties = properties.ToArray() };
            return algorithm;
        }
    }
}
