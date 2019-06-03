
#region Using directives

using System.Data;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Vanilla engine base class.
    /// </summary>
    public abstract class VanillaEngine : PricingEngine
    {

        #region Implementation of IPricingEngine
//		abstract public Calculate(DataRow args, DataRow results);
        override public void Validate(DataRow dataRow)
        {
            VanillaOptionArguments.Validate(dataRow);
        }
//		abstract public string UniqueId
//		{
//			get;
//		}
        #endregion

        static private DataTable argumentsTable;
        protected override DataTable NewArgumentsTable()
        {
            if( argumentsTable == null ) 
            {
                argumentsTable = base.NewArgumentsTable();
                argumentsTable.Columns.AddRange( VanillaOptionArguments.Columns );
            }
            return argumentsTable.Clone();
        }

        static private DataTable resultsTable;
        protected override DataTable NewResultsTable()
        {
            if( resultsTable == null ) 
            {
                resultsTable = base.NewResultsTable();
                resultsTable.Columns.AddRange( OptionValue.Columns );
                resultsTable.Columns.AddRange( OptionGreeks.Columns );
            }
            return resultsTable.Clone();
        }

    }
}