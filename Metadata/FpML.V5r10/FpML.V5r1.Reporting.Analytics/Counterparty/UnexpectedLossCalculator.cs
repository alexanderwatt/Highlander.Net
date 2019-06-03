using System;
using System.Collections.Generic;
using System.Text;

namespace National.QRSC.Analytics.CreditMetrics
{
    /// <summary>
    /// Class to calculate unexpected loss 
    /// </summary>
    public class UnexpectedLossCalculator
    {
        /// <summary>
        /// Method to calculate unexpected loss
        /// 
        /// <param name="mdp"></param>
        /// <param name="LGD"></param>
        /// <param name="epe"></param>
        /// <param name="epeSq"></param>
        /// <param name="df"></param>
        /// <returns>unexpected loss vector</returns>
        /// </summary>
        public static decimal[] CalculateUL(decimal[] mdp, decimal LGD, decimal[] epe, decimal[] epeSq, decimal[] df)
        {
            decimal[] sum = new decimal[epe.Length];
            decimal stdev;
            decimal d1;
        
            const decimal TOL1 = 1.0M;

            for (int j = 0; j < epe.Length; ++j)
            {
                if (epe[j] > TOL1) {
                    d1 = epeSq[j] / epe[j] - epe[j];
                    d1 = checked(d1 * epe[j]);
                } else {
                    d1 = epeSq[j] - checked(epe[j] * epe[j]);
                }
                stdev = (decimal)Math.Sqrt((double)Math.Abs(d1)); 
                sum[j] = mdp[j] * LGD * stdev * df[j];
            }

            return sum;
        }

        /// <summary>
        /// Method to calculate unexpected loss.
        /// 
        /// Current Capital methodology is based on the concept of 'forward' Risk Capital.
        /// however there is also the concept of 'Expected' risk Capital.
        /// 
        /// The ExpRcFlag flag can be used to select which method should be applied.
        /// 
        /// Note: Use calcType setting as "Lending" for lending and "Derivative" for derivative
        /// calc method
        /// </summary>
        /// <param name="i">Monthly interval</param>
        /// <param name="AsAt"></param>
        /// <param name="Maturity"></param>
        /// <param name="Horizon"></param>
        /// <param name="LFlow"></param>
        /// <param name="RfDisc"></param>
        /// <param name="rating"></param>
        /// <param name="RatingNo"></param>
        /// <param name="Lied"></param>
        /// <param name="TermMatrix"></param>
        /// <param name="FutMatrix"></param>
        /// <param name="InvMatrix"></param>
        /// <param name="MtransMatrix"></param>
        /// <param name="Qmonthly"></param>
        /// <param name="CEDF"></param>
        /// <param name="nStates"></param>
        /// <param name="TRev"></param>
        /// <param name="ExpRcFlag">Use expected risk capital method flag</param>
        /// <param name="AuditDump"></param>
        /// <param name="calcType">"Lending" or "Derivative"</param>
        /// <returns></returns>
        public static double CalcUL(int i, int AsAt, int Maturity, int Horizon,
                    double[] LFlow, double RfDisc, string rating, int RatingNo, 
                    double Lied, double[,] TermMatrix, double[,] FutMatrix, 
                    double[,] InvMatrix, double[,] MtransMatrix, 
                    double[,] Qmonthly, double[,] CEDF,
                    int nStates, double[] TRev, bool ExpRcFlag, bool AuditDump,
                    string calcType)
        {
            double UL = 0.0;
            double KMVEL;
            double KMVUL;

            if (ExpRcFlag)
            {
                    //***********************************************************
                    //**** This block used with "expected" risk capital
                    //**** always move the 'AsAt' transition matrix back
                    TermMatrix = DataUtilities.Matrmult(TermMatrix, InvMatrix, nStates);

                    //**** FutMatrix arrives as the identity and is always
                    //**** incremented, until it becomes the one year
                    if (Horizon == Maturity) 
                    {
                        FutMatrix = DataUtilities.Matrmult(FutMatrix, MtransMatrix, nStates);
                    }

                    //**** "expected" risk capital [CALCULATED AT THE BEGINNING OF THE PERIOD]
                    AllCalcs(i - 1, Maturity, Horizon, LFlow, RfDisc, rating, RatingNo, Lied, TermMatrix, FutMatrix, out KMVEL, out KMVUL, Qmonthly, CEDF, nStates, TRev, calcType, AuditDump);

                    //**** End of "expected risk capital"
                    //***********************************************************
            }
            else
            {
                    //***********************************************************
                    //**** This block used with "forward" risk capital
                    //****   [CALCULATED AT THE BEGINNING OF THE PERIOD]

                    //**** move the Horizon time transition matrix back with the horizon
                    if (Horizon == Maturity) 
                    {
                        TermMatrix = DataUtilities.Matrmult(TermMatrix, InvMatrix, nStates);
                    }

                    //**** "forward" risk capital
                    XAllCalcs(i - 1, Maturity, Horizon, LFlow, RfDisc, rating, RatingNo, Lied, TermMatrix, out KMVEL, out KMVUL, Qmonthly, CEDF, nStates, TRev, calcType, AuditDump);

                    //**** end of "forward" risk capital
                    //***********************************************************
            }
            UL = KMVUL;
            return UL;
        }

        /// <summary>
        /// Method used for UL calculation under "expected" risk capital calculation.
        /// 
        /// AllCalcs returns the Unexpected Loss over the forward 12 month period
        /// From this we can calculate the Credit Risk Capital (CRC) value
        /// </summary>
        /// <param name="AsAt"></param>
        /// <param name="Maturity"></param>
        /// <param name="Horizon"></param>
        /// <param name="LFlow"></param>
        /// <param name="RfDisc"></param>
        /// <param name="rating"></param>
        /// <param name="RatingNo"></param>
        /// <param name="Lied"></param>
        /// <param name="AsAtMatrix"></param>
        /// <param name="AsToMatMatrix"></param>
        /// <param name="EL"></param>
        /// <param name="ExpUL"></param>
        /// <param name="Qmonthly"></param>
        /// <param name="CEDF"></param>
        /// <param name="nStates"></param>
        /// <param name="TRev"></param>
        /// <param name="ProductCategory"></param>
        /// <param name="AuditDump"></param>
        public static void AllCalcs(int AsAt, int Maturity, int Horizon,
                    double[] LFlow, double RfDisc, string rating, int RatingNo,
                    double Lied, double[,] AsAtMatrix, double[,] AsToMatMatrix,
                    out double EL, out double ExpUL,
                    double[,] Qmonthly, double[,] CEDF,
                    int nStates, double[] TRev, string ProductCategory, bool AuditDump)
        {
            EL = 0.0; //Added by damien during conversion to c#
            double CAtoM = 0;
            double Amount = 0;
            double LastEdf = 0;
            double CAtoH = 0;
            double EVHD = 0;
            double EVHnd = 0;
            double RDisc = 0;
            double SumE = 0;
            double SumE2 = 0;
            double UL = 0;
            double VarHND = 0;
            double VarHD = 0;

            double[] TempRes = null;
            double AdjProb = 0;
            double ERYVH = 0D;

            //UPGRADE_WARNING: Lower bound of array TempRes was changed from 1 to 0. 

            // given AsAt, Horizon and Maturity: 
            RDisc = Math.Pow(RfDisc, (Maturity - Horizon));

            // step back from Maturity via Horizon to AsAt 
            // collecting cash flows 
            for (int i = Maturity; i >= AsAt + 1; i += -1)
            {
                // Cashflows valued to horizon 
                Amount = LFlow[i] * RDisc;
                CAtoM = CAtoM + Amount;
                if (i > Horizon)
                {
                    for (int j = 1; j <= nStates - 1; j++)
                    {
                        TempRes[j] = TempRes[j] + Amount * (1 - Qmonthly[j, i - Horizon]);
                    }
                }
                else
                {
                    CAtoH = CAtoH + Amount;
                }
                RDisc = RDisc / RfDisc;
            }

            // For all rating states at 'AsAt' carry out unexpected loss calculation, 
            // then obtain Expected(UL) for horizon, at AsAt          
            ExpUL = 0.0;

            for (int j = 1; j <= nStates - 1; j++)
            {

                // CEDF wrt AsAt, for rating 
                LastEdf = CEDF[j, Horizon - AsAt];

                // step across possible rating states at horizon 
                SumE = 0;
                SumE2 = 0;
                for (int i = 1; i <= nStates - 1; i++)
                {
                    // condition on no default 
                    AdjProb = AsToMatMatrix[j, i] / (1 - AsToMatMatrix[j, nStates]);
                    SumE = SumE + TempRes[i] * AdjProb;
                    SumE2 = SumE2 + Math.Pow(TempRes[i], 2) * AdjProb;
                }

                // some calcs could be outside j loop, left in for clarity 
                EVHnd = (1 - Lied) * CAtoM + Lied * (CAtoH + SumE);
                VarHND = (SumE2 - Math.Pow(SumE, 2)) * Math.Pow(Lied, 2);

                EVHD = (1 - Lied) * CAtoM;
                VarHD = (Math.Pow(CAtoM, 2) * Lied * (1 - Lied)) / 4.0;

                UL = (LastEdf * VarHD + (1 - LastEdf) * VarHND + LastEdf * (1 - LastEdf) * Math.Pow((EVHnd - EVHD), 2));
                // cope with rounding probs 
                if (UL < 0.0)
                {
                    UL = 0;
                }
                else
                {
                    UL = Math.Pow(UL, 0.5);
                }

                EL = EVHD + EVHnd;

                ExpUL = ExpUL + UL * AsAtMatrix[RatingNo, j];
            }

            return;
        }


        /// <summary>
        ///  Method used for UL calculation under "forward" risk capital calculation
        /// </summary>
        /// <param name="AsAt"></param>
        /// <param name="Maturity"></param>
        /// <param name="Horizon"></param>
        /// <param name="LFlow"></param>
        /// <param name="RfDisc"></param>
        /// <param name="rating"></param>
        /// <param name="RatingNo"></param>
        /// <param name="Lied"></param>
        /// <param name="TermMatrix"></param>
        /// <param name="EL"></param>
        /// <param name="UL"></param>
        /// <param name="Qmonthly"></param>
        /// <param name="CEDF"></param>
        /// <param name="nStates"></param>
        /// <param name="TRev"></param>
        /// <param name="ProductCategory"></param>
        /// <param name="AuditDump"></param>
        public static void XAllCalcs(int AsAt, int Maturity, int Horizon,
            double[] LFlow, double RfDisc, string rating, int RatingNo, double Lied, double[,] TermMatrix,
            out double EL, out double UL,
            double[,] Qmonthly, double[,] CEDF, int nStates, double[] TRev, string ProductCategory, bool AuditDump)
        {
            EL = 0.0; //Added by damien during conversion to c#
            UL = 0.0; //Added by damien during conversion to c#
            double PplFlow = 0;
            double CAtoH = 0;
            double CAtoM = 0;
            double NextEdf = 0;
            double FirstEdf = 0;
            double Amount = 0;
            double SumEdf = 0;
            double LastEdf = 0;
            double CotoI = 0;
            double CHtoM = 0;
            double EffRate = 0;
            double VHDI = 0;
            double EVHD = 0;
            double EVHnd = 0;
            double SumE2 = 0;
            double SumE = 0;
            double RDisc = 0;
            double VarHND = 0;
            double VarHD = 0;
            double ETime = 0;
            double ITime = 0;
            int filenum = 0;
            double[] temam = null;
            double[] TempRes = null;
            double temsum = 0;
            int Offs = 0;
            double ERYVH = 0;
            double EVH = 0;
            double RYVo = 0;
            double RFVo = 0;
            double Vo = 0;
            double[] RYVH = null;
            int er = 0;

            //UPGRADE_WARNING: Lower bound of array TempRes was changed from 1 to 0. 
            //UPGRADE_WARNING: Lower bound of array temam was changed from 1 to 0. 
            //UPGRADE_WARNING: Lower bound of array RYVH was changed from 1 to 0. 

            // given AsAt, Horizon and Maturity: 
            RDisc = Math.Pow(RfDisc, (Maturity - Horizon));
            er = 1;

            // step back from Maturity via Horizon to AsAt 
            // collecting cash flows 
            for (int i = Maturity; i >= AsAt + 1; i += -1)
            {
                //**** 
                //**** Risk Free Cashflows valued to horizon 
                //**** If i > H then it is a discount factor 
                //**** if i < H then it is a wealth factor 
                //**** 
                Amount = LFlow[i] * RDisc;
                RYVo = RYVo + Amount * (1 - Qmonthly[RatingNo, i]);
                er = 2;
                if (i > Horizon)
                {
                    //**** 
                    //**** CHtoM is Adam's version of RFV(h) 
                    //**** 
                    CHtoM = CHtoM + Amount;
                    //**** 
                    //**** CAtoM is Adam's version of {RFV(h) + CF(AsAt -> H)} 
                    //**** 
                    CAtoM = CAtoM + Amount;
                    //**** 
                    //**** TempRes is Adam's version of SUM{CF(i) x [1-Pr(Cpty defaulting between H and i)]} 
                    //**** assuming that we are in state j at the Horizon 
                    //**** Cash flows go from Horizon to Maturity 
                    //**** 
                    for (int j = 1; j <= nStates - 1; j++)
                    {
                        temam[j] = Amount * (1 - Qmonthly[j, i - Horizon]);
                        TempRes[j] = TempRes[j] + Amount * (1 - Qmonthly[j, i - Horizon]);
                    }
                }
                else
                {
                    switch (ProductCategory)
                    {
                        case "Lending":
                            //**** 
                            //**** Lending can use the standard Delta Flow method 
                            //**** 

                            CAtoH = CAtoH + LFlow[i] * RDisc;
                            CAtoM = CAtoM + LFlow[i] * RDisc;
                            break;
                        case "Derivative":
                            //**** 
                            //**** Derivatives cannot use the standard method as it can cause large negative values 
                            //**** when Maturity >> Horizon especially for Xccy Swaps and long dated FX Fwds. 
                            //**** Replace with strict known cash flows and ignore potential market moves. 
                            //**** 
                            CAtoH = CAtoH + TRev[i] * RDisc;
                            CAtoM = CAtoM + TRev[i] * RDisc;
                            //**** 
                            //**** However, when Maturity <= Horizon, we need to take into account any actual exchange 
                            //**** of principal as this is not reflected in just the revenue flows. 
                            //**** 
                            if (i == Maturity)
                            {
                                CAtoH = CAtoH + LFlow[i] * RDisc;
                                CAtoM = CAtoM + LFlow[i] * RDisc;
                            }
                            break;
                        default:
                            throw new Exception("XAllCalcs: Product Category not supported");
                    }
                }
                er = 3;
                //**** 

                RDisc = RDisc / RfDisc;
            }

            er = 4;
            RYVo = RYVo * Lied / RDisc;
            RFVo = CAtoM * (1 - Lied) / RDisc;
            Vo = RYVo + RFVo;

            er = 5;

            // step across possible rating states at horizon 
            for (int i = 1; i <= nStates - 1; i++)
            {
                //**** 
                //**** Adam's version of RYV(h|State = i) 
                //**** = CF(AsAt -> H) + SUM{CF(i) x [1-Pr(Cpty defaulting between H and i)]} 
                //**** assuming that we are in state j at the Horizon 
                //**** 
                RYVH[i] = TempRes[i] + CAtoH;
                //**** 
                //**** SumE is Adam's version of E[RYV(h)] 
                //**** = SUM{RYV(h|State = i) x Pr(moving from Rating at time ASAT to rating i at Horizon)} 
                //**** assuming that we cannot default 
                //****       
                SumE = SumE + TempRes[i] * TermMatrix[RatingNo, i] / (1 - TermMatrix[RatingNo, nStates]);
                SumE2 = SumE2 + Math.Pow(TempRes[i], 2) * TermMatrix[RatingNo, i] / (1 - TermMatrix[RatingNo, nStates]);

                if (double.IsNaN(SumE))
                {
                    return;
                }
            }

            er = 6;
            ERYVH = SumE + CAtoH;
            //**** 
            //**** Scenario One - Assuming Default after the maturity of the Trade 
            //**** 
            //**** V(H|ND) = CF(AsAt -> H) + (1 - LGD(s)] * RFV(h) + LGD(s) * RYV(h) 
            //**** =(1 - LGD(s)] * {CF(AsAt -> H)+ RFV(h)} 
            //**** + LGD(s) * {CF(AsAt -> H)+ RYV(h)} 
            //**** 
            //**** E[V(H|ND)] 
            //**** = (1 - LGD(s)] * {CF(AsAt -> H) + RFV(h)} 
            //**** + LGD(s) * {CF(AsAt -> H) + E[RFV(h)]} 
            EVHnd = (1 - Lied) * CAtoM + Lied * (CAtoH + SumE);
            //**** 
            //**** Var[V(H|ND)] 
            //**** = E[V(H|ND)^2]-E[V(H|ND)]^2 
            //**** 
            VarHND = (SumE2 - Math.Pow(SumE, 2)) * Math.Pow(Lied, 2);

            er = 7;
            //**** 
            //**** Scenario Two - Assuming Default occurs at the Horizon date 
            //**** 
            //**** V(H|D) = (1 - LGD(s)] * {RFV(h) + CF(AsAt -> H)} 
            //**** 
            //**** E[V(H|D)] 
            //**** = (1 - LGD(s)] * {RFV(h)+ CF(AsAt -> H)} 
            //**** 
            EVHD = (1 - Lied) * CAtoM;
            //**** 
            //**** Var[(H|D)] 
            //**** = Var[LGD(s)] * [RFV(h) + CF(AsAt -> H)]^2 
            //**** 
            VarHD = Math.Pow(CAtoM, 2) * (Lied * (1 - Lied)) / 4.0;
            //**** 
            //**** There are two possibilities here 
            //**** (1) - Forward marginal EDFs 
            //**** (2) - Spot Marginal EDFs used forward in time 
            //**** 
            // get probabilities for this period AsAt to Horizon, wrt zero 
            // CEDF wrt AsAt, for rating 

            er = 8;
            FirstEdf = CEDF[RatingNo, AsAt];
            NextEdf = CEDF[RatingNo, Horizon];
            LastEdf = NextEdf - FirstEdf;

            UL = (LastEdf * VarHD + (1 - LastEdf) * VarHND + LastEdf * (1 - LastEdf) * Math.Pow((EVHnd - EVHD), 2));
            //**** 

            if (UL < 0.0)
            {
                UL = 0;
            }
            else
            {
                UL = Math.Pow(UL, 0.5);
            }
            //**** 
            //**** KMV Expected Loss is not yet calculated - not required for any calculation yet. 
            //**** 
            EL = 0;
            EVH = LastEdf * EVHD + (1 - LastEdf) * EVHnd;

            return;
        } 
    }
}
