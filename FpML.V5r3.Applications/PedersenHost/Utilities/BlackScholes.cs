using System;
using Extreme.Statistics.Distributions;

namespace PedersenHost.Utilities
{
    public enum PayStyle {Call, Put}

    class BlackScholes
    {
        public static double Black(double t, double f, double k, double v, PayStyle pay)
        {
            double d1 = (Math.Log(f / k) + 0.5 * v * v * t) / v / Math.Sqrt(t);
            double d2 = d1 - v * Math.Sqrt(t);
            double n1 = NormalDistribution.DistributionFunction(d1, 0, 1);
            double n2 = NormalDistribution.DistributionFunction(d2, 0, 1);
            if (pay == PayStyle.Call)
            {
                return f * n1 - k * n2;
            }
            return k * (1 - n2) - f * (1 - n1);
        }

        public static double ImpVol(double t, double f, double k, double p, double atm, PayStyle pay)
        {
            const double ftol = 0.000000001;
            double result = atm;
            for (int i = 0; i < 50; i++)
            {
                double fun = Black(t, f, k, result, pay) - p;
                if (Math.Abs(fun) < ftol)
                {
                    return result;
                }
                double f1 = Black(t, f, k, result + 0.00001, pay) - p;
                double delta = -fun * 0.00001 / (f1 - fun);
                result += 0.7 * delta;
                //0.7 is a dampening factor
            }
            return result;
        }
    }
}
