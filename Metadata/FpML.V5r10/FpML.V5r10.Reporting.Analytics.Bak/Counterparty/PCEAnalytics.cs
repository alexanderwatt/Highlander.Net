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

#region Usings

using System;
using FpML.V5r10.Codes;
using Orion.Analytics.Distributions;
using Math = System.Math;

#endregion

namespace Orion.Analytics.Counterparty
{
    /// <summary>
    /// Set of PCE approximations (from pricing centre)
    /// 
    /// Includes closed form PCE calcs for different product types
    /// </summary>
    public class PCEAnalytics
    {
        /// <summary>
        /// calculates the PCE at a point in time
        /// </summary>
        /// <param name="term"></param>
        /// <param name="rate"></param>
        /// <param name="fxVol"></param>
        /// <param name="irVol"></param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="correlation"></param>
        /// <param name="time"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public static double CrossCurrencySwapPCE(double term,
                                                  double rate,
                                                  double fxVol,
                                                  double irVol,
                                                  double meanRev,
                                                  double ci,
                                                  double correlation,
                                                  double time,
                                                  string region)
        {
            double fxpce;
            //Get the Interest Rate component of the PCE at time t from the IRS functionality
            double swapPCE = IRSwapPCE(term, rate, irVol, meanRev, ci, time);

            //And the Foreign Exchange component of the PCE at time t
            //
            // London state that, for a professional counterparty,they reset the principal every reset date
            // This means that they are running swap related risk over the entire period
            // but are running an FX risk only up to the next reset period.
            // Presumably the P&L impact of the principal reset is reflected in the MTM
            // We take this at face value and make a horrible adjustment to try and mimic this behaviour.
            //
            const double rateResetPeriod = 0.5D;
            if ((region == "LONDON") && (time > rateResetPeriod))
            {
                fxpce = 0.0D;
            }
            else
            {
                fxpce = FxForwardPCE(term, rate, fxVol, meanRev, ci, time);
            }
            //Then Calculate the overall cross currency PCE
            double resultPCE = swapPCE*swapPCE + 2.0D * correlation * swapPCE * fxpce + fxpce*fxpce;
            resultPCE = Math.Sqrt(resultPCE);
            return resultPCE;
        }

        /// <summary>
        /// calculates the PCE at a point in time
        /// </summary>
        /// <param name="term"></param>
        /// <param name="rate">Rate = Get_Rate(Term, Rate_Col_no)</param>
        /// <param name="fxVol"></param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double FxForwardPCE(double term,
                                          double rate,
                                          double fxVol,
                                          double meanRev,
                                          double ci,
                                          double time)
        {
            //**** diffusion component
            double temp1 = fxVol * ci * Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            double temp2 = Math.Exp(-rate * (term - time)); //**** discounting
            return temp1 * temp2;
        }

        /// <summary>
        /// ***
        /// ***    Term to Maturity is No_of_Periods
        /// ***   Tenor is now known from the input data (in years)
        /// ***   IR Vol is driven by the remaining Term to Maturity
        /// ***   Rate is driven by the remaining Term to Maturity.
        /// ***
        /// </summary>
        /// <param name="term"></param>
        /// <param name="tenor"></param>
        /// <param name="rate">Rate = Get_Rate(Term, Rate_Col_no)</param>
        /// <param name="irVol">IR_Vol = Get_IRVol(Term, IRVol_Col_no)</param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double FraPCE(double term,
                                    double tenor,
                                    double rate,
                                    double irVol,
                                    double meanRev,
                                    double ci,
                                    double time)
        {
            //**** diffusion component
            double temp1 = Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            return rate * ci * irVol * tenor * temp1; 
        }


        /// <summary>
        /// ****
        /// ****  Bought Bond Option - Cash Settled
        /// ****
        /// ****  Term to Expiry - seen on input as Term
        /// ****  Tenor - no information on spreadsheet - assuming 10 year vanilla Bond
        /// ****  IR vol is driven by Term to Expiry
        /// ****  Rate is driven by the proxied Tenor
        /// ****
        /// </summary>
        /// <param name="term"></param>
        /// <param name="tenor"></param>
        /// <param name="rate">Rate = Get_Rate(Tenor, Rate_Col_no)</param>
        /// <param name="irVol">IR_Vol = Get_IRVol(Term, IRVol_Col_no)</param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double BondOptionPCE(double term,
                                           double tenor,
                                           double rate,
                                           double irVol,
                                           double meanRev,
                                           double ci,
                                           double time)
        {
            //**** diffusion component
            double temp1 = irVol * ci * Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            double temp2 = 1.0D - Math.Exp(-temp1);
            double temp3 = 1.0D - Math.Pow((1.0D + rate / 2.0D) , (-2.0D * tenor)); //*** PV of future cash flows
            double temp4 = Math.Pow((1.0D + rate / 2.0D) , (-2.0D * (term - time))); //*** discounting
            return temp4 * temp3 * temp2;
        }

        /// <summary>
        /// ****
        /// ****  Repurchase Agreements - Cash Settled
        /// ****
        /// ****  Term to Expiry - seen on input as No_of_Periods
        /// ****  Tenor - seen on input
        /// ****  IR vol is driven by Term to Expiry
        /// ****  Rate is driven by the Tenor
        /// ****
        /// </summary>
        /// <param name="term"></param>
        /// <param name="tenor"></param>
        /// <param name="rate">Rate = Get_Rate(Tenor, Rate_Col_no)</param>
        /// <param name="irVol">IR_Vol = Get_IRVol(Term, IRVol_Col_no)</param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double RepoPCE(double term,
                                     double tenor,
                                     double rate,
                                     double irVol,
                                     double meanRev,
                                     double ci,
                                     double time)
        {
            //**** diffusion component
            double temp1 = irVol * ci * Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            double temp2 =  1.0D - Math.Exp(-temp1);
            double temp3 = 1.0D - Math.Pow((1.0D + rate) , (-1.0D * (tenor - term))); //*** PV of future cash flows
            return temp3 * temp2;
        }

        
        /// <summary>
        /// 
        /// ****  Bought swaption - Cash Settled
        /// ****
        /// ****  Term to Expiry - seen on input as No_of_Periods
        /// ****  Tenor - now known
        /// ****  IR vol is driven by Term to Expiry
        /// ****  Rate is driven by Tenor - should be fwd rate but will proxy this by spot rate.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="tenor"></param>
        /// <param name="rate">Get_Rate(Tenor, Rate_Col_no)</param>
        /// <param name="irVol">Get_IRVol(Term, IRVol_Col_no)</param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double SwaptionPCE(double term,
                                         double tenor,
                                         double rate,
                                         double irVol,
                                         double meanRev,
                                         double ci,
                                         double time)
        {
            //**** diffusion component
            double temp1 = irVol * ci * Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            double temp2 = 1.0D - Math.Exp(-temp1);
            double temp3 = 1.0D - Math.Pow((1.0D + rate / 2.0D) , (-2.0D * tenor)); //*** PV of future cash flows
            double temp4 = Math.Pow((1.0D + rate / 2.0D), (-2.0D * (term - time))); //*** discounting
            return temp4 * temp3 * temp2;
        }

        /// <summary>
        /// calculates the PCE at a point in time
        /// 
        /// ***  Term to Maturity is No_of_Periods
        /// ***   IR Vol is driven by this period
        /// ***   Rate is driven by this period.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="rate">Rate = Get_Rate(Term, Rate_Col_no)</param>
        /// <param name="irVol">IR_Vol = Get_IRVol(Term, IRVol_Col_no)</param>
        /// <param name="meanRev"></param>
        /// <param name="ci"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double IRSwapPCE(double term,
                                       double rate, //Rate based on Term and group
                                       double irVol, //IR vol
                                       double meanRev,
                                       double ci,
                                       double time) 
        {
            //**** diffusion component
            double diffusion = Math.Sqrt((1.0D - Math.Exp(-2.0D * meanRev * time)) / (2.0D * meanRev));
            diffusion = 1.0D - Math.Exp(-1.0 * irVol * ci * diffusion);
            //*** PV of future cash flows
            double cashflow = 1.0D - Math.Pow((1.0D + rate / 2.0D), (-2.0D * (term - time)));

            return cashflow * diffusion;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public static double AltNonePCE()
        {
            return 0.0;
        }

        ///<summary>
        ///</summary>
        ///<param name="boughtSold"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        public static double AltBondPCE(string boughtSold)
        {
            switch (boughtSold)
            {
                case "Bought":
                    return 1.0;
                case "N/A":
                    return 1.0;
                case "Sold":
                    return 0.0;
                default:
                    throw new NotSupportedException(boughtSold + " not implemented as a Boughtsold value");
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="boughtSold">Bought or Sold.</param>
        ///<param name="settled">Settlement type: Cash or Physical.</param>
        ///<param name="rateRenamed"></param>
        ///<param name="maturity"></param>
        ///<param name="tenor">note: Tenor refers to total outstanding tenor of bond ie now to maturity
        /// unlike Swaption, where tenor refers to underlying swap ie expiry to maturity</param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        ///<exception cref="NotSupportedException"></exception>
        public static double AltBondOptionPCE(string boughtSold,
                                               string settled,
                                               double rateRenamed,
                                               double maturity,
                                               double tenor,
                                               double time,
                                               double ci,
                                               double volatility,
                                               double reversion)
        {
            //****************************************************************************
            // note: Tenor refers to total outstanding tenor of bond ie now to maturity
            // unlike Swaption, where tenor refers to underlying swap ie expiry to maturity
            // ****************************************************************************

            //redenominate in years
            double mmaturity = maturity / 365.0;
            double ttime = time / 365.0;
            double ttenor = tenor / 365.0;

            double srs = volatility * Math.Sqrt((1.0 - Math.Exp(-2 * reversion * ttime)) / (2 * reversion));

            double altBondOptionPCE = 0.0D;

            switch (boughtSold)
            {
                case "Bought":
                    if (time <= maturity)
                    {
                        altBondOptionPCE = Math.Pow((1.0 + rateRenamed) , (ttime - mmaturity)) * (1.0 - Math.Pow((1.0 + rateRenamed) , (mmaturity - ttenor))) * (1.0 - Math.Exp(-ci * srs));
                    }
                    else if ((time > maturity) && (time <= tenor))  //TODO: check this condition
                    {
                        //branch on settlement type
                        switch (settled)
                        {
                            case "Physical":
                                //face value
                                altBondOptionPCE = 1.0;
                                break;
                            case "Cash":
                                altBondOptionPCE = 0.0;
                                break;
                            default:
                                throw new Exception("BondOption settlement type: " + settled + "  Unknown");
                        }
                    }
                    break;
                case "Sold":
                    if (time <= maturity)
                    {
                        altBondOptionPCE = 0.0;
                    }
                    else if ((time > maturity) && (time <= tenor))  //TODO: check this condition
                    {
                        switch (settled)
                        {
                            case "Physical":
                                //delivered... zero exposure
                                altBondOptionPCE = 0.0D;
                                break;
                            case "Cash":
                                altBondOptionPCE = 0.0;
                                break;
                            default:
                                throw new Exception("BondOption settlement type: " + settled + "  Unknown");
                        }
                    }
                    break;
                default:
                    throw new NotSupportedException(boughtSold + " not implemented as a Boughtsold value");
            }
            return altBondOptionPCE;
        }

        ///<summary>
        ///</summary>
        ///<param name="boughtSold"></param>
        ///<param name="settled"></param>
        ///<param name="rateRenamed"></param>
        ///<param name="maturity"></param>
        ///<param name="tenor"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<returns></returns>
        ///<exception cref="Exception"></exception>
        ///<exception cref="NotSupportedException"></exception>
        public static double AltSwaptionPCE(string boughtSold,
                                             string settled,
                                             double rateRenamed,
                                             double maturity,
                                             double tenor,
                                             double time,
                                             double ci,
                                             double volatility,
                                             double reversion)
        {
            //redenominate in years
            double mmaturity = maturity / 365.0;
            double ttime = time / 365.0;
            double ttenor = tenor / 365.0;
            double srs = volatility * Math.Sqrt((1.0 - Math.Exp(-2 * reversion * ttime)) / (2 * reversion));
            //GEIS' U1
            double srt = volatility * Math.Sqrt((1.0 - Math.Exp(-2 * reversion * mmaturity)) / (2 * reversion));
            //GEIS' U2
            double altswaptionPCE;
            switch (boughtSold)
            {
                case "Bought":
                    if (time < maturity)
                    {
                        altswaptionPCE = Math.Pow((1.0 + rateRenamed) , (ttime - mmaturity)) * (1.0 - Math.Pow((1 + rateRenamed), -ttenor)) * (1.0 - Math.Exp(-ci * srs));
                    }
                    else if ((time >= maturity) && (time < (maturity + tenor)))
                    {
                        //branch on settlement type
                        switch (settled)
                        {
                            case "Physical":
                                altswaptionPCE = (1.0 - Math.Pow((1.0 + rateRenamed) , (ttime - mmaturity - ttenor))) * (1.0 - Math.Exp(-ci * srs));
                                break;
                            case "Cash":
                                altswaptionPCE = 0.0D;
                                break;
                            default:
                                throw new Exception("Swaption settlement type: " + settled + "  Unknown");
                        }
                    }
                    else 
                    {
                        //must be on or beyond maturity + tenor...
                        altswaptionPCE = 0.0D;
                    }
                    break;
                case "Sold":
                    if (time < maturity)
                    {
                        altswaptionPCE = 0.0D;
                    }
                    else if ((time >= maturity) && (time < (maturity + tenor)))
                    {
                        switch (settled)
                        {
                            case "Physical":
                                //*****************************************
                                // Tem1 = Application.NormSDist(srt)
                                // *****************************************
                                double tem1 = NormalDistribution.Probability(srt);
                                double srst = volatility * Math.Sqrt((1.0 - Math.Exp(-2 * reversion * (ttime - mmaturity))) / (2 * reversion));
                                double tem2 = Math.Exp(volatility * volatility * mmaturity / 2 - srst * ci);
                                double tem3 = 1.0 - Math.Pow((1.0 + rateRenamed) , (ttime - mmaturity - ttenor));
                                altswaptionPCE = (1.0 - 2 * tem1 * tem2) * tem3;
                                if (altswaptionPCE < 0) altswaptionPCE = 0.0D;
                                break;
                            case "Cash":
                                altswaptionPCE = 0.0D;
                                break;
                            default:
                                throw new Exception("Swaption settlement type: " + settled + "  Unknown");
                        }
                    }
                    else
                    {
                        //outside range of interest, so.....
                        altswaptionPCE = 0.0D;
                    }
                    break;
                default:
                    throw new NotSupportedException(boughtSold + " not implemented as a Boughtsold value");
            }
            return altswaptionPCE;
        }

        ///<summary>
        ///</summary>
        ///<param name="rateRenamed"></param>
        ///<param name="tenor"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="irVolatility"></param>
        ///<param name="fxVolatility"></param>
        ///<param name="reversion"></param>
        ///<param name="fxCorrelation"></param>
        ///<returns></returns>
        public static double AltCrossCurrencySwapPCE(double rateRenamed, double tenor,
                                                      double time, double ci, double irVolatility, double fxVolatility,
                                                      double reversion, double fxCorrelation)
        {
            //calculates the PCE at a point in time

            if ((time < 0) || (time > tenor)) //Evaluate argument.
            {
                return 0.0D; //Invalid form
            }
            //Get the Interest Rate component of the PCE at time t from the IRS functionality
            double tempI = AltInterestRateSwapPCE(rateRenamed, tenor, time, ci, irVolatility, reversion);

            //And the Foreign Exchange component of the PCE at time t
            double tempF = AltFwdFxpce(tenor, time, ci, fxVolatility, reversion, rateRenamed);

            //Then Calculate the overall cross currency PCE
            double result = tempI * tempI + 2 * fxCorrelation * tempI * tempF + tempF * tempF;
            result = Math.Sqrt(result);
            return result;
        }

        /// <summary>
        /// Calculates PCE for fx forward
        /// </summary>
        /// <param name="maturity">Time (in days) to maturity of contract</param>
        /// <param name="time">Time horizon (in days) for PCE calculation</param>
        /// <param name="ci">Confidence level</param>
        /// <param name="volatility">Currency pair volatility -	Measure of the riskiness of the currency pair’s exchange rate</param>
        /// <param name="reversion">Reversion rate</param>
        /// <param name="interest">Interest rate</param>
        /// <returns></returns>
        private static double AltFwdFxpce(double maturity,
                                          double time,
                                          double ci,
                                          double volatility,
                                          double reversion,
                                          double interest)
        {
            //calculates the PCE at a point in time 

            // Evaluate argument. 
            if ((time < 0) ||
                (time > maturity))
            {
                return 0.0;
                //Invalid form 
            }
                //estimate PCE at time t 
            double functionReturnValue = Math.Exp(-interest * (maturity - time) / 365) * ci * volatility * Math.Sqrt((1 - Math.Exp(-2 * reversion * time / 365)) / (2 * reversion));

            return functionReturnValue;

        }

        /// <summary>
        /// Calculates closed form PCE for fx forward
        /// </summary>
        /// <param name="maturity">Time (in days) to maturity of contract</param>
        /// <param name="time">Time horizon (in days)</param>
        /// <param name="ci">Confidence level</param>
        /// <param name="volatility">Currency pair volatility</param>
        /// <param name="reversion">Reversion rate</param>
        /// <param name="faceValue">Amount of currency - Size of trade </param>
        /// <returns></returns>
        public static double FxFwdPCE(double maturity,
                                      double time,
                                      double ci,
                                      double volatility,
                                      double reversion,
                                      double faceValue)
        {
            if ((time < 0) ||
                (time > maturity))
            {
                return 0.0;
            }
            return faceValue * ci * volatility * Math.Sqrt((1 - Math.Exp(-2 * reversion * time / 365)) / (2 * reversion));
        }

        ///<summary>
        ///</summary>
        ///<param name="boughtsold"></param>
        ///<param name="maturity"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<param name="interest"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        public static double AltFXOptPCE(string boughtsold,
                                          double maturity,
                                          double time,
                                          double ci,
                                          double volatility,
                                          double reversion,
                                          double interest)
        {
            switch (boughtsold)
            {
                case "Bought":
                    return AltFwdFxpce(maturity, time, ci, volatility, reversion, interest);
                case "Sold":
                    return 0.0;
                default:
                    throw new NotSupportedException(boughtsold + " not implemented as a Boughtsold value");
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="rateRenamed"></param>
        ///<param name="bondTenor"></param>
        ///<param name="repoMaturity"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<returns></returns>
        public static double AltRepoPCE(double rateRenamed,
                                         double bondTenor,
                                         double repoMaturity,
                                         double time,
                                         double ci,
                                         double volatility,
                                         double reversion)
        {
            //calculates the PCE at a point in time

            if ((time < 0) || (time > repoMaturity))
            {
                return 0.0D; //Invalid form
            }
            //estimate PCE at time t
            double srs = volatility * Math.Sqrt((1.0 - Math.Exp(-2 * reversion * time / 365)) / (2 * reversion));
            return (1.0 - Math.Pow((1.0 + rateRenamed) , ((time - bondTenor) / 365))) * (1.0 - Math.Exp(-ci * srs));
        }

        ///<summary>
        ///</summary>
        ///<param name="payment"></param>
        ///<param name="rateRenamed"></param>
        ///<param name="maturity"></param>
        ///<param name="tenor"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<returns></returns>
        public static double AltFrapce(string payment,
                                        double rateRenamed,
                                        double maturity,
                                        double tenor,
                                        double time,
                                        double ci,
                                        double volatility,
                                        double reversion)
        {
            double functionReturnValue;

            //calculates the PCE at a point in time 

            // Invalid form 
            if ((time < 0) ||
                (time > maturity + tenor))
            {
                return 0.0;
                // Exit to calling procedure. 
            }
            if ((payment == "In Advance") &&
                (time > maturity))
            {
                return 0.0;
                // Exit to calling procedure. 
            }
                //estimate PCE beyond the term to maturity for payment in Arrears 
            if ((time > maturity) &&
                (time <= maturity + tenor))
            {
                functionReturnValue = rateRenamed * ci * volatility * tenor / 365 * Math.Sqrt((1 - Math.Exp(-2 * reversion * maturity / 365)) / (2 * reversion));
            }
                //estimate PCE as per normal 
            else
            {
                functionReturnValue = rateRenamed * ci * volatility * tenor / 365 * Math.Sqrt((1 - Math.Exp(-2 * reversion * time / 365)) / (2 * reversion));
            }
            return functionReturnValue;

        }

        private static double AltInterestRateSwapPCE(double rateRenamed, 
                                                     double tenor, double time, double ci, double volatility,
                                                     double reversion)
        {
            //calculates the PCE at a point in time
            if ( (time < 0) || (time > tenor))  // Evaluate argument.
            {
                //Invalid form
                return 0D; //Exit to calling procedure.
            }
            double srs = volatility * Math.Sqrt((1 - Math.Exp(-2.0 * reversion * time / 365.0)) / (2.0 * reversion));

            return (1.0 - Math.Pow((1.0 + rateRenamed) , ((time - tenor) / 365.0) ) ) * (1.0 - Math.Exp(-ci * srs));
        }

        ///<summary>
        ///</summary>
        ///<param name="boughtsold"></param>
        ///<param name="rateRenamed"></param>
        ///<param name="tenor"></param>
        ///<param name="time"></param>
        ///<param name="ci"></param>
        ///<param name="volatility"></param>
        ///<param name="reversion"></param>
        ///<returns></returns>
        ///<exception cref="NotSupportedException"></exception>
        public static double AltInterestRateOptPCE(string boughtsold, double rateRenamed,
                                                    double tenor, double time, double ci, double volatility, double reversion)
        {

            switch (boughtsold)
            {
                case "Bought":
                    return AltInterestRateSwapPCE(rateRenamed, tenor, time, ci, volatility, reversion);
                case "Sold":
                    return 0.0D;
                default:
                    throw new NotSupportedException(boughtsold + " not implemented as a Boughtsold value");
            }
        }

        /// <summary>
        /// Find the expected exposure
        /// </summary>
        /// <param name="mtm"></param>
        /// <param name="pce"></param>
        /// <param name="expExp"></param>
        /// <param name="expExp2"></param>
        public static void Exposures(double mtm,
                                     double pce,
                                     out double expExp,
                                     out double expExp2)
        {
            //**** Step 2 - Find the Cumulative Normal Probability 
            //**** CDF=1-NORMDIST(-MTM/PCE,0,1,TRUE) 
            //**** Step 3 - Find the marginal Normal Probability 
            //**** PDF=NORMDIST(-MTM/PCE,0,1,FALSE) 
            //**** 

            double cumulativeNormal = 1.0;
            double normalDensity = 0.0;
            if (Math.Abs(pce) > 1E-07)
            {
                normalDensity = NormalDistribution.Ndf(-mtm / pce); //use normal dist
                cumulativeNormal = 1.0 - new NormalDistribution().CumulativeDistribution(-mtm / pce); //use cum normal dist
            }

            //**** Step 4 - Find the expected exposure for the FX fwd 
            //**** MTM*CDF+PCE*PDF 
            //**** 
            expExp = mtm * cumulativeNormal + pce * normalDensity;
            expExp2 = (Math.Pow(mtm, 2) + Math.Pow(pce, 2)) * cumulativeNormal + mtm * pce * normalDensity;

        }


        /// <summary>
        /// to build the PCE and principal profile, and give the max value 
        /// extended to handle loans 
        /// </summary>
        /// <param name="ppl"></param>
        /// <param name="pce">Pce profile</param>
        /// <param name="rxmPce">Max pce value</param>
        /// <param name="productType"></param>
        /// <param name="ci"></param>
        /// <param name="product"></param>
        /// <param name="boughtsold"></param>
        /// <param name="cashPhys"></param>
        /// <param name="tenor"></param>
        /// <param name="maturity"></param>
        /// <param name="fraPayment"></param>
        /// <param name="reversion"></param>
        /// <param name="intRate"></param>
        /// <param name="irVol"></param>
        /// <param name="fxVol"></param>
        /// <param name="tabRows"></param>
        /// <param name="ptype"></param>
        /// <param name="months"></param>
        public static void ProfilePce(double[] ppl,
                                      out double[] pce,
                                      out double rxmPce,
                                      int productType,
                                      double ci,
                                      ProductTypeSimpleEnum product,
                                      string boughtsold,
                                      string cashPhys,
                                      double tenor,
                                      double maturity,
                                      string fraPayment,
                                      double reversion,
                                      double intRate,
                                      double irVol,
                                      double fxVol,
                                      int tabRows,
                                      string ptype,
                                      DateMonths months)
        {
            pce = new double[months.Totalmonths + 1];
            rxmPce = 0.0;
            // results stay in arrays 
            int i;
            double expi;

            if ((ptype != "N/A") &&
                (tabRows >= 2))
            {
                // product can have a profile, and table has more than one row.. 
                // build profile based on Ptype 

                if (ptype == "Swap")
                {
                    // swap type profile 
                    // per period df based on initial interest rate 
                    double df = Math.Pow((1 + intRate), (-1 / 12));
                    // Initialise total 
                    double cumSum = 0;

                    // step backwards from maturity date to start date 
                    //***** 
                    //***** Profile will be calculated over the term of the TradeBase ie [0, MaxMonths] 
                    //***** but will be placed forward in time if this is a forward start TradeBase ie [DelayMonths, Totalmonths] 
                    //***** 
                    for (i = months.Maxmonths; i >= 1; i += -1)
                    {
                        // sigma sqrt t 
                        double sst = irVol * Math.Pow(((1 - Math.Exp(-2 * reversion * i / 12)) / (2 * reversion)), 0.5);
                        // use approximation 
                        // DeltaR = IntRate * CI * Sst 
                        double deltaR = intRate * (Math.Exp(ci * sst) - 1);
                        cumSum = cumSum * df + ppl[i + months.DelayMonths] / 12;
                        expi = cumSum * deltaR;
                        if (expi > rxmPce) rxmPce = expi;
                        pce[i + months.DelayMonths] = expi;
                    }

                }
                else if (ptype == "LOAN")
                {
                    // loan type profile 
                    for (i = months.Totalmonths; i >= months.DelayMonths; i += -1)
                    {
                        pce[i] = ppl[i];
                        if (pce[i] > rxmPce) rxmPce = pce[i];
                    }

                }
                else
                {
                    throw new Exception("Attempt to build PCE profile- nonexisting product: " + ptype);
                }

            }
            else
            {
                // product either cannot or does not have a principal profile 
                // use closed form PCE calcs 
                //***** 
                //***** Profile will be calculated over the term of the TradeBase ie [0, MaxMonths] 
                //***** but will be placed forward in time if this is a forward start TradeBase ie [DelayMonths, Months.Totalmonths] 
                //***** 
                for (i = months.Maxmonths; i >= 1; i += -1)
                {
                    // use midpoint of month 
                    double t1 = (i - 0.5) / 12.0 * 365.0;
                    double tt1 = t1;
                    // average exposure 
                    expi = Value(product, tt1, boughtsold, cashPhys, tenor, maturity, irVol, fxVol, intRate, fraPayment, ci, reversion);
                    expi = expi * ppl[i + months.DelayMonths];

                    if (expi > rxmPce) rxmPce = expi;
                    pce[i + months.DelayMonths] = expi;
                }
            }
        }


        /// <summary>
        /// This method is used to calculate a PCE (closed form) for various products
        /// </summary>
        /// <param name="product">String representing valid product name (see enum)</param>
        /// <param name="time">Time horizon (in days) for PCE calculation. Point for which PCE is calculated, relative to reporting time</param>
        /// <param name="boughtsold"></param>
        /// <param name="cashPhys"></param>
        /// <param name="tenor"></param>
        /// <param name="maturity">Time (in days) to maturity of contract</param>
        /// <param name="irVol">IR volatility</param>
        /// <param name="fxVol">FX volatility</param>
        /// <param name="intRate">interest rate</param>
        /// <param name="fraPayment"></param>
        /// <param name="ci">Confidence level. Scales volatility to obtain desired level of risk measurement</param>
        /// <param name="reversion">Reversion rate. Factor controlling future dispersion of rates</param>
        /// <returns>PCE at time horizon</returns>
        public static double Value(ProductTypeSimpleEnum product,
                                   double time,
                                   string boughtsold,
                                   string cashPhys,
                                   double tenor,
                                   double maturity,
                                   double irVol,
                                   double fxVol,
                                   double intRate,
                                   string fraPayment,
                                   double ci,
                                   double reversion)
        {
            double functionReturnValue;

            switch (product)
            {
                case ProductTypeSimpleEnum.FxSwap:
                    functionReturnValue = AltFwdFxpce(maturity, time, ci, fxVol, reversion, intRate);
                    break;
                case ProductTypeSimpleEnum.FxForward: //fx forward
                    functionReturnValue = AltFwdFxpce(maturity, time, ci, fxVol, reversion, intRate);
                    break;
                case ProductTypeSimpleEnum.FxOption: //fx option
                    functionReturnValue = AltFXOptPCE(boughtsold, maturity, time, ci, fxVol, reversion, intRate);
                    break;
                case ProductTypeSimpleEnum.InterestRateSwap:
                    functionReturnValue = AltInterestRateSwapPCE(intRate, tenor, time, ci, irVol, reversion);
                    break;
                //interest rate option
                case ProductTypeSimpleEnum.CapFloor:
                    functionReturnValue = AltInterestRateOptPCE(boughtsold, intRate, tenor, time, ci, irVol, reversion);
                    break;
                case ProductTypeSimpleEnum.CrossCurrencySwap: //cross currency swap
                    functionReturnValue = AltCrossCurrencySwapPCE(intRate, tenor, time, ci, irVol, fxVol, reversion, 0);
                    break;
                case ProductTypeSimpleEnum.InterestRateSwaption: //swaption
                    functionReturnValue = AltSwaptionPCE(boughtsold, cashPhys, intRate, maturity, tenor, time, ci, irVol, reversion);
                    break;
                case ProductTypeSimpleEnum.BondOption: //bond option
                    functionReturnValue = AltBondOptionPCE(boughtsold, cashPhys, intRate, maturity, tenor, time, ci, irVol, reversion);
                    break;
                case ProductTypeSimpleEnum.FRA: //forward rate agreement
                    functionReturnValue = AltFrapce(fraPayment, intRate, maturity, tenor, time, ci, irVol, reversion);
                    break;
                    //case ProductTypeSimpleEnum.Repo: //repo 
                    //functionReturnValue = AltRepoPCE(intRate, tenor, maturity, time, ci, irVol, reversion);
                    //break;
                case ProductTypeSimpleEnum.BondTransaction: //bond
                    functionReturnValue = AltBondPCE(boughtsold);
                    break;
                //case ProductTypeSimpleEnum.Loan: //loan
                //    functionReturnValue = AltBondPCE("Bought");
                //    break;
                default:
                    throw new NotSupportedException(product + " not found as a product");

            }
            return functionReturnValue;
        }
    }
}