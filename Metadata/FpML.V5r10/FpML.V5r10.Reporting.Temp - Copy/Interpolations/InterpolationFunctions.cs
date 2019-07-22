/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Orion.Analytics.Interpolations
{
    /// <summary>
    /// 
    /// </summary>
    public static class InterpolationFunctions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyData"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LinearInterp(object[,] xyData, double x)
        {
            int size=xyData.GetUpperBound(0);
            int start = 0;
            for (int i = 0; i < size; i++)
            {
                if (x >= (double)xyData[i, 0] && x < (double)xyData[i+1, 0]) start=i;
            }
            if (x <= (double)xyData[0, 0]) start = 0;
            if (x >= (double)xyData[size, 0]) start = size - 1;
            double output = (double)xyData[start, 1] + (x - (double)xyData[start, 0]) * ((double)xyData[start + 1, 1] - (double)xyData[start, 1])
                            / ((double)xyData[start + 1, 0] - (double)xyData[start, 0]);
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyData"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double PiecewiseLinearInterp(object[,] xyData, double x)
        {
            int size = xyData.GetUpperBound(0);
            int start = 0;
            for (int i = 0; i < size; i++)
            {
                if (x > (double)xyData[i, 0] && x <= (double)xyData[i + 1, 0]) start = i;
            }
            if (x <= (double)xyData[0, 0]) start = 0;
            if (x >= (double)xyData[size, 0]) start = size - 1;
            var output = (double)xyData[start + 1, 1];
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyData"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LogLinearInterp(object[,] xyData, double x)
        {
            int size = xyData.GetUpperBound(0);
            int start = 0;
            for (int i = 0; i <= size; i++)
            {
                if (x >=(double)xyData[i, 0] && x < (double)xyData[i+1, 0]) start = i;
            }
            if (x <= (double)xyData[1, 0]) start = 1;
            if (x >= (double)xyData[size, 0]) start = size - 1;
            double output = Math.Exp(Math.Log((double)xyData[start, 1]) + (x - (double)xyData[start, 0])
                                     * (Math.Log((double)xyData[start + 1, 1]) - Math.Log((double)xyData[start, 1]))
                                     / ((double)xyData[start + 1, 0] - (double)xyData[start, 0]));
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyData"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double HSplineInterp(object[,] xyData, double x)
        {
            int size = xyData.GetUpperBound(0);
            int start = 0;
            for (int i = 0; i < size; i++)
            {
                if (x >= (double)xyData[i, 0] && x < (double)xyData[i + 1, 0]) start = i;
            }
            if (x == (double)xyData[size, 0]) { start = size - 1; }
            if (x < (double)xyData[0, 0]) return 0;
            if (x > (double)xyData[size, 0]) return 0;
            //Reference - Wikipedia http://en.wikipedia.org/wiki/Cubic_Hermite_spline
            double t = (x - (double)xyData[start, 0]) / ((double)xyData[start + 1, 0] - (double)xyData[start, 0]);
            double h00 = 2 * Math.Pow(t, 3) - 3 * Math.Pow(t, 2) + 1;
            double h10 = Math.Pow(t, 3) - 2 * Math.Pow(t, 2) + t;
            double h01 = - 2 * Math.Pow(t, 3) + 3 * Math.Pow(t, 2);
            double h11 = Math.Pow(t, 3) - Math.Pow(t, 2);
            double slopeIn;
            double slopeOut;
            double slopeMid = ((double)xyData[start + 1, 1] - (double)xyData[start, 1]) /
                                ((double)xyData[start + 1, 0] - (double)xyData[start, 0]);
            if (start < 1)
            
            {
                slopeIn = slopeMid;
            }           
            else
            {
                slopeIn = ((double)xyData[start, 1] - (double)xyData[start - 1, 1])/
                            ((double)xyData[start, 0] - (double)xyData[start - 1, 0]);
            }
            if (start >= size - 1)
            {
                slopeOut = slopeMid;
            }
            else
            {
                slopeOut = ((double)xyData[start + 2, 1] - (double)xyData[start + 1, 1]) /
                            ((double)xyData[start + 2, 0] - (double)xyData[start + 1, 0]);
            }
            double m0 = (slopeIn + slopeMid) / 2 * ((double)xyData[start + 1, 0] - (double)xyData[start, 0]); // Slope of tangent at start of spline
            double m1 = (slopeMid + slopeOut) / 2 * ((double)xyData[start + 1, 0] - (double)xyData[start, 0]); // Slope of tangent at end  of spline
            double output = h00 * (double)xyData[start, 1] + h10 * m0 + h01 * (double)xyData[start + 1, 1] + h11 * m1;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyzData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double BiLinearInterp(object[,] xyzData, double x, double y)
        {
            int sizeX = xyzData.GetUpperBound(0);
            int sizeY = xyzData.GetUpperBound(1);
            int startX = 0;
            int startY = 0;
            for (int i = 1; i < sizeX; i++)
            {
                if (x > (double)xyzData[i, 0] && x <= (double)xyzData[i + 1, 0]) startX = i;
            }
            if (x <= (double)xyzData[1, 0]) startX = 1;
            if (x >= (double)xyzData[sizeX, 0]) startX = sizeX-1;

            for (int i = 1; i < sizeY; i++)
            {
                if (y > (double)xyzData[0, i] && y <= (double)xyzData[0,i + 1]) startY = i;
            }

            if (y <= (double)xyzData[0, 1]) startY = 1;
            if (y >= (double)xyzData[0, sizeY]) startY = sizeY-1;

            double weightX = (x - (double)xyzData[startX, 0]) / ((double)xyzData[startX + 1, 0] - (double)xyzData[startX, 0]);
            double weightY = (y - (double)xyzData[0, startY]) / ((double)xyzData[0, startY + 1] - (double)xyzData[0, startY]);

            double output = (double)xyzData[startX, startY] * (1 - weightX) * (1 - weightY)
                            + (double)xyzData[startX + 1, startY] * weightX * (1 - weightY)
                            + (double)xyzData[startX, startY + 1] * (1 - weightX) * weightY
                            + (double)xyzData[startX + 1, startY + 1] * weightX * weightY;

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xyzData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double BiLinearInterp2(object[,] xyzData, double x, double y)
        {
            int sizeX = xyzData.GetUpperBound(0);
            int sizeY = xyzData.GetUpperBound(1);

            int startX = 0;
            int startY = 0;

            for (int i = 1; i < sizeX; i++)
            {
                if (x > (double)xyzData[i, 0] && x <= (double)xyzData[i + 1, 0]) startX = i;
            }

            if (x <= (double)xyzData[1, 0]) startX = 1;
            if (x >= (double)xyzData[sizeX, 0]) startX = sizeX - 1;

            for (int i = 1; i < sizeY; i++)
            {
                if (y > (double)xyzData[0, i] && y <= (double)xyzData[0, i + 1]) startY = i;
            }

            if (y <= (double)xyzData[0, 1]) startY = 1;
            if (y >= (double)xyzData[0, sizeY]) startY = sizeY - 1;

            var xPillars = new object[2, 2];
            var yPillars = new object[2, 2];

            yPillars[0, 0] = xyzData[0, startY];
            yPillars[1, 0] = xyzData[0, startY + 1];
            yPillars[0, 1] = xyzData[startX, startY];
            yPillars[1, 1] = xyzData[startX, startY + 1];

            xPillars[0, 0] = xyzData[startX, 0];
            xPillars[0, 1] = LinearInterp(yPillars, y);

            yPillars[0, 1] = xyzData[startX + 1, startY];
            yPillars[1, 1] = xyzData[startX + 1, startY + 1];

            xPillars[1, 0] = xyzData[startX + 1, 0];
            xPillars[1, 1] = LinearInterp(yPillars, y);

            double output = LinearInterp(xPillars, x);

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zeroNodes"></param>
        /// <param name="interpX"></param>
        /// <param name="interpSpace"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static double GeneralZeroInterp(object[,] zeroNodes, double interpX, string interpSpace, double zeroCompFreq, string method)
        {
            int size = zeroNodes.GetUpperBound(0);
            var fwdNodes = new object[size + 1];

            int start = 0;
            for (int i = 0; i < size; i++)
            {
                if (interpX >= (double)zeroNodes[i, 0] && interpX < (double)zeroNodes[i + 1, 0]) start = i;
                fwdNodes[i] = DF2Zero(Zero2DF((double)zeroNodes[i + 1, 1], zeroCompFreq, (double)zeroNodes[i+1, 0] / 365)
                                / Zero2DF((double)zeroNodes[i, 1], zeroCompFreq, (double)zeroNodes[i, 0] / 365)
                                    , zeroCompFreq, ((double)zeroNodes[i + 1, 0] - (double)zeroNodes[i, 0]) / 365);
            }

            fwdNodes[size] = (double)zeroNodes[size, 1];


            double y1 = 0;
            double y2 = 0;
            double output = 0;

            var x1 = (double)zeroNodes[start, 0];
            var x2 = (double)zeroNodes[start + 1, 0];
            
            if (interpSpace == "ZERO")
            {
                y1 = (double)zeroNodes[start, 1];
                y2 = (double)zeroNodes[start + 1, 1];
            }

            if (interpSpace == "DF")
            {
                y1 = Zero2DF((double)zeroNodes[start, 1], zeroCompFreq, x1 / 365);
                y2 = Zero2DF((double)zeroNodes[start + 1, 1], zeroCompFreq, x2 / 365);
            }

            if (interpSpace == "FWD")
            {
                y1 = (double)fwdNodes[start];
                y2 = (double)fwdNodes[start];
            }

            if (method == "LINEAR")
            {
                output = y1 + (interpX - x1) * (y2 - y1) / (x2 - x1);
                if (interpSpace == "DF") output = DF2Zero(output, zeroCompFreq, interpX / 365);
            }
            if (method == "LOGLINEAR")
            {
                output = Math.Exp(Math.Log(y1) + (interpX - x1) * (Math.Log(y2) - Math.Log(y1)) / (x2 - x1));
                if (interpSpace=="DF") output = DF2Zero(output, zeroCompFreq, interpX / 365);
            }

            if (interpSpace == "FWD")
            {
                double interpDF = Zero2DF(output, zeroCompFreq, (interpX - x1) / 365);
                double startDF = Zero2DF((double)zeroNodes[start, 1], zeroCompFreq, x1 / 365);
                output = DF2Zero(interpDF * startDF, zeroCompFreq, interpX / 365);
            }   
            
            if (interpX <= (double)zeroNodes[0, 0]) output = (double)zeroNodes[0, 1];
            if (interpX >= (double)zeroNodes[size, 0]) output = (double)zeroNodes[size, 1];

            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zero"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static double Zero2DF(double zero, double zeroCompFreq, double T)
        {
            double output = 0;
            if (zeroCompFreq>0) output= Math.Pow((1 + zero / zeroCompFreq), -zeroCompFreq * T);
            if (zeroCompFreq == 0) output = Math.Exp(-T * zero);
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="df"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="T"></param>
        /// <returns></returns>
        public static double DF2Zero(double df, double zeroCompFreq, double T)
        {
            double output = 0;
            if (zeroCompFreq > 0) output = (Math.Pow(df, -1 / (zeroCompFreq * T)) - 1) * zeroCompFreq;
            if (zeroCompFreq == 0) output = -Math.Log(df) * 1 / T;
            return output;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="matTime"></param>
        /// <param name="curve"></param>
        /// <param name="interpSpace"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static double FWDfromCurve(double startTime, double matTime, object[,] curve, string interpSpace, double zeroCompFreq, string method)
        {
            double startZero = GeneralZeroInterp(curve, startTime, interpSpace, zeroCompFreq, method);
            double matZero = GeneralZeroInterp(curve, matTime, interpSpace, zeroCompFreq, method);
            return DF2Zero(Zero2DF(matZero, zeroCompFreq, matTime) / Zero2DF(startZero, zeroCompFreq, startTime),zeroCompFreq,matTime-startTime);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cf"></param>
        /// <param name="cfDays"></param>
        /// <param name="interpSpace"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="method"></param>
        /// <param name="seedRates"></param>
        /// <returns></returns>
        public static object[,] ZeroCalc(object[,] cf, object[,] cfDays, string interpSpace, double zeroCompFreq, string method, object[,] seedRates)
        {
            int numInstruments = cf.GetUpperBound(0);
            int numCashflows = cf.GetUpperBound(1);
            var nodes = new object[numInstruments + 2, 2];

            nodes[0, 0] = 0.00;
            nodes[0, 1] = seedRates[0, 0];

            for (int i = 1; i <= numInstruments+1; i++) // Loop to set up Nodes array with initial values for PV calc.
            {
                nodes[i, 0] = 0;
                nodes[i, 1] = seedRates[i-1,0];

                for (int j = 0; j <= numCashflows; j++)
                {
                    if (Convert.ToDouble(cfDays[i-1,j])> Convert.ToDouble(nodes[i,0])) nodes[i, 0] = cfDays[i-1,j];
                }
            }   

            var pv = new object[numInstruments + 1]; //Initial PV calc.

            for (int i = 0; i <= numInstruments; i++)
            {
                pv[i] = PVFromCurve(cfDays,cf,nodes,i,interpSpace,zeroCompFreq,method);
            }

            double shift = Math.Pow(10, -4);

            for (int i = 0; i <= numInstruments; i++) //Loop to iterate to zero PV for all instruments
            {
                int count = 0;
                while (Math.Abs((double)pv[i]) > Math.Pow(10, -10)&& count<10)
                {
                    nodes[i + 1, 1] = (double)nodes[i + 1, 1] + shift;
                    double pvUp = PVFromCurve(cfDays, cf, nodes, i, interpSpace, zeroCompFreq, method);
                    nodes[i + 1, 1] = (double)nodes[i + 1, 1] - 2 * shift;
                    double pvDown = PVFromCurve(cfDays, cf, nodes, i, interpSpace, zeroCompFreq, method);
                    nodes[i + 1, 1] = (double)nodes[i + 1, 1] + shift;
                    double pvChange = (pvUp - pvDown) / 2;
                    nodes[i + 1, 1] = (double)nodes[i + 1, 1] - (double)pv[i] / pvChange * shift;
                    pv[i] = PVFromCurve(cfDays, cf, nodes, i, interpSpace, zeroCompFreq, method);
                    count++;
                }

                pv[i] = PVFromCurve(cfDays, cf, nodes, i, interpSpace, zeroCompFreq, method);
            }

            nodes[0, 1] = nodes[1, 1];
            return nodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfDays"></param>
        /// <param name="cf"></param>
        /// <param name="curve"></param>
        /// <param name="instrumentNumber"></param>
        /// <param name="interpSpace"></param>
        /// <param name="zeroCompFreq"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public static double PVFromCurve(object[,] cfDays, object[,] cf, object[,] curve, int instrumentNumber, string interpSpace, double zeroCompFreq, string method)
        {
            int numCashflows = cf.GetUpperBound(1);
            var df = new object[numCashflows + 1];

            double pvTmp = 0.0;
            int i = instrumentNumber;
            
            for (int j = 0; j <= numCashflows; j++)
            {
                //Map the valid days.
                double cfDaysTmp;
                object o = cfDays[i, j];
                if ((o == null) || (o.ToString() == String.Empty)) { cfDaysTmp = 0.0; }
                else { cfDaysTmp = Convert.ToDouble(cfDays[i, j]); }
                //MAp the valid cash flows.
                double cfTmp;
                o = cf[i, j];
                if ((o == null) || (o.ToString() == String.Empty)) { cfTmp = 0.0; } 
                else { cfTmp = Convert.ToDouble(cf[i, j]); }
                //InterpOut = LinearInterp(Curve, CF_Days_Tmp);
                double interpOut = GeneralZeroInterp(curve,cfDaysTmp,interpSpace,zeroCompFreq,method);
                if (zeroCompFreq != 0) { df[j] = Math.Pow((1 + interpOut / zeroCompFreq), -zeroCompFreq * cfDaysTmp / 365); }
                if (zeroCompFreq == 0) { df[j] = Math.Exp(-interpOut * cfDaysTmp / 365); }
                var dfTmp = (double)df[j];
                pvTmp += cfTmp * dfTmp;
            }
            return pvTmp;
        }
    }
}