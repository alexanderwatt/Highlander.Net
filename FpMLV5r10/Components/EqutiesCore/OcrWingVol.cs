using System;
using System.Collections.Generic;
using System.Text;

namespace EquitiesAddin
{
  public class OcrWingVol
  {

    #region Private Members

    private double _currentVol;
    private double _slopeRef;
    private double _putCurve;
    private double _callCurve;
    private double _dnCutoff;
    private double _upCutoff;
    private double _refFwd;
    private double _vcr;
    private double _scr;
    private double _ssr;
    private double _dsr;
    private double _usr;



    #endregion


    #region Accessors

    public double currentVol
    {
      get { return _currentVol; }
      set { _currentVol = value; }
    }

    public double slopeRef
    {
      get { return _slopeRef; }
      set { _slopeRef = value; }
    }
    public double putCurve
    {
      get { return _putCurve; }
      set { _putCurve = value; }
    }

    public double callCurve
    {
      get { return _callCurve; }
      set { _callCurve = value; }
    }

    public double dnCutoff
    {
      get { return _dnCutoff; }
      set { _dnCutoff = value; }
    }

    public double upCutoff
    {
      get { return _upCutoff; }
      set { _upCutoff = value; }
    }

    public double refFwd
    {
      get { return _refFwd; }
      set { _refFwd = value; }
    }

    public double vcr
    {
      get { return _vcr; }
      set { _vcr = value; }
    }

    public double scr
    {
      get { return _scr; }
      set { _scr = value; }
    }

    public double ssr
    {
      get { return _ssr; }
      set { _ssr = value; }
    }

    public double dsr
    {
      get { return _dsr; }
      set { _dsr = value; }
    }

    public double usr
    {
      get { return _usr; }
      set { _usr = value; }
    }



    #endregion


    #region

    public double orcvol(double atFwd, double k)
    {





    }


    #endregion
  }
}
