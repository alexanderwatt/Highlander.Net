// This file is part of Orion.NET, a set of open source assemblies 


namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Time grid interface.
    /// </summary>
    public interface ITimeGrid // public std::SparseVector<Time> 
    {
        // TODO: Add a ToString() method

        ///<summary>
        ///</summary>
        int Count
        {
            get;
        }

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        double this[int i]
        {
            get;
        }

        ///<summary>
        ///</summary>
        ///<param name="time"></param>
        ///<returns></returns>
        int FindIndex(double time);

        ///<summary>
        ///</summary>
        ///<param name="i"></param>
        ///<returns></returns>
        double Dt(int i);
    }
}