namespace Orion.EquitiesCore
{
  /// <summary>
  /// 
  /// </summary>
  public class DivList 
  {
      private double[] _d;
    private double[] _t;

    //set the number of div points
      /// <summary>
      /// 
      /// </summary>
      public int Divpoints { get; set; }

      //get rate item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetD(int idx)
    {
        if (idx < Divpoints)
      {
        return _d[idx];
      }
        return 0.0;
    }

      //get time item
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <returns></returns>
    public double GetT(int idx)
    {
        if (idx < Divpoints)
      {
        return _t[idx];
      }
        return 0.0;
    }

      //set the  dividend and time 
    /// <summary>
    /// 
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="d"></param>
    /// <param name="t"></param>
    public void SetD(int idx, double d, double t)
    {
      if (idx < Divpoints)
      {
        _d[idx] = d;
        _t[idx] = t;
      }
    }

    //make the arrays
    /// <summary>
    /// 
    /// </summary>
    public void MakeArrays()
    {
      if (_d == null)
      {
        _d = new double[Divpoints];
        _t = new double[Divpoints];
      }
    }

    //empty the arrays
    /// <summary>
    /// 
    /// </summary>
    public void EmptyArrays()
    {
      _d = null;
      _t = null;
    }


  }

}
