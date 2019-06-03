
using System;
// COM interop attributes
using System.Data;
using System.Collections;
using System.Reflection;

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// Base class with reflection helpers to dynamically build 
    /// argument and result tables.
    /// </summary>
    abstract public class DataColumns
    {
        static private FieldInfo[] columnFields(Type t)
        {
            var fields = t.GetFields( BindingFlags.FlattenHierarchy |
                                              BindingFlags.Public | BindingFlags.NonPublic | 
                                              BindingFlags.Instance);
            var filtered = new ArrayList();
            foreach(FieldInfo field in fields)
            {
                if( field.GetCustomAttributes( typeof(AutoColumnAttribute), 
                                               true).Length > 0 )
                    filtered.Add(field);
            }
            return (FieldInfo[]) filtered.ToArray(typeof(FieldInfo));
        }

        static protected DataColumn[] reflectColumns(Type t)
        {
            FieldInfo[] fields = columnFields(t);
            var columns = new DataColumn[fields.Length];
            int i=0;
            foreach( FieldInfo field in fields) 
            {
                object[] attrs = field.GetCustomAttributes(
                    typeof(AutoColumnAttribute), true);
                var ac = (AutoColumnAttribute)attrs[0];
                columns[i++] = ac.NewColumn();
            }
            return columns;
        }

        static protected void validateColumns(Type t, DataRow r)
        {
            foreach( FieldInfo field in columnFields(t)) 
            {
                object[] attrs = field.GetCustomAttributes(
                    typeof(AutoColumnAttribute), true);
                var ac = (AutoColumnAttribute)attrs[0];
                ac.Validate(r);
            }
        }

        protected void parseColumns(DataRow r)
        {
            Type t = GetType();
            foreach( FieldInfo field in columnFields(t)) 
            {
                object[] attrs = field.GetCustomAttributes(
                    typeof(AutoColumnAttribute), true);
                var ac = (AutoColumnAttribute)attrs[0];
                field.SetValue(this, r[ac.ColumnName]);
            }
        }
    }
}