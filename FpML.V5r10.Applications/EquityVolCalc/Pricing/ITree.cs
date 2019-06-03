using System;

namespace Orion.Equity.VolatilityCalculator.Pricing
{
    public interface ITree : ICloneable
    {
        //set the step size
        int Gridsteps { get; set; }

        //set the volatility
        double Sig { get; set; }

        //set the spot
        double Spot { get; set; }

        //set the time step size
        double Tau { get; set; }

        //get forward rate item
        double GetR(int idx);

        //get div rate item
        double GetDiv(int idx);

        //get div rate item
        double GetDivtime(int idx);

        //get up item
        double GetUp(int idx);

        //get dn item
        double GetDn(int idx);

        double FlatRate { get; }

        bool FlatFlag { get; }

        //get SpotMatrix item
        double GetSpotMatrix(int idx, int jdx);

        /// <summary>
        /// Makes the grid.
        /// </summary>
        void MakeGrid();

        //object Clone();
    }
}
