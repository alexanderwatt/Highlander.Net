using System;

namespace Orion.EquitiesCore
{
  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class OrcWingVol
  {

    #region Private Members

    private double _currentVol;
    private double _slopeRef;
    private double _putCurve;
    private double _callCurve;
    private double _dnCutoff;
    private double _upCutoff;
    private double _refFwd;
    private double _refVol;
    private double _vcr;
    private double _scr;
    private double _ssr;
    private double _dsr;
    private double _usr;
    private double _timeToMaturity;
    private double _dsc;
    private double _usc;



    #endregion

    #region Accessors

    /// <summary>
    /// 
    /// </summary>
    public double CurrentVol
    {
      get => _currentVol;
        set => _currentVol = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double SlopeRef
    {
      get => _slopeRef;
        set => _slopeRef = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double PutCurve
    {
      get => _putCurve;
        set => _putCurve = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double CallCurve
    {
      get => _callCurve;
        set => _callCurve = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double DnCutoff
    {
      get => _dnCutoff;
        set => _dnCutoff = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double UpCutoff
    {
      get => _upCutoff;
        set => _upCutoff = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double RefFwd
    {
      get => _refFwd;
        set => _refFwd = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double RefVol
    {
      get => _refVol;
        set => _refVol = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Vcr
    {
      get => _vcr;
        set => _vcr = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Scr
    {
      get => _scr;
        set => _scr = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Ssr
    {
      get => _ssr;
        set => _ssr = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Dsr
    {
      get => _dsr;
        set => _dsr = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Usr
    {
      get => _usr;
        set => _usr = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double TimeToMaturity
    {
      get => _timeToMaturity;
        set => _timeToMaturity = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Dsc
    {
      get => _dsc;
        set => _dsc = value;
    }

    /// <summary>
    /// 
    /// </summary>
    public double Usc
    {
      get => _usc;
        set => _usc = value;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="atFwd"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public double Orcvol(double atFwd, double k)
    {
      double vol = 0.0;
      double F = Math.Pow(atFwd, _ssr / 100.0) * Math.Pow(_refFwd, 1.0 - _ssr / 100.0);
      double aux = Math.Log(k / F);
        double sc = (_slopeRef - _scr*_ssr*(atFwd-_refFwd)/_refFwd); 
      double f1 = 1.0;
      double f2 = aux;
      double f3=0.0;
      if (aux <=0.0)      
        f3 = aux*aux;
      double f4 = 0.0;
      if (aux>0.0)
         f4 = aux * aux;
      //Volatility surface calculations;      
      if (aux <= (1.0 + _dsr) * _dnCutoff)
      {
        vol = _currentVol + _dnCutoff*(2.0+_dsr)*(sc/2.0) + (1.0 + _dsr)*_putCurve*_dnCutoff*_dnCutoff;                    
      }
      else if (((1.0+_dsr)*_dnCutoff < aux) & (aux <= _dnCutoff))
      {
        vol = (-_putCurve/_dsr - sc/(2.0*_dnCutoff*_dsr))*aux*aux
                      + (1.0+_dsr)/_dsr*(2*_putCurve*_dnCutoff+sc)*aux
                      - _putCurve*_dnCutoff*DnCutoff*(1.0+1.0/_dsr)
                      - sc*_dnCutoff/(2.0*_dsr)
                      + _currentVol;
      }
      else if ( (_upCutoff < aux) & (aux <= (1.0+ _usr)*_upCutoff) )
      {
        vol = (-_callCurve / _usr - sc / (2.0 * UpCutoff * _usr)) * aux * aux
              + (1.0 + _usr) / _usr * (2.0 * _callCurve * _upCutoff + sc) * aux
              - _callCurve * _upCutoff * _upCutoff * (1.0 + 1.0 / _usr)
              - sc * UpCutoff / (2.0 *_usr)
              + _currentVol;
      }
      else if (aux > (1.0 + _usr) * _upCutoff)
      {
        vol = _currentVol 
              + _upCutoff * (2.0 + _usr) * (sc / 2.0) 
              + (1.0 + _usr) * _callCurve * _upCutoff * _upCutoff;
      }
      else if ((Scr == 0.0) && (Vcr == 0.0))
      {
        vol = _currentVol * f1 + sc * f2 + _putCurve * f3 + _callCurve * f4;
      }
      return vol;
    }

    #endregion
  }
}
