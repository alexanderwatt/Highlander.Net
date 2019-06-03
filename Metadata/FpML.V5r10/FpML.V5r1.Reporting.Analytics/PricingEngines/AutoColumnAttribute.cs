
#region Using directives

using System;
using System.Data;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Attribute to build <see cref="System.Data.DataColumn"/> 
    /// objects at runtime.
    /// </summary>
    [ AttributeUsage( AttributeTargets.Field, Inherited = true ) ]
    public class AutoColumnAttribute : Attribute
    {
        ///<summary>
        ///</summary>
        ///<param name="columnName"></param>
        ///<param name="dataType"></param>
        public AutoColumnAttribute(string columnName,
                                   Type dataType)
        {
            this.columnName = columnName;
            this.dataType = dataType;
        }

        #region private members

        private readonly string columnName;
        private readonly Type   dataType;
        
        private object      defaultValue;
        private double      minValue      =           double.MinValue;
        private double      maxValue      =           double.MaxValue;
        private bool        allowDBNull;
        private MappingType columnMapping =           MappingType.Element;
        
        #endregion

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public DataColumn NewColumn()
        {
            var dc = new DataColumn(columnName, dataType);
            if(defaultValue != null)
                dc.DefaultValue = defaultValue;
            dc.ColumnMapping = columnMapping;
            dc.AllowDBNull = allowDBNull;
            return dc;
        }

        #region Properties

        ///<summary>
        ///</summary>
        public string ColumnName
        { 
            get { return columnName; }
        }
        ///<summary>
        ///</summary>
        public Type DataType
        { 
            get { return dataType; }
        }
        ///<summary>
        ///</summary>
        public object DefaultValue
        {
            get { return defaultValue; }
            set { defaultValue = value; }
        }
        ///<summary>
        ///</summary>
        public double Min
        {
            get { return minValue; }
            set { minValue = value; }
        }
        ///<summary>
        ///</summary>
        public double Max
        {
            get { return maxValue; }
            set { maxValue = value; }
        }
        ///<summary>
        ///</summary>
        public bool AllowDBNull
        {
            get { return allowDBNull; }
            set { allowDBNull = value; }
        }
        ///<summary>
        ///</summary>
        public MappingType ColumnMapping
        { 
            get { return columnMapping; }
            set { columnMapping = value; }
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="row"></param>
        ///<exception cref="ApplicationException"></exception>
        public void Validate(DataRow row) 
        {
            if( row.IsNull(columnName) && AllowDBNull ) return;
            if( minValue > double.MinValue || maxValue < double.MaxValue )
            {
                double d;
                object o = row[columnName];
                if( o is double )
                    d = (double)o;
                else if (o is int ) 
                    d = Convert.ToDouble((int)o);
                else throw new ApplicationException("Column '" +
                                                    columnName + "': value is not numeric.");
                if( d<minValue || d>maxValue )
                    throw new ApplicationException( "Column '" +
                                                    columnName + "': value is out of allowed range.");
            }
        }
    }
}