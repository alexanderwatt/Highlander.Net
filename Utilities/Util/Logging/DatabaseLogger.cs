/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using Orion.Util.Helpers;

namespace Orion.Util.Logging
{
    public enum MandatoryLogField { Source, Severity, Description, User, LogDateTime }

    public interface IDatabaseLogger : ILogger
    {
        string ApplicationName { get; set; }
    }

    public class DatabaseLogger : BaseLogger, IDatabaseLogger
    {
        private const string DefaultFormat = "{prefix}{text}{suffix}";

        private DatabaseLog Logger { get; }
        public string ApplicationName { get; set; }
        public string ConnectionString { get; }
        public SqlDataAdapter Adapter { get; }
        public DataTable LogTable { get; }
        public string LogTableName { get; }

        internal IDictionary<MandatoryLogField, string> FieldMappings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DatabaseLogger class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logTable">The log table.</param>
        /// <param name="applicationName"></param>
        public DatabaseLogger(string connectionString, string logTable, string applicationName)
            : this(connectionString, logTable, null, applicationName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseLogger class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logTable">The log table.</param>
        /// <param name="fieldMappings">The field mappings.</param>
        /// <param name="applicationName">Name of the application.</param>
        public DatabaseLogger(string connectionString, string logTable, IDictionary<MandatoryLogField, string> fieldMappings, string applicationName)
            : this(connectionString, logTable, fieldMappings, applicationName, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseLogger class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logTableName">The log table.</param>
        /// <param name="mandatoryFieldMappings">The persist interval.</param>
        /// <param name="applicationName">The application name to show in the log.</param>
        /// <param name="prefix">The prefix.</param>
        public DatabaseLogger(string connectionString, string logTableName, IDictionary<MandatoryLogField, string> mandatoryFieldMappings, string applicationName, string prefix)
            : this(connectionString, logTableName, applicationName, mandatoryFieldMappings, prefix, 0.0d)
        {
        }

        /// <summary>
        /// Initializes a new instance of the DatabaseLogger class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="logTable">The log table.</param>
        /// <param name="applicationName">The application name to show in the log.</param>
        /// <param name="mandatoryFieldMappings">The column mappings.</param>
        /// <param name="prefix">The prefix.</param>
        /// <param name="persistInterval">The persist interval.</param>
        public DatabaseLogger(string connectionString, string logTable, string applicationName, IDictionary<MandatoryLogField, string> mandatoryFieldMappings, string prefix, Double persistInterval)
            : base(prefix, null)
        {
            Format = DefaultFormat;
            Prefix = prefix;

            ConnectionString = connectionString;
            LogTableName = logTable;
            ApplicationName = applicationName;
            FieldMappings = mandatoryFieldMappings ?? InitialiseMandatoryFields();
            Adapter = CreateDataAdapter(connectionString, LogTableName, FieldMappings, out var schemaTable);
            LogTable = CreateLogTable(LogTableName, schemaTable, FieldMappings);
            Boolean delayPersistance = persistInterval > 0;
            Logger = InitialiseLogger(delayPersistance);
            Logger.PersistInterval = persistInterval;
            Logger.Enabled = true;
        }

        /// <summary>
        /// Initialises the mandatory fields.
        /// </summary>
        private static IDictionary<MandatoryLogField, string> InitialiseMandatoryFields()
        {
            IDictionary<MandatoryLogField, string> fieldMappings
                = new Dictionary<MandatoryLogField, string>
                      {
                          {MandatoryLogField.Source, MandatoryLogField.Source.ToString()},
                          {MandatoryLogField.Severity, "LogLevelId"}, 
                          {MandatoryLogField.Description, MandatoryLogField.Description.ToString()},
                          {MandatoryLogField.User, MandatoryLogField.User.ToString()},
                          {MandatoryLogField.LogDateTime, "Time"}
                      };
            return fieldMappings;
        }

        /// <summary>
        /// Initialises the logger.
        /// </summary>
        /// <param name="delayPersistance"></param>
        private DatabaseLog InitialiseLogger(Boolean delayPersistance)
        {
            var logging = new DataSet();
            logging.Tables.Add(LogTable);
            var logger = new DatabaseLog(delayPersistance)
                                     {
                                         ActiveTable = LogTable,
                                         Adapter = Adapter
                                     };
            return logger;
        }

        /// <summary>
        /// Called when [write].
        /// </summary>
        /// <param name="severity">The severity.</param>
        /// <param name="text">The text.</param>
        protected override void OnWrite(int severity, string text)
        {
            Add(ApplicationName, severity, text);
            if (!Logger.DelayPersistance)
                Logger.Flush();
        }


        /// <summary>
        /// Adds to the log table
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="text">The description.</param>
        private void Add(string source, int severity, string text)
        {
            DataRow row = LogTable.NewRow();
            SetRowValue(row, MandatoryLogField.Source, source, source);
            SetRowValue(row, MandatoryLogField.Severity, severity, source);
            SetRowValue(row, MandatoryLogField.Description, text, source);
            var windowsIdentity = WindowsIdentity.GetCurrent();
            SetRowValue(row, MandatoryLogField.User, windowsIdentity.Name, source);
            SetRowValue(row, MandatoryLogField.LogDateTime, DateTime.UtcNow, source);
            LogTable.Rows.Add(row);
        }

        private void SetRowValue(DataRow row, MandatoryLogField field, object value, string source)
        {
            try
            {
                row[FieldMappings[field]] = value;
            }
            catch (Exception ex)
            {
                string message =
                    $"Mandatory log field {field} cannot be resolved. Please ensure this field has been correctly mapped int mandatory log fields prior to creating the logger";
                WriteFailure(source, message, ex);
            }
        }

        /// <summary>
        /// Creates the data adapter.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName"></param>
        /// <param name="fieldMappings"></param>
        /// <param name="schemaTable"></param>
        /// <returns></returns>
        private SqlDataAdapter CreateDataAdapter(string connectionString, string tableName, IDictionary<MandatoryLogField, string> fieldMappings, out DataTable schemaTable)
        {
            var connection = new SqlConnection(connectionString);
            var adapter = new SqlDataAdapter();
            schemaTable = GetSchemaTable(connectionString, tableName);
            SqlCommand cmd = GenerateInsertCommand(schemaTable, connection, fieldMappings);
            adapter.InsertCommand = cmd;
            return adapter;
        }

        /// <summary>
        /// Gets the schema table.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="tableName">Name of the table.</param>
        /// <returns></returns>
        static DataTable GetSchemaTable(string connectionString, string tableName)
        {
            DataTable schemaTable;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand($"Select top 1 * from {tableName} where 1=2")
                              {Connection = connection};
                SqlDataReader rdr = cmd.ExecuteReader();
                schemaTable = rdr.GetSchemaTable();
                connection.Close();
            }
            return schemaTable;
        }

        /// <summary>
        /// Generates the insert command.
        /// </summary>
        /// <param name="schemaTable">The schema table.</param>
        /// <param name="connection">The connection.</param>
        /// <param name="fieldMappings"></param>
        /// <returns></returns>
        private SqlCommand GenerateInsertCommand(DataTable schemaTable, SqlConnection connection, IDictionary<MandatoryLogField, string> fieldMappings)
        {
            var parameters = new List<Triplet<string, SqlDbType, int>>();
            foreach (DataRow row in schemaTable.Rows)
            {
                if (!Convert.ToBoolean(row["IsAutoIncrement"]))
                {
                    string columnName = row["BaseColumnName"].ToString();
                    if (fieldMappings.Values.Contains(columnName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        var dataType = (SqlDbType)Enum.Parse(typeof(SqlDbType), row["DataTypeName"].ToString(), true);
                        int fieldSize = Convert.ToInt32(row["ColumnSize"]);
                        parameters.Add(new Triplet<string, SqlDbType, int>(columnName, dataType, fieldSize));
                    }
                }
            }
            string[] names = parameters.Select(item => item.First).ToArray();
            SqlDbType[] dataTypes = parameters.Select(item => item.Second).ToArray();
            int[] size = parameters.Select(item => item.Third).ToArray();
            string insertCommand =
                $"INSERT INTO dbo.{LogTableName} ( {"[" + string.Join("],[", names) + "]"} ) VALUES ( {"@" + string.Join(", @", names)} )";
            var cmd = new SqlCommand(insertCommand, connection);
            int index = 0;
            foreach (string name in names)
            {
                cmd.Parameters.Add($"@{name}", dataTypes[index], size[index], name);
                index++;
            }
            return cmd;
        }

        /// <summary>
        /// Creates the log table.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <param name="schemaTable">The schema table.</param>
        /// <param name="fieldMappings"></param>
        /// <returns></returns>
        private static DataTable CreateLogTable(string tableName, DataTable schemaTable, IDictionary<MandatoryLogField, string> fieldMappings)
        {
            var logDataTable = new DataTable(tableName);
            foreach (DataRow row in schemaTable.Rows)
            {
                if (!Convert.ToBoolean(row["IsAutoIncrement"]))
                {
                    string columnName = row["BaseColumnName"].ToString();
                    if (fieldMappings.Values.Contains(columnName, StringComparer.InvariantCultureIgnoreCase))
                    {
                        var dataColumn
                            = new DataColumn
                                  {
                                      DataType = Type.GetType(row["DataType"].ToString()),
                                      ColumnName = row["BaseColumnName"].ToString()
                                  };
                        logDataTable.Columns.Add(dataColumn);
                    }
                }
            }
            return logDataTable;
        }

        /// <summary>
        /// Writes the failure.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="message">What to enter.</param>
        /// <param name="exception">The exception.</param>
        private static void WriteFailure(string source, string message, Exception exception)
        {
            System.Diagnostics.Trace.WriteLine(string.Concat(" Database logging failure. Source: ", source, " Message: ", message, " Exception:", exception.Message));
        }
    }
}