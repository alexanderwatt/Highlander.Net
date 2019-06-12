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
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Orion.Util.Caching;
using Orion.Util.Logging;

namespace Orion.Util.WinTools
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFilterGroup
    {
        bool EvaluateFilter(int column, string value);
        void AddFilterValue(int column, string value);
        void SetColumnSize(int column, int left, int width);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class ViewChangeNotification<TData>
    {
        public CacheChange Change;
        public TData OldData;
        public TData NewData;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="change"></param>
    /// <param name="oldData"></param>
    /// <param name="newData"></param>
    public delegate void ViewChangeHandler<in TData>(CacheChange change, TData oldData, TData newData);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface ISelecter<in TData>
    {
        bool Select(TData x);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface IViewHelper
    {
        int ColumnCount { get; }
        string GetColumnTitle(int column);
        bool IsFilterColumn(int column);
        HorizontalAlignment GetColumnAlignment(int column);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IDataHelper<in TData>
    {
        string GetUniqueKey(TData data);
        string GetDisplayValue(TData data, int column);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class BaseSelecter<TData> : ISelecter<TData>
    {
        protected readonly IFilterGroup FilterValues;
        protected readonly IViewHelper ViewHelper;
        protected readonly IDataHelper<TData> DataHelper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterValues"></param>
        /// <param name="viewHelper"></param>
        /// <param name="dataHelper"></param>
        public BaseSelecter(
            IFilterGroup filterValues,
            IViewHelper viewHelper,
            IDataHelper<TData> dataHelper)
        {
            FilterValues = filterValues ?? throw new ArgumentNullException(nameof(filterValues));
            ViewHelper = viewHelper ?? throw new ArgumentNullException(nameof(viewHelper));
            DataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
        }

        protected virtual bool OnSelect(TData data) { return true; }
        protected virtual bool OnSelectColumn(TData data, int column) { return true; }

        private string GetDisplayValueHelper(TData data, int column)
        {
            string cellValue;
            try
            {
                cellValue = DataHelper.GetDisplayValue(data, column);
            }
            catch (Exception e)
            {
                cellValue = "(" + e.GetType().Name + ")";
            }
            return cellValue ?? "(null)";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool Select(TData data)
        {
            bool result = true;
            int column = 0;
            while (result && column < ViewHelper.ColumnCount)
            {
                if (ViewHelper.IsFilterColumn(column))
                {
                    string cellValue = GetDisplayValueHelper(data, column);
                    result = FilterValues.EvaluateFilter(column, cellValue);
                }
                // call optional override
                if (result)
                    result = OnSelectColumn(data, column);
                // next
                column++;
            } // while
            if (result)
                result = OnSelect(data);
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class WinFormsEventDispatcher
    {
        private readonly SynchronizationContext _syncContext;
        private readonly SendOrPostCallback _callback;
        private readonly bool _aggregateCallbacks;
        private int _queuedEvents;
        public int QueuedEvents => _queuedEvents;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="aggregateCallbacks"></param>
        public WinFormsEventDispatcher(SendOrPostCallback callback, bool aggregateCallbacks)
        {
            _syncContext = SynchronizationContext.Current;
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _aggregateCallbacks = aggregateCallbacks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        public void Post(object state)
        {
            if (state != null && _aggregateCallbacks)
                throw new ArgumentException("State must be null when aggregating callbacks", nameof(state));
            Interlocked.Increment(ref _queuedEvents);
            _syncContext.Post(Recv, state);
        }

        private void Recv(object state)
        {
            int queuedEvents = Interlocked.Decrement(ref _queuedEvents);
            if (!_aggregateCallbacks || queuedEvents == 0)
            {
                // note: no try/catch wrapper required here as we are 
                // running on the main windows thread.
                _callback(state);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public interface IListViewHelper<TData>
    {
        void Clear();
        void RebuildView();
        //void UpdateData(TData oldData, TData newData);
        void UpdateData(ViewChangeNotification<TData> notification);
        List<TData> DataItems { get; }
        List<TData> SelectedDataItems { get; }
        List<TData> CheckedDataItems { get; }
        TData GetDataItem(int index);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class ListViewManager<TData> : IListViewHelper<TData>
    {
        private readonly ListView _view;

        //private readonly int _ColumnCount;
        private readonly ISelecter<TData> _dataSelecter;
        private readonly IFilterGroup _filterGroup;
        private readonly IComparer<TData> _sortComparer;
        private readonly IViewHelper _viewHelper;
        private readonly IDataHelper<TData> _dataHelper;

        private readonly WinFormsEventDispatcher _receiveRequestHandler;
        private readonly WinFormsEventDispatcher _collateRequestHandler;
        private readonly WinFormsEventDispatcher _displayRequestHandler;

        private readonly Dictionary<string, TData> _originalData = new Dictionary<string, TData>();
        private readonly List<TData> _transientData = new List<TData>();
        private readonly List<TData> _displayData = new List<TData>();

        /// <summary>
        /// 
        /// </summary>
        public List<TData> DataItems => new List<TData>(_displayData);

        /// <summary>
        /// 
        /// </summary>
        public List<TData> SelectedDataItems => (from int index in _view.SelectedIndices select _displayData[index]).ToList();

        /// <summary>
        /// 
        /// </summary>
        public List<TData> CheckedDataItems => (from int index in _view.CheckedIndices select _displayData[index]).ToList();

        /// <summary>
        /// 
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public TData GetDataItem(int index)
        {
            return _displayData[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="view"></param>
        /// <param name="viewHelper"></param>
        /// <param name="dataSelecter"></param>
        /// <param name="filterGroup"></param>
        /// <param name="sortComparer"></param>
        /// <param name="dataHelper"></param>
        public ListViewManager(
            ILogger logger,
            ListView view,
            IViewHelper viewHelper,
            ISelecter<TData> dataSelecter,
            IFilterGroup filterGroup,
            IComparer<TData> sortComparer,
            IDataHelper<TData> dataHelper)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            //
            //Set the properties.
            _dataSelecter = dataSelecter;
            _filterGroup = filterGroup;
            _sortComparer = sortComparer;
            _viewHelper = viewHelper ?? throw new ArgumentNullException(nameof(viewHelper));
            _dataHelper = dataHelper ?? throw new ArgumentNullException(nameof(dataHelper));
            //
            //Updating
            _receiveRequestHandler = new WinFormsEventDispatcher(ExecReceiveRequest, false);
            _collateRequestHandler = new WinFormsEventDispatcher(ExecCollateRequest, true);
            _displayRequestHandler = new WinFormsEventDispatcher(ExecDisplayRequest, true);
            //
            // configure view
            _view.VirtualMode = true;
            _view.FullRowSelect = true;
            _view.CheckBoxes = false;
            _view.View = View.Details;
            _view.Columns.Clear();
            for (int column = 0; column < viewHelper.ColumnCount; column++)
            {
                var hdr = new ColumnHeader
                              {
                                  Text = _viewHelper.GetColumnTitle(column),
                                  TextAlign = _viewHelper.GetColumnAlignment(column)
                              };
                _view.Columns.Add(hdr);
            }
            _view.RetrieveVirtualItem += ViewRetrieveVirtualItem;
            _view.CacheVirtualItems += ViewCacheVirtualItems;
            _view.ColumnWidthChanged += ViewColumnWidthChanged;
            //
            // init filter group column positions
            ViewColumnWidthChanged(null, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="view"></param>
        /// <param name="viewHelper"></param>
        /// <param name="dataSelecter"></param>
        /// <param name="filterGroup"></param>
        /// <param name="sortComparer"></param>
        /// <param name="dataHelper"></param>
        /// <param name="gridlines"></param>
        /// <param name="color"></param>
        public ListViewManager(
            ILogger logger,
            ListView view,
            IViewHelper viewHelper,
            ISelecter<TData> dataSelecter,
            IFilterGroup filterGroup,
            IComparer<TData> sortComparer,
            IDataHelper<TData> dataHelper, 
            bool gridlines,
            Color color): 
            this(logger, view, viewHelper, dataSelecter, filterGroup, sortComparer, dataHelper)
        {
            _view.GridLines = gridlines;
            _view.BackColor = color;
        }

        void ViewColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            int left = 0;
            for (int column = 0; column < _view.Columns.Count; column++)
            {
                int width = _view.Columns[column].Width;
                _filterGroup?.SetColumnSize(column, left, width);
                left += width;
            }
        }

        private ListViewItem[] _listViewItemCache;
        private int _listViewItemCacheFirst;

        private string GetDisplayValueHelper(TData data, int column)
        {
            string cellValue;
            try
            {
                cellValue = _dataHelper.GetDisplayValue(data, column);
            }
            catch (Exception e)
            {
                cellValue = "(" + e.GetType().Name + ")";
            }
            return cellValue ?? "(null)";
        }

        private ListViewItem GetListItem(int i)
        {
            TData data = _displayData[i];
            var result = new ListViewItem(GetDisplayValueHelper(data, 0));
            for (int column = 1; column < _viewHelper.ColumnCount; column++)
                result.SubItems.Add(GetDisplayValueHelper(data, column));
            return result;
        }

        void ViewCacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            // Only recreate the cache if we need to.
            if (_listViewItemCache != null &&
                e.StartIndex >= _listViewItemCacheFirst &&
                e.EndIndex <= _listViewItemCacheFirst + _listViewItemCache.Length)
                return;

            _listViewItemCacheFirst = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1;

            _listViewItemCache = new ListViewItem[length];
            for (int i = 0; i < _listViewItemCache.Length; i++)
            {
                _listViewItemCache[i] = GetListItem(_listViewItemCacheFirst + i);
            }
        }

        void ViewRetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // If we have the item cached, return it. Otherwise, recreate it.
            if (_listViewItemCache != null &&
                e.ItemIndex >= _listViewItemCacheFirst &&
                e.ItemIndex < _listViewItemCacheFirst + _listViewItemCache.Length)
            {
                e.Item = _listViewItemCache[e.ItemIndex - _listViewItemCacheFirst];
            }
            else
            {
                e.Item = GetListItem(e.ItemIndex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _originalData.Clear();
            RebuildView();
        }

        /// <summary>
        /// 
        /// </summary>
        public void RebuildView()
        {
            _collateRequestHandler.Post(null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="notification"></param>
        public void UpdateData(ViewChangeNotification<TData> notification)
        {
            _receiveRequestHandler.Post(notification);
        }

        private void ExecDisplayRequest(object notUsed)
        {
            // rebuild the display grid
            //_View.SuspendLayout();
            _displayData.Clear();
            foreach (TData item in _transientData)
            {
                _displayData.Add(item);
                //_view.SelectedItems.Add(item);
            }
            _listViewItemCache = null;
            _view.VirtualListSize = _displayData.Count;
            _view.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //foreach (ColumnHeader item in _View.Columns)
            //{
            //    if (item.Width < 50)
            //        item.Width = 50;
            //    if (item.Width > 500)
            //        item.Width = 500;
            //}
            //_View.ResumeLayout();
            _view.Refresh();
            //Play with the format
            //_view.GridLines = true;
            //_view.BackColor = Color.Azure;
        }

        private void ExecCollateRequest(object state)
        {
            // re-select and re-sort items for display
            _transientData.Clear();
            // - select
            foreach (var data in _originalData.Values)
            {
                bool include = _dataSelecter == null || _dataSelecter.Select(data);

                if (include)
                    _transientData.Add(data);
            }
            // - sort
            if (_sortComparer != null)
                _transientData.Sort(_sortComparer);

            // next step
            _displayRequestHandler.Post(state);
        }

        private void ExecReceiveRequest(object state)
        {
            var notification = (ViewChangeNotification<TData>)state;
            var oldData = notification.OldData;
            var newData = notification.NewData;
            // check for clear notification
            if (notification.Change == CacheChange.CacheCleared)
            {
                _originalData.Clear();
            }

            // process other notifications
            if (oldData != null)
            {
                // delete
                string oldKey = _dataHelper.GetUniqueKey(oldData);
                if (oldKey != null)
                    _originalData.Remove(oldKey);
            }
            if (newData != null)
            {
                // create/update
                string newKey = _dataHelper.GetUniqueKey(newData);
                if (newKey != null)
                    _originalData[newKey] = newData;
            }
            // optimisation - todo - only re-collate if item has actually changed
            // update filters
            if (_filterGroup != null && newData != null)
            {
                for (int column = 0; column < _viewHelper.ColumnCount; column++)
                    _filterGroup.AddFilterValue(column, GetDisplayValueHelper(newData, column));
            }
            // rebuild view
            _collateRequestHandler.Post(null);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ComboxBoxFilterGroup : IFilterGroup
    {
        private readonly ComboBox[] _comboBoxes;

        public ComboxBoxFilterGroup(
            Panel parentPanel, 
            IViewHelper viewHelper,
            EventHandler selectionChangedHandler)
        {
            var parentPanel1 = parentPanel ?? throw new ArgumentNullException(nameof(parentPanel));
            var viewHelper1 = viewHelper ?? throw new ArgumentNullException(nameof(viewHelper));

            // build UI
            // - fix parent panel if required
            if (parentPanel1.Height < 32)
                parentPanel1.Height = 32;
            // - reset filter button
            var resetButton = new Button();
            parentPanel1.Controls.Add(resetButton);
            resetButton.Text = "Reset filters";
            resetButton.Width = 75;
            resetButton.Left = parentPanel1.Width - resetButton.Width - 5;
            resetButton.Height = 23;
            resetButton.Top = parentPanel1.Height - resetButton.Height - 3;
            resetButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            resetButton.Click += ClearButtonClick;
            // - bind user-defined combos to column numbers
            _comboBoxes = new ComboBox[viewHelper.ColumnCount];
            for (int i = 0; i < viewHelper.ColumnCount; i++)
            {
                foreach (Control control in parentPanel1.Controls)
                {
                    var box = control as ComboBox;
                    var combo = box;
                    if (combo?.Tag is int i1 && i1 == i)
                    {
                        // found - bind it
                        _comboBoxes[i] = combo;
                    }
                }
            }
            // - create other required combos
            for (int i = 0; i < viewHelper.ColumnCount; i++)
            {
                ComboBox combo = _comboBoxes[i];
                if (combo == null && viewHelper1.IsFilterColumn(i))
                {
                    combo = new ComboBox();
                    parentPanel1.Controls.Add(combo);
                    combo.Width = 49;
                    combo.Left = parentPanel1.Width - combo.Width - 5;
                    combo.Height = 21;
                    combo.Top = parentPanel1.Height - combo.Height - 3;
                    combo.Tag = i;
                    _comboBoxes[i] = combo;
                }
            }
            // init all combos
            foreach (ComboBox combo in _comboBoxes)
            {
                if (combo != null)
                {
                    combo.Items.Clear();
                    combo.Items.Add("(all)");
                    combo.SelectedIndex = 0;
                    combo.SelectedIndexChanged += selectionChangedHandler;
                    combo.Top = parentPanel1.Height - combo.Height - 3;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool EvaluateFilter(int column, string value)
        {
            if (column < 0 || column >= _comboBoxes.Length)
                return true;
            value = value ?? "(null)";
            ComboBox combo = _comboBoxes[column];
            if (combo != null && combo.SelectedIndex != 0)
                return (String.Compare(value, combo.Text, StringComparison.OrdinalIgnoreCase) == 0);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public string GetFilterValue(int column)
        {
            if (column < 0 || column >= _comboBoxes.Length)
                return null;
            ComboBox combo = _comboBoxes[column];
            if (combo != null && (combo.SelectedIndex != 0))
                return combo.Text;
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public void AddFilterValue(int column, string value)
        {
            if (column < 0 || column >= _comboBoxes.Length)
                return;
            value = value ?? "(null)";
            ComboBox combo = _comboBoxes[column];
            if (combo == null) return;
            if (!combo.Items.Contains(value))
                combo.Items.Add(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="left"></param>
        /// <param name="width"></param>
        public void SetColumnSize(int column, int left, int width)
        {
            if (column < 0 || (column >= _comboBoxes.Length))
                return;
            ComboBox combo = _comboBoxes[column];
            if (combo == null) return;
            combo.Left = left + 1;
            combo.Width = width - 2;
        }

        private void ClearButtonClick(object sender, EventArgs e)
        {
            foreach (ComboBox combo in _comboBoxes)
            {
                if (combo != null)
                {
                    combo.SelectedIndex = 0;
                }
            }
        }
    }
}
