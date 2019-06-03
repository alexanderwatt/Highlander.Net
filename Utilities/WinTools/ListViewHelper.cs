using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

using Orion.Utility.Logging;
using Orion.Utility.Threading;
using Orion.Utility.Caching;

namespace Orion.UI.WinTools
{
    public interface IFilterGroup
    {
        bool EvaluateFilter(int column, string value);
        void AddFilterValue(int column, string value);
        void SetColumnSize(int column, int left, int width);
    }

    public class ViewChangeNotification<TData>
    {
        public CacheChange Change;
        public TData OldData;
        public TData NewData;
    }

    public delegate void ViewChangeHandler<TData>(CacheChange change, TData oldData, TData newData);

    public interface ISelecter<TData>
    {
        bool Select(TData x);
    }

    public interface IViewHelper
    {
        int ColumnCount { get; }
        string GetColumnTitle(int column);
        bool IsFilterColumn(int column);
        HorizontalAlignment GetColumnAlignment(int column);
    }

    public interface IDataHelper<TData>
    {
        string GetUniqueKey(TData data);
        string GetDisplayValue(TData data, int column);
    }

    public class BaseSelecter<TData> : ISelecter<TData>
    {
        protected readonly IFilterGroup _FilterValues;
        protected readonly IViewHelper _ViewHelper;
        protected readonly IDataHelper<TData> _DataHelper;
        public BaseSelecter(
            IFilterGroup filterValues,
            IViewHelper viewHelper,
            IDataHelper<TData> dataHelper)
        {
            if (filterValues == null)
                throw new ArgumentNullException("filterControls");
            if (viewHelper == null)
                throw new ArgumentNullException("viewHelper");
            if (dataHelper == null)
                throw new ArgumentNullException("dataHelper");
            _FilterValues = filterValues;
            _ViewHelper = viewHelper;
            _DataHelper = dataHelper;
        }

        protected virtual bool OnSelect(TData data) { return true; }
        protected virtual bool OnSelectColumn(TData data, int column) { return true; }

        private string GetDisplayValueHelper(TData data, int column)
        {
            string cellValue;
            try
            {
                cellValue = _DataHelper.GetDisplayValue(data, column);
            }
            catch (Exception e)
            {
                cellValue = "(" + e.GetType().Name + ")";
            }
            return cellValue ?? "(null)";
        }

        public bool Select(TData data)
        {
            bool result = true;
            int column = 0;
            while (result && (column < _ViewHelper.ColumnCount))
            {
                if (_ViewHelper.IsFilterColumn(column))
                {
                    string cellValue = GetDisplayValueHelper(data, column);
                    result = result && _FilterValues.EvaluateFilter(column, cellValue);
                }
                // call optional override
                if (result)
                    result = result && this.OnSelectColumn(data, column);
                // next
                column++;
            } // while
            if (result)
                result = result && this.OnSelect(data);
            return result;
        }
    }

    public class WinFormsEventDispatcher
    {
        private readonly SynchronizationContext _SyncContext;
        private readonly SendOrPostCallback _Callback;
        private readonly bool _AggregateCallbacks;
        private int _QueuedEvents = 0;
        public int QueuedEvents { get { return _QueuedEvents; } }
        public WinFormsEventDispatcher(SendOrPostCallback callback, bool aggregateCallbacks)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            _SyncContext = WindowsFormsSynchronizationContext.Current;
            _Callback = callback;
            _AggregateCallbacks = aggregateCallbacks;
        }
        public void Post(object state)
        {
            if ((state != null) && _AggregateCallbacks)
                throw new ArgumentException("State must be null when aggregating callbacks", "state");
            Interlocked.Increment(ref _QueuedEvents);
            _SyncContext.Post(Recv, state);
        }
        private void Recv(object state)
        {
            int queuedEvents = Interlocked.Decrement(ref _QueuedEvents);
            if ((!_AggregateCallbacks) || (queuedEvents == 0))
            {
                // note: no try/catch wrapper required here as we are 
                // running on the main windows thread.
                _Callback(state);
            }
        }
    }

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

    public class ListViewManager<TData> : IListViewHelper<TData>
    {
        private readonly ILogger _Logger;
        private readonly ListView _View;
        //private readonly int _ColumnCount;
        private readonly ISelecter<TData> _DataSelecter;
        private readonly IFilterGroup _FilterGroup;
        private readonly IComparer<TData> _SortComparer;
        private readonly IViewHelper _ViewHelper;
        private readonly IDataHelper<TData> _DataHelper;

        private readonly WinFormsEventDispatcher _ReceiveRequestHandler;
        private readonly WinFormsEventDispatcher _CollateRequestHandler;
        private readonly WinFormsEventDispatcher _DisplayRequestHandler;

        private readonly Dictionary<string, TData> _OriginalData = new Dictionary<string, TData>();
        private readonly List<TData> _TransientData = new List<TData>();
        private readonly List<TData> _DisplayData = new List<TData>();
        public List<TData> DataItems
        {
            get { return new List<TData>(_DisplayData); }
        }
        public List<TData> SelectedDataItems
        {
            get
            {
                List<TData> result = new List<TData>();
                foreach (int index in _View.SelectedIndices)
                    result.Add(_DisplayData[index]);
                return result;
            }
        }
        public List<TData> CheckedDataItems
        {
            get
            {
                List<TData> result = new List<TData>();
                foreach (int index in _View.CheckedIndices)
                    result.Add(_DisplayData[index]);
                return result;
            }
        }
        public TData GetDataItem(int index)
        {
            return _DisplayData[index];
        }

        public ListViewManager(
            ILogger logger,
            ListView view,
            IViewHelper viewHelper,
            ISelecter<TData> dataSelecter,
            IFilterGroup filterGroup,
            IComparer<TData> sortComparer,
            IDataHelper<TData> dataHelper)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (view == null)
                throw new ArgumentNullException("view");
            if (viewHelper == null)
                throw new ArgumentNullException("viewHelper");
            if (dataHelper == null)
                throw new ArgumentNullException("dataHelper");

            _Logger = logger;
            _View = view;
            _DataSelecter = dataSelecter;
            _FilterGroup = filterGroup;
            _SortComparer = sortComparer;
            _ViewHelper = viewHelper;
            _DataHelper = dataHelper;

            _ReceiveRequestHandler = new WinFormsEventDispatcher(Exec_Receive_Request, false);
            _CollateRequestHandler = new WinFormsEventDispatcher(Exec_Collate_Request, true);
            _DisplayRequestHandler = new WinFormsEventDispatcher(Exec_Display_Request, true);

            // configure view
            _View.VirtualMode = true;
            _View.FullRowSelect = true;
            _View.CheckBoxes = false;
            _View.View = View.Details;
            _View.Columns.Clear();
            for (int column = 0; column < viewHelper.ColumnCount; column++)
            {
                ColumnHeader hdr = new ColumnHeader();
                hdr.Text = _ViewHelper.GetColumnTitle(column);
                hdr.TextAlign = _ViewHelper.GetColumnAlignment(column);
                _View.Columns.Add(hdr);
            }
            _View.RetrieveVirtualItem += new RetrieveVirtualItemEventHandler(_View_RetrieveVirtualItem);
            _View.CacheVirtualItems += new CacheVirtualItemsEventHandler(_View_CacheVirtualItems);
            _View.ColumnWidthChanged += new ColumnWidthChangedEventHandler(_View_ColumnWidthChanged);

            // init filter group column positions
            _View_ColumnWidthChanged(null, null);

        }

        void _View_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
        {
            int left = 0;
            for (int column = 0; column < _View.Columns.Count; column++)
            {
                int width = _View.Columns[column].Width;
                if (_FilterGroup != null)
                {
                    _FilterGroup.SetColumnSize(column, left, width);
                }
                left += width;
            }
        }

        private ListViewItem[] _ListViewItemCache;
        private int _ListViewItemCacheFirst;

        private string GetDisplayValueHelper(TData data, int column)
        {
            string cellValue;
            try
            {
                cellValue = _DataHelper.GetDisplayValue(data, column);
            }
            catch (Exception e)
            {
                cellValue = "(" + e.GetType().Name + ")";
            }
            return cellValue ?? "(null)";
        }

        private ListViewItem GetListItem(int i)
        {
            TData data = _DisplayData[i];
            ListViewItem result = new ListViewItem(GetDisplayValueHelper(data, 0));
            for (int column = 1; column < _ViewHelper.ColumnCount; column++)
                result.SubItems.Add(GetDisplayValueHelper(data, column));
            return result;
        }

        void _View_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            // Only recreate the cache if we need to.
            if (_ListViewItemCache != null &&
                e.StartIndex >= _ListViewItemCacheFirst &&
                e.EndIndex <= _ListViewItemCacheFirst + _ListViewItemCache.Length)
                return;

            _ListViewItemCacheFirst = e.StartIndex;
            int length = e.EndIndex - e.StartIndex + 1;

            _ListViewItemCache = new ListViewItem[length];
            for (int i = 0; i < _ListViewItemCache.Length; i++)
            {
                _ListViewItemCache[i] = GetListItem(_ListViewItemCacheFirst + i);
            }
        }

        void _View_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            // If we have the item cached, return it. Otherwise, recreate it.
            if (_ListViewItemCache != null &&
                e.ItemIndex >= _ListViewItemCacheFirst &&
                e.ItemIndex < _ListViewItemCacheFirst + _ListViewItemCache.Length)
            {
                e.Item = _ListViewItemCache[e.ItemIndex - _ListViewItemCacheFirst];
            }
            else
            {
                e.Item = GetListItem(e.ItemIndex);
            }
        }

        public void Clear()
        {
            _OriginalData.Clear();
            RebuildView();
        }
        public void RebuildView()
        {
            _CollateRequestHandler.Post(null);
        }

        public void UpdateData(ViewChangeNotification<TData> notification)
        {
            _ReceiveRequestHandler.Post(notification);
        }

        private void Exec_Display_Request(object notUsed)
        {
            // rebuild the display grid
            //_View.SuspendLayout();
            _DisplayData.Clear();
            foreach (TData item in _TransientData)
                _DisplayData.Add(item);
            _ListViewItemCache = null;
            _View.VirtualListSize = _DisplayData.Count;
            //_View.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //foreach (ColumnHeader item in _View.Columns)
            //{
            //    if (item.Width < 50)
            //        item.Width = 50;
            //    if (item.Width > 500)
            //        item.Width = 500;
            //}
            //_View.ResumeLayout();
            _View.Refresh();
        }

        private void Exec_Collate_Request(object state)
        {
            // re-select and re-sort items for display
            _TransientData.Clear();
            // - select
            foreach (var data in _OriginalData.Values)
            {
                bool include = true;
                if (_DataSelecter != null)
                    include = _DataSelecter.Select(data);
                else
                    include = true;

                if (include)
                    _TransientData.Add(data);
            }
            // - sort
            if (_SortComparer != null)
                _TransientData.Sort(_SortComparer);

            // next step
            _DisplayRequestHandler.Post(state);
        }

        private void Exec_Receive_Request(object state)
        {
            ViewChangeNotification<TData> notification = (ViewChangeNotification<TData>)state;

            TData oldData = (TData)notification.OldData;
            TData newData = (TData)notification.NewData;

            // check for clear notification
            if (notification.Change == CacheChange.CacheCleared)
            {
                _OriginalData.Clear();
            }

            // process other notifications
            if (oldData != null)
            {
                // delete
                string oldKey = _DataHelper.GetUniqueKey(oldData);
                if (oldKey != null)
                    _OriginalData.Remove(oldKey);
            }
            if (newData != null)
            {
                // create/update
                string newKey = _DataHelper.GetUniqueKey(newData);
                if (newKey != null)
                    _OriginalData[newKey] = newData;
            }
            // optimisation - todo - only re-collate if item has actually changed

            // update filters
            if ((_FilterGroup != null) && (newData != null))
            {
                for (int column = 0; column < _ViewHelper.ColumnCount; column++)
                    _FilterGroup.AddFilterValue(column, GetDisplayValueHelper(newData, column));
            }

            // rebuild view
            _CollateRequestHandler.Post(null);
        }
    }

    public class ComboxBoxFilterGroup : IFilterGroup
    {
        private readonly Panel _ParentPanel;
        private readonly IViewHelper _ViewHelper;
        private readonly ComboBox[] _ComboBoxes;

        public ComboxBoxFilterGroup(
            Panel parentPanel, 
            IViewHelper viewHelper,
            EventHandler selectionChangedHandler)
        {
            if (parentPanel == null)
                throw new ArgumentNullException("parentPanel");
            if (viewHelper == null)
                throw new ArgumentNullException("viewHelper");
            _ParentPanel = parentPanel;
            _ViewHelper = viewHelper;

            // build UI
            // - fix parent panel if required
            if (_ParentPanel.Height < 32)
                _ParentPanel.Height = 32;

            // - reset filter button
            Button resetButton = new Button();
            _ParentPanel.Controls.Add(resetButton);
            resetButton.Text = "Reset filters";
            resetButton.Width = 75;
            resetButton.Left = _ParentPanel.Width - resetButton.Width - 5;
            resetButton.Height = 23;
            resetButton.Top = _ParentPanel.Height - resetButton.Height - 3;
            resetButton.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            resetButton.Click += new EventHandler(clearButton_Click);

            // - bind user-defined combos to column numbers
            _ComboBoxes = new ComboBox[viewHelper.ColumnCount];
            for (int i = 0; i < viewHelper.ColumnCount; i++)
            {
                foreach (Control control in _ParentPanel.Controls)
                {
                    if (control is ComboBox)
                    {
                        ComboBox combo = (ComboBox)control;
                        if ((combo.Tag is int) && ((int)(combo.Tag) == i))
                        {
                            // found - bind it
                            _ComboBoxes[i] = combo;
                        }
                    }
                }
            }
            // - create other required combos
            for (int i = 0; i < viewHelper.ColumnCount; i++)
            {
                ComboBox combo = _ComboBoxes[i];
                if ((combo == null) && _ViewHelper.IsFilterColumn(i))
                {
                    combo = new ComboBox();
                    _ParentPanel.Controls.Add(combo);
                    combo.Width = 49;
                    combo.Left = _ParentPanel.Width - combo.Width - 5;
                    combo.Height = 21;
                    combo.Top = _ParentPanel.Height - combo.Height - 3;
                    combo.Tag = i;
                    _ComboBoxes[i] = combo;
                }
            }
            // init all combos
            foreach (ComboBox combo in _ComboBoxes)
            {
                if (combo != null)
                {
                    combo.Items.Clear();
                    combo.Items.Add("(all)");
                    combo.SelectedIndex = 0;
                    combo.SelectedIndexChanged += selectionChangedHandler;
                    combo.Top = _ParentPanel.Height - combo.Height - 3;
                }
            }
        }

        public bool EvaluateFilter(int column, string value)
        {
            if ((column < 0) || (column >= _ComboBoxes.Length))
                return true;

            value = value ?? "(null)";

            ComboBox combo = _ComboBoxes[column];
            if ((combo != null) && (combo.SelectedIndex != 0))
                return (String.Compare(value, combo.Text, true) == 0);
            else
                return true;
        }

        public string GetFilterValue(int column)
        {
            if ((column < 0) || (column >= _ComboBoxes.Length))
                return null;

            ComboBox combo = _ComboBoxes[column];
            if ((combo != null) && (combo.SelectedIndex != 0))
                return combo.Text;
            else
                return null;
        }

        public void AddFilterValue(int column, string value)
        {
            if ((column < 0) || (column >= _ComboBoxes.Length))
                return;

            value = value ?? "(null)";

            ComboBox combo = _ComboBoxes[column];
            if (combo != null)
            {
                if (!combo.Items.Contains(value))
                    combo.Items.Add(value);
            }
        }

        public void SetColumnSize(int column, int left, int width)
        {
            if ((column < 0) || (column >= _ComboBoxes.Length))
                return;

            ComboBox combo = _ComboBoxes[column];
            if (combo != null)
            {
                combo.Left = left + 1;
                combo.Width = width - 2;
            }
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            foreach (ComboBox combo in _ComboBoxes)
            {
                if (combo != null)
                {
                    combo.SelectedIndex = 0;
                }
            }
        }
    }

}
