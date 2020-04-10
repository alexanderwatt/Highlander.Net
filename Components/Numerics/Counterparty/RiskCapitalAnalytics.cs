/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Numerics.Counterparty
{
    ///<summary>
    ///</summary>
    public class RiskCapitalAnalytics
    {
        private readonly decimal[] _multiplierCoefficients;

        ///<summary>
        ///</summary>
        public enum AlphaCoefficient
        {
            ///<summary>
            ///</summary>
            MRC = 0, 
            ///<summary>
            ///</summary>
            OPC = 1, 
            ///<summary>
            ///</summary>
            BRC = 2
        }; 

        /// <summary>
        /// Constructor to initialises the table and parameters
        /// </summary>
        public RiskCapitalAnalytics()
        {
            _multiplierCoefficients = new decimal[3];
            _multiplierCoefficients[(int)AlphaCoefficient.MRC] = 1.0M / 3.0M;
            _multiplierCoefficients[(int)AlphaCoefficient.OPC] = 1.0M / 3.0M;
            _multiplierCoefficients[(int)AlphaCoefficient.BRC] = 1.0M / 3.0M;
        }

        /// <summary>
        /// CalculateRiskCapital for RAROC calculation
        /// </summary>
        /// <param name="ul"></param>
        /// <param name="capAdjustFactor"></param>
        /// <param name="numStandDeviations"></param>
        /// <param name="corEstimate"></param>
        /// <returns></returns>
        public decimal CalculateRiskCapital(decimal ul, decimal capAdjustFactor, decimal numStandDeviations, decimal corEstimate)
        {
            var sqrtRho = (decimal) Math.Sqrt((double) corEstimate);
            //calculate CRC
            decimal crc = capAdjustFactor * numStandDeviations * sqrtRho * ul;
            decimal mrc = _multiplierCoefficients[(int)AlphaCoefficient.MRC] * crc;
            decimal opc = _multiplierCoefficients[(int)AlphaCoefficient.OPC] * crc;
            decimal brc = _multiplierCoefficients[(int)AlphaCoefficient.BRC] * crc;
            decimal riskCap = crc + mrc + opc + brc;
            return riskCap;
        }


        /// <summary>
        /// Calculate undiscounted RC for lending
        /// 
        /// for Lending Total Risk Capital = Credit + Operational Risk Capital
        /// </summary>
        /// <param name="concentration"></param>
        /// <param name="sqCorrelation"></param>
        /// <param name="confidence"></param>
        /// <param name="ul">unexpected loss</param>
        /// <param name="opRiskCapital">operational risk capital</param>
        /// <param name="mktRiskCapital">market risk capital</param>
        /// <param name="capWeighting"></param>
        /// <param name="crc">credit risk capital</param>
        /// <returns>Per Period RC (undiscounted)</returns>
        public static double CalcLendRC(double concentration, double sqCorrelation,
                                        double confidence, double ul,
                                        double opRiskCapital, double mktRiskCapital, double capWeighting,
                                        out double crc)
        {
            //*****
            //*****  Current Policy, for Lending, is that the Operational Risk Capital
            //*****  (ORC) is calculated as a % of Credit Capital (ie CRC)
            //*****
            crc = concentration * sqCorrelation * confidence * ul;
            double rc = crc * (1 + opRiskCapital + mktRiskCapital) * capWeighting;
            double perPeriodRC = rc;
            return perPeriodRC;
        }

        /// <summary>
        ///  Calculate undiscounted RC for markets
        ///  
        /// 
        /// </summary>
        /// <param name="concentration"></param>
        /// <param name="sqCorrelation"></param>
        /// <param name="confidence"></param>
        /// <param name="ul">unexpected loss</param>
        /// <param name="opRiskCapital">operational risk capital</param>
        /// <param name="mktRiskCapital">market risk capital</param>
        /// <param name="capWeighting"></param>
        /// <param name="crc">credit risk capital</param>
        /// <returns>Per Period RC (undiscounted)</returns>
        public static double CalcDerivRC(double concentration, double sqCorrelation,
                                         double confidence, double ul,
                                         double opRiskCapital, double mktRiskCapital, double capWeighting,
                                         out double crc)
        {
            crc = concentration * sqCorrelation * confidence * ul;
            double rc = crc * (1 + opRiskCapital + mktRiskCapital) * capWeighting;
            double perPeriodRC = rc;
            return perPeriodRC;
        }
    }
}