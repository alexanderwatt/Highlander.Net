#region Using directives

using System.Data;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Interface for option pricing engines.
    /// </summary>
    public interface IPricingEngine
    {
        ///<summary>
        ///</summary>
        ///<returns></returns>
        string ToString();
        ///<summary>
        ///</summary>
        string UniqueId
        {
            get;
        }
        ///<summary>
        ///</summary>
        ///<returns></returns>
        DataSet NewDataSet();
        ///<summary>
        ///</summary>
        ///<param name="dataSet"></param>
        void Calculate(DataSet dataSet);
        ///<summary>
        ///</summary>
        ///<param name="args"></param>
        ///<param name="results"></param>
        void Calculate(DataRow args, DataRow results);
        ///<summary>
        ///</summary>
        ///<param name="dataSet"></param>
        void Validate(DataSet dataSet);
        ///<summary>
        ///</summary>
        ///<param name="dataRow"></param>
        void Validate(DataRow dataRow);
    }
}