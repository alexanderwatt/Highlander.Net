/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/
#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;

#endregion

namespace Core.Server
{
    partial class ItemDataDataContext
    {
        // add extension methods here
    }

    // ItemData extensions
    /// <summary>
    /// 
    /// </summary>
    public partial class ItemData
    {
        internal ItemData(CommonItem row)
        {
            _ItemId = row.Id;
            _ItemName = row.Name;
            _ItemType = (int)row.ItemKind;
            if (row.AppProps != null)
                _AppProps = row.AppProps.Serialise();
            if (row.SysProps != null)
                _SysProps = row.SysProps.Serialise();
            _DataType = row.DataTypeName;
            _AppScope = row.AppScope;
            _NetScope = row.NetScope;
            _Created = row.Created.ToString("o");
            _Expires = row.Expires.ToString("o");
            if (row.YData != null)
                _YData = row.YData.ToArray();
            if (row.YSign != null)
                _YSign = row.YSign.ToArray();
            _StoreSRN = 1;
            _StoreUSN = row.StoreUSN;
        }
    }

    /// <summary>
    /// Represents a value for a given reference type
    /// </summary>
    internal class ItemData2Table
    {
        private readonly string _connectionString;
        //private ItemData2DataContext _DataContext;
        //private long _ExceptionCount = 0;
        //public long ExceptionCount { get { return _ExceptionCount; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemData"/> class.
        /// </summary>
        internal ItemData2Table(string connectionString)
        {
            _connectionString = connectionString;
            //_DataContext = new ItemData2DataContext(connectionString);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ItemData"/> class.
        ///// </summary>
        ///// <param name="dataContext">The data context.</param>
        //internal ItemData2Table(ItemDataDataContext dataContext)
        // {
        //}

        ///// <summary>
        ///// Finds an item with an id.
        ///// </summary>
        ///// <param name="itemId">The item id.</param>
        ///// <returns></returns>
        //internal ItemData SelectItem(Guid itemId)
        //{
        //    var query = _DataContext.ItemDatas.Where(item => (item.ItemId == itemId));

        //    List<ItemData> list = query.ToList();
        //    if (list.Count > 0)
        //        return list[0];
        //    else
        //        return null;
        //}

        /// <summary>
        /// Selects all items.
        /// </summary>
        /// <returns></returns>
        internal List<ItemData> SelectAllItems()
        {
            using (var dataContext = new ItemData2DataContext(_connectionString))
            {
                var query = dataContext.ItemDatas.Where(item => (true));
                return query.ToList();
            }
        }

        /// <summary>
        /// Inserts the item.
        /// </summary>
        /// <param name="newItem">The new item.</param>
        internal void InsertItem(CommonItem newItem)
        {
            using (var dataContext = new ItemData2DataContext(_connectionString))
            {
                dataContext.ItemDatas.InsertOnSubmit(new ItemData(newItem));
                dataContext.SubmitChanges();
            }
        }

        /// <summary>
        /// Deletes the item with the id.
        /// </summary>
        /// <param name="itemId">The item id.</param>
        internal void DeleteItem(Guid itemId)
        {
            string sqlCommand = $"DELETE FROM [ItemData] WHERE [ItemId] = '{itemId}'";
            using (var dataContext = new ItemData2DataContext(_connectionString))
            {
                dataContext.ExecuteCommand(sqlCommand);
            }

            // LINQ round-trip DELETE queries are too slow
            //var query = _DataContext.ItemDatas.Where(item => (item.ItemId == itemId));
            //_DataContext.ItemDatas.DeleteAllOnSubmit(query);
            //_DataContext.SubmitChanges();
        }
    }
}
