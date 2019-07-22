#region Usings



#endregion

namespace Orion.Analytics.Equities
{
    /// <summary>
    /// 
    /// </summary>
  public class DivList
  {
        private double[] _d;
        private double[] _t;

        public DivList()
        {
            Divpoints = 0;
        }

        /// <summary>
        /// set the number of div points
        /// </summary>
        public int Divpoints { get; set; }

        /// <summary>
        /// get rate item
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

            /// <summary>
        /// get time item
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

            /// <summary>
        /// set the  dividend and time 
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

        /// <summary>
        /// make the arrays
        /// </summary>
        public void MakeArrays()
        {
          if (_d == null)
              {
                _d = new double[Divpoints];
                _t = new double[Divpoints];
              }
        }

        /// <summary>
        /// empty the arrays
        /// </summary>
        public void EmptyArrays()
        {
          _d = null;
          _t = null;
        }
    }
}