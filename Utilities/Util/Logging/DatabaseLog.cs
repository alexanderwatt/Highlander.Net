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
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;

namespace Orion.Util.Logging
{
    /// <summary>
    /// Caches updates to a table and then periodically flushes it to the database
    /// </summary>
    [DefaultProperty("ActiveTable")]
    internal sealed class DatabaseLog
    {
        public Boolean DelayPersistance { get; }

        /// <summary>
        /// Holds the data table that is used for keeping the log
        /// </summary>
        private DataTable _activeTable;

        /// <summary>
        /// Holds a reference to the supplied adapter for executing the INSERT
        /// </summary>
        private SqlDataAdapter _adapter;

        /// <summary>
        /// Maximum number of rows allowed in cache
        /// </summary>
        private long _maxQueueLen = 1000;

        /// <summary>
        /// Flag that indicates that the cache is being flushed to the database
        /// </summary>
        private bool _persisting;

        /// <summary>
        /// Timer used to fire an event when the log is to be persisted
        /// </summary>
        private System.Timers.Timer _timer;

        /// <summary>
        /// Flag that indicates whether the log is enabled
        /// </summary>
        private bool _enabled;

        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseLog(Boolean delayPersistance)
        {
            DelayPersistance = delayPersistance;
            InitializeComponent();
        }
        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            _timer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(_timer)).BeginInit();
            // 
            // timer
            // 
            _timer.Interval = 100;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            ((System.ComponentModel.ISupportInitialize)(_timer)).EndInit();
            _timer.Enabled = DelayPersistance;

        }
        #endregion
        /// <summary>
        /// Gets/sets the state of the logger (true = Capturing info to database).
        /// If set to false, log will attempt to flush the cache to the database.
        /// </summary>
        [Category("Behavior"), DefaultValue(false)]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                // Turn on/off the timer and if turning off, make sure cache is flushed
                if (value != _enabled)
                {
                    // When turning on, ensure table and adapter are set
                    if (value)
                    {
                        if (_activeTable == null)
                            throw new ArgumentException("ActiveTable must be set before enabling the log");
                        if (_adapter == null)
                            throw new ArgumentException("Adapter must be set before enabling the log");
                    }
                    _timer.Enabled = DelayPersistance;
                    _enabled = value;
                    if (!_enabled) Flush();
                }
            }
        }

        /// <summary>
        /// Gets or sets the time, in milliseconds, between database flushes
        /// </summary>
        [Category("Behavior"), DefaultValue(300000)]
        public double PersistInterval
        {
            get => _timer.Enabled ? _timer.Interval : 0;
            set
            {
                if (_timer.Enabled)
                    _timer.Interval = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of rows to cache. Initially set to 1000.
        /// </summary>
        [Category("Behavior"), DefaultValue(1000)]
        public long MaxQueueLength
        {
            get => _maxQueueLen;
            set => _maxQueueLen = value;
        }

        /// <summary>
        /// Gets or sets the adapter used to persist information from the table to the database.
        /// You must have the SelectCommand and InsertCommand complete.
        /// </summary>
        [Category("Data")]
        public SqlDataAdapter Adapter
        {
            get => _adapter;
            set
            {
                if (value.InsertCommand == null || value.InsertCommand.CommandText.Length == 0 || value.InsertCommand.Connection == null)
                    throw new ArgumentNullException(nameof(value), "Adapter must have a valid InsertCommand with an active Connection");
                _adapter = value;
            }
        }

        /// <summary>
        /// Gets or sets the table that is used to cache the log writes
        /// </summary>
        [Category("Data")]
        public DataTable ActiveTable
        {
            get => _activeTable;
            set => _activeTable = value;
        }

        /// <summary>
        /// Try to persist information on timer interval
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void OnTimer(object source, System.Timers.ElapsedEventArgs args)
        {
            Flush();
        }

        /// <summary>
        /// If log is enabled and not currently writing to the database, flushes cache to database
        /// </summary>
        public void Flush()
        {
            if (_enabled && !_persisting)
            {
                _persisting = true;
                Persist();
                _persisting = false;
            }
        }

        /// <summary>
        /// Adds a row to the cache if logger is enabled and max size is not exceeded
        /// </summary>
        /// <param name="row">Row of information to add to <see cref="ActiveTable"/></param>
        /// <returns>Success</returns>
        public bool Write(DataRow row)
        {
            if (_enabled && PersistInterval > 0)
            {
                if (_activeTable.Rows.Count < _maxQueueLen)
                {
                    if (System.Threading.Monitor.TryEnter(_activeTable, 3000))
                    {
                        _activeTable.Rows.Add(row);
                        System.Threading.Monitor.Exit(_activeTable);
                        return true;
                    }
                }
                else
                    System.Diagnostics.Trace.WriteLine(string.Concat("Warning: Log cache for ", _activeTable.TableName, " has exceeded its maximum capacity"));
            }
            return false;
        }

        /// <summary>
        /// Flushes cache to the the database defined by <see cref="Adapter"/>
        /// </summary>
        private void Persist()
        {
            System.Threading.Monitor.Enter(_activeTable);
            try
            {
                if (_activeTable.DataSet.HasChanges())
                {
                    if (_adapter.InsertCommand.Connection.State == ConnectionState.Closed)
                        _adapter.InsertCommand.Connection.Open();

                    _adapter.InsertCommand.Transaction = _adapter.InsertCommand.Connection.BeginTransaction();
                    try
                    {
                        _adapter.Update(_activeTable);
                        _adapter.InsertCommand.Transaction.Commit();
                        _activeTable.Clear();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Trace.WriteLine(e.Message);
                        _adapter.InsertCommand.Transaction.Rollback();
                    }
                    finally
                    {
                        _adapter.InsertCommand.Connection.Close();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
            }
            finally
            {
                System.Threading.Monitor.Exit(_activeTable);
            }
        }
    }
}