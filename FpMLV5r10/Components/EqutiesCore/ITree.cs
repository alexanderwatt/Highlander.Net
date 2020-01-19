namespace Orion.EquitiesCore
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITree
    {
       
        //set the step size
        /// <summary>
        /// 
        /// </summary>
        int Gridsteps { get; set; }
        
        //set the volatility
        /// <summary>
        /// 
        /// </summary>
        double Sig { get; set; }
     
        //set the spot
        /// <summary>
        /// 
        /// </summary>
        double Spot { get; set; }

        //set the time step size
        /// <summary>
        /// 
        /// </summary>
        double Tau { get; set; }
      
        //get forward rate item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetR(int idx);
              

        //get div rate item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDiv(int idx);
       
        //get div rate item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDivtime(int idx);
       
        //get up item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetUp(int idx);
             
        //get dn item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        double GetDn(int idx);

        /// <summary>
        /// 
        /// </summary>
        double FlatRate { get; }
      
        /// <summary>
        /// 
        /// </summary>
        bool FlatFlag { get;}
       

        //get SpotMatrix item
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="jdx"></param>
        /// <returns></returns>
        double GetSpotMatrix(int idx, int jdx);

        /// <summary>
        /// Makes the grid.
        /// </summary>
        /// <param name="myZero">My zero.</param>
        /// <param name="myDivList">My div list.</param>
        void MakeGrid(ZeroCurve myZero, DivList myDivList);
       
    }
}

