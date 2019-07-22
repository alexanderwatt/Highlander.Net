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
using Highlander.Numerics.Helpers;

namespace Highlander.Numerics.Options
{
    public class OrcWingVol
    {

      #region Private Members

      public double currentVol { get; set; }
      public double slopeRef { get; set; }
      public double putCurve { get; set; }
      public double callCurve { get; set; }
      public double dnCutoff { get; set; }
      public double upCutoff { get; set; }
      public double refFwd { get; set; }
      public double refVol { get; set; }
      public double vcr { get; set; }
      public double scr { get; set; }
      public double ssr { get; set; }
      public double dsr { get; set; }
      public double usr { get; set; }
      public double timeToMaturity { get; set; }
      public double dsc { get; set; }
      public double usc { get; set; }

      #endregion
        
      #region Public Methods

      public static double Value(double k, OrcWingParameters parameters)
      {
          double vol = 0.0;
          double f = Math.Pow(parameters.AtmForward, parameters.Ssr / 100.0) * Math.Pow(parameters.RefFwd, 1.0 - parameters.Ssr / 100.0);
          double aux = Math.Log(k / f);
          //var usc = f * Math.Exp(Parameters.UpCutoff);
          //var dsc = f * Math.Exp(Parameters.DnCutoff);
          double sc = (parameters.SlopeRef - parameters.Scr * parameters.Ssr * (parameters.AtmForward - parameters.RefFwd) / parameters.RefFwd);
          double f1 = 1.0;
          double f2 = aux;
          double f3 = 0.0;
          if (aux <= 0.0)
              f3 = aux * aux;
          double f4 = 0.0;
          if (aux > 0.0)
              f4 = aux * aux;
          //Volatility surface calculations;      
          if (aux <= (1.0 + parameters.Dsr) * parameters.DnCutoff)
          {
              vol = parameters.CurrentVol + parameters.DnCutoff * (2.0 + parameters.Dsr) * (sc / 2.0) + (1.0 + parameters.Dsr) * parameters.PutCurve * parameters.DnCutoff * parameters.DnCutoff;

          }
          else if (((1.0 + parameters.Dsr) * parameters.DnCutoff < aux) & (aux <= parameters.DnCutoff))
          {
              vol = (-parameters.PutCurve / parameters.Dsr - sc / (2.0 * parameters.DnCutoff * parameters.Dsr)) * aux * aux
                    + (1.0 + parameters.Dsr) / parameters.Dsr * (2 * parameters.PutCurve * parameters.DnCutoff + sc) * aux
                    - parameters.PutCurve * parameters.DnCutoff * parameters.DnCutoff * (1.0 + 1.0 / parameters.Dsr)
                    - sc * parameters.DnCutoff / (2.0 * parameters.Dsr)
                    + parameters.CurrentVol;
          }
          else if ((parameters.UpCutoff < aux) & (aux <= (1.0 + parameters.Usr) * parameters.UpCutoff))
          {
              vol = (-parameters.CallCurve / parameters.Usr - sc / (2.0 * parameters.UpCutoff * parameters.Usr)) * aux * aux
                    + (1.0 + parameters.Usr) / parameters.Usr * (2.0 * parameters.CallCurve * parameters.UpCutoff + sc) * aux
                    - parameters.CallCurve * parameters.UpCutoff * parameters.UpCutoff * (1.0 + 1.0 / parameters.Usr)
                    - sc * parameters.UpCutoff / (2.0 * parameters.Usr)
                    + parameters.CurrentVol;
          }
          else if (aux > (1.0 + parameters.Usr) * parameters.UpCutoff)
          {
              vol = parameters.CurrentVol
                    + parameters.UpCutoff * (2.0 + parameters.Usr) * (sc / 2.0)
                    + (1.0 + parameters.Usr) * parameters.CallCurve * parameters.UpCutoff * parameters.UpCutoff;
          }
          else if ((parameters.Scr == 0.0) && (parameters.Vcr == 0.0))
          {
              vol = parameters.CurrentVol * f1 + sc * f2 + parameters.PutCurve * f3 + parameters.CallCurve * f4;
          }
          return vol;
      }

      public double orcvol(double atFwd, double k)
      {
          double vol = 0.0;
          double F = Math.Pow(atFwd, ssr / 100.0) * Math.Pow(refFwd, 1.0 - ssr / 100.0);
          double aux = Math.Log(k / F);
          double _usc = F * Math.Exp(upCutoff);
          double _dsc = F * Math.Exp(dnCutoff);
          double sc = (slopeRef - scr*ssr*(atFwd-refFwd)/refFwd);
          //f1;     
          double f1 = 1.0;
          //f2;
          double f2 = aux;
          //f3;
          double f3=0.0;
          if (aux <=0.0)      
            f3 = aux*aux;
          //f4;
          double f4 = 0.0;
          if (aux>0.0)
             f4 = aux * aux;
          //Volatility surface calculations;      
          if (aux <= (1.0 + dsr) * dnCutoff)
          {
            vol = currentVol + dnCutoff*(2.0+dsr)*(sc/2.0) + (1.0 + dsr)*putCurve*dnCutoff*dnCutoff;
          }
          else if (((1.0+dsr)*dnCutoff < aux) & (aux <= dnCutoff))
          {
            vol = (-putCurve/dsr - sc/(2.0*dnCutoff*dsr))*aux*aux
                          + (1.0+dsr)/dsr*(2*putCurve*dnCutoff+sc)*aux
                          - putCurve*dnCutoff*dnCutoff*(1.0+1.0/dsr)
                          - sc*dnCutoff/(2.0*dsr)
                          + currentVol;
          }
          else if ( (upCutoff < aux) & (aux <= (1.0+ usr)*upCutoff) )
          {
            vol = (-callCurve / usr - sc / (2.0 * upCutoff * usr)) * aux * aux
                  + (1.0 + usr) / usr * (2.0 * callCurve * upCutoff + sc) * aux
                  - callCurve * upCutoff * upCutoff * (1.0 + 1.0 / usr)
                  - sc * upCutoff / (2.0 *usr)
                  + currentVol;
          }
          else if (aux > (1.0 + usr) * upCutoff)
          {
            vol = currentVol 
                  + upCutoff * (2.0 + usr) * (sc / 2.0) 
                  + (1.0 + usr) * callCurve * upCutoff * upCutoff;
          }
          else if ((scr == 0.0) && (vcr == 0.0))
          {
            vol = currentVol * f1 + sc * f2 + putCurve * f3 + callCurve * f4;
          }
          return vol;
      }
      
      #endregion
    }
}
