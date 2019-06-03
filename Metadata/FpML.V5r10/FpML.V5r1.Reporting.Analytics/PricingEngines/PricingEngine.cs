
using System;
using System.Data;
using Orion.Analytics.PricingEngines;

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Abstract base class for data-driven pricing engines.
    /// </summary>
    public abstract class PricingEngine : IPricingEngine
    {

        #region Implementation of IPricingEngine
        public DataSet NewDataSet()
        {
            var ds = new DataSet(UniqueId);
            var arguments = NewArgumentsTable();
            var results = NewResultsTable();
            ds.Tables.Add( arguments );
            return ds;
        }
        public void Calculate(DataSet dataSet)
        {
            var arguments = dataSet.Tables["Arguments"];

            // get or make the results table
            DataTable results;
            if( dataSet.Tables.Contains("Results") )
            {
                results = dataSet.Tables["Results"];
            }
            else
            {
                results = NewResultsTable();
                dataSet.Tables.Add( results );
                dataSet.Relations.Add( arguments.PrimaryKey, results.PrimaryKey);
            }

//			// get the relation between the two tables
//			DataRelation relArgsResults = null;
//			foreach( DataRelation rel in ds.Relations)
//				if(rel.ParentTable == arguments && rel.ChildTable==results)
//					relArgsResults = rel;
//			if( relArgsResults == null )
//				throw new ApplicationException("Could not find a relation between " + 
//					"arguments and results.");

            // calculate all rows
            object[] keys = results.PrimaryKey;
            foreach( DataRow args in arguments.Rows ) 
            {
                var result = results.NewRow();
                Calculate(args, result);
                try
                {
                    result.SetParentRow(args);
                } 
                catch
                {}
//				DataRow prev = results.Rows.Find(keyValues);
//				prev.
//				if(Contains(keyValues));
                try
                {
                    results.Rows.Add( result );
                } 
                catch( ConstraintException /* e */) 
                {
                    // Debug.WriteLine(e);
                    var keyValues = new object[keys.Length];
                    for(int i=0; i<keys.Length; i++)
                        keyValues[i] = result[(DataColumn)keys[i]];
                    results.Rows.Find(keyValues).Delete();
                    results.Rows.Add(result);
                }
            }
        }

        abstract public void Calculate(DataRow args, DataRow results);

        public void Validate(DataSet dataSet)
        {
            DataTable arguments = dataSet.Tables["Arguments"];
            // DataTable results   = dataSet.Tables["Results"];
            foreach( DataRow row in arguments.Rows )
                Validate(row);
        }
        abstract public void Validate(DataRow dataRow);
        abstract public string UniqueId
        {
            get;
        }
        #endregion

        static private DataTable argumentsTable;
        protected virtual DataTable NewArgumentsTable()
        {
            if( argumentsTable == null ) 
            {
                argumentsTable = new DataTable("Arguments");
                var argumentsId = new DataColumn("id", typeof(long)) {AutoIncrement = true};
                argumentsTable.Columns.Add( argumentsId );
                // argumentsId made NotDBNull and Unique below!
                argumentsTable.PrimaryKey = new[]{ argumentsId };
            }
            return argumentsTable.Clone();
        }

        static private DataTable resultsTable;
        protected virtual DataTable NewResultsTable()
        {
            if( resultsTable == null ) 
            {
                resultsTable = new DataTable("Results");
                var resultsId = new DataColumn("id", typeof(long)) {AutoIncrement = true};
                resultsTable.Columns.Add( resultsId );
                // resultsId made NotDBNull and Unique below!
                resultsTable.PrimaryKey = new[]{ resultsId };
            }
            return resultsTable.Clone();
        }
    }
}