﻿#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Highlander.Core.Server
{
	using System.Data.Linq;
	using System.Data.Linq.Mapping;
	using System.Data;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Linq;
	using System.Linq.Expressions;
	using System.ComponentModel;
	using System;
	
	
	[global::System.Data.Linq.Mapping.DatabaseAttribute(Name="Core_DEV")]
	public partial class ItemData2DataContext : System.Data.Linq.DataContext
	{
		
		private static System.Data.Linq.Mapping.MappingSource mappingSource = new AttributeMappingSource();
		
    #region Extensibility Method Definitions
    partial void OnCreated();
    partial void InsertItemData(ItemData instance);
    partial void UpdateItemData(ItemData instance);
    partial void DeleteItemData(ItemData instance);
    #endregion
		
		public ItemData2DataContext() : 
				base(global::Highlander.Core.Server.Properties.Settings.Default.Core_DEVConnectionString, mappingSource)
		{
			OnCreated();
		}
		
		public ItemData2DataContext(string connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ItemData2DataContext(System.Data.IDbConnection connection) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ItemData2DataContext(string connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public ItemData2DataContext(System.Data.IDbConnection connection, System.Data.Linq.Mapping.MappingSource mappingSource) : 
				base(connection, mappingSource)
		{
			OnCreated();
		}
		
		public System.Data.Linq.Table<ItemData> ItemDatas
		{
			get
			{
				return this.GetTable<ItemData>();
			}
		}
	}
	
	[global::System.Data.Linq.Mapping.TableAttribute(Name="dbo.ItemData")]
	public partial class ItemData : INotifyPropertyChanging, INotifyPropertyChanged
	{
		
		private static PropertyChangingEventArgs emptyChangingEventArgs = new PropertyChangingEventArgs(String.Empty);
		
		private System.Guid _ItemId;
		
		private string _ItemName;
		
		private int _ItemType;
		
		private string _AppScope;
		
		private string _AppProps;
		
		private string _Created;
		
		private string _Expires;
		
		private string _DataType;
		
		private System.Data.Linq.Binary _YData;
		
		private System.Data.Linq.Binary _YSign;
		
		private string _NetScope;
		
		private string _SysProps;
		
		private int _StoreSRN;
		
		private long _StoreUSN;
		
    #region Extensibility Method Definitions
    partial void OnLoaded();
    partial void OnValidate(System.Data.Linq.ChangeAction action);
    partial void OnCreated();
    partial void OnItemIdChanging(System.Guid value);
    partial void OnItemIdChanged();
    partial void OnItemNameChanging(string value);
    partial void OnItemNameChanged();
    partial void OnItemTypeChanging(int value);
    partial void OnItemTypeChanged();
    partial void OnAppScopeChanging(string value);
    partial void OnAppScopeChanged();
    partial void OnAppPropsChanging(string value);
    partial void OnAppPropsChanged();
    partial void OnCreatedChanging(string value);
    partial void OnCreatedChanged();
    partial void OnExpiresChanging(string value);
    partial void OnExpiresChanged();
    partial void OnDataTypeChanging(string value);
    partial void OnDataTypeChanged();
    partial void OnYDataChanging(System.Data.Linq.Binary value);
    partial void OnYDataChanged();
    partial void OnYSignChanging(System.Data.Linq.Binary value);
    partial void OnYSignChanged();
    partial void OnNetScopeChanging(string value);
    partial void OnNetScopeChanged();
    partial void OnSysPropsChanging(string value);
    partial void OnSysPropsChanged();
    partial void OnStoreSRNChanging(int value);
    partial void OnStoreSRNChanged();
    partial void OnStoreUSNChanging(long value);
    partial void OnStoreUSNChanged();
    #endregion
		
		public ItemData()
		{
			OnCreated();
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ItemId", DbType="UniqueIdentifier NOT NULL", IsPrimaryKey=true)]
		public System.Guid ItemId
		{
			get
			{
				return this._ItemId;
			}
			set
			{
				if ((this._ItemId != value))
				{
					this.OnItemIdChanging(value);
					this.SendPropertyChanging();
					this._ItemId = value;
					this.SendPropertyChanged("ItemId");
					this.OnItemIdChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ItemName", DbType="NVarChar(255) NOT NULL", CanBeNull=false)]
		public string ItemName
		{
			get
			{
				return this._ItemName;
			}
			set
			{
				if ((this._ItemName != value))
				{
					this.OnItemNameChanging(value);
					this.SendPropertyChanging();
					this._ItemName = value;
					this.SendPropertyChanged("ItemName");
					this.OnItemNameChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_ItemType", DbType="Int NOT NULL")]
		public int ItemType
		{
			get
			{
				return this._ItemType;
			}
			set
			{
				if ((this._ItemType != value))
				{
					this.OnItemTypeChanging(value);
					this.SendPropertyChanging();
					this._ItemType = value;
					this.SendPropertyChanged("ItemType");
					this.OnItemTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AppScope", DbType="NVarChar(255)")]
		public string AppScope
		{
			get
			{
				return this._AppScope;
			}
			set
			{
				if ((this._AppScope != value))
				{
					this.OnAppScopeChanging(value);
					this.SendPropertyChanging();
					this._AppScope = value;
					this.SendPropertyChanged("AppScope");
					this.OnAppScopeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_AppProps", DbType="NVarChar(MAX)")]
		public string AppProps
		{
			get
			{
				return this._AppProps;
			}
			set
			{
				if ((this._AppProps != value))
				{
					this.OnAppPropsChanging(value);
					this.SendPropertyChanging();
					this._AppProps = value;
					this.SendPropertyChanged("AppProps");
					this.OnAppPropsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Created", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Created
		{
			get
			{
				return this._Created;
			}
			set
			{
				if ((this._Created != value))
				{
					this.OnCreatedChanging(value);
					this.SendPropertyChanging();
					this._Created = value;
					this.SendPropertyChanged("Created");
					this.OnCreatedChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_Expires", DbType="NVarChar(50) NOT NULL", CanBeNull=false)]
		public string Expires
		{
			get
			{
				return this._Expires;
			}
			set
			{
				if ((this._Expires != value))
				{
					this.OnExpiresChanging(value);
					this.SendPropertyChanging();
					this._Expires = value;
					this.SendPropertyChanged("Expires");
					this.OnExpiresChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_DataType", DbType="NVarChar(255)")]
		public string DataType
		{
			get
			{
				return this._DataType;
			}
			set
			{
				if ((this._DataType != value))
				{
					this.OnDataTypeChanging(value);
					this.SendPropertyChanging();
					this._DataType = value;
					this.SendPropertyChanged("DataType");
					this.OnDataTypeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_YData", DbType="Image", CanBeNull=true, UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary YData
		{
			get
			{
				return this._YData;
			}
			set
			{
				if ((this._YData != value))
				{
					this.OnYDataChanging(value);
					this.SendPropertyChanging();
					this._YData = value;
					this.SendPropertyChanged("YData");
					this.OnYDataChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_YSign", DbType="VarBinary(MAX)", CanBeNull=true, UpdateCheck=UpdateCheck.Never)]
		public System.Data.Linq.Binary YSign
		{
			get
			{
				return this._YSign;
			}
			set
			{
				if ((this._YSign != value))
				{
					this.OnYSignChanging(value);
					this.SendPropertyChanging();
					this._YSign = value;
					this.SendPropertyChanged("YSign");
					this.OnYSignChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_NetScope", DbType="NVarChar(255)")]
		public string NetScope
		{
			get
			{
				return this._NetScope;
			}
			set
			{
				if ((this._NetScope != value))
				{
					this.OnNetScopeChanging(value);
					this.SendPropertyChanging();
					this._NetScope = value;
					this.SendPropertyChanged("NetScope");
					this.OnNetScopeChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_SysProps", DbType="NVarChar(MAX)")]
		public string SysProps
		{
			get
			{
				return this._SysProps;
			}
			set
			{
				if ((this._SysProps != value))
				{
					this.OnSysPropsChanging(value);
					this.SendPropertyChanging();
					this._SysProps = value;
					this.SendPropertyChanged("SysProps");
					this.OnSysPropsChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreSRN", DbType="Int NOT NULL")]
		public int StoreSRN
		{
			get
			{
				return this._StoreSRN;
			}
			set
			{
				if ((this._StoreSRN != value))
				{
					this.OnStoreSRNChanging(value);
					this.SendPropertyChanging();
					this._StoreSRN = value;
					this.SendPropertyChanged("StoreSRN");
					this.OnStoreSRNChanged();
				}
			}
		}
		
		[global::System.Data.Linq.Mapping.ColumnAttribute(Storage="_StoreUSN", DbType="BigInt NOT NULL")]
		public long StoreUSN
		{
			get
			{
				return this._StoreUSN;
			}
			set
			{
				if ((this._StoreUSN != value))
				{
					this.OnStoreUSNChanging(value);
					this.SendPropertyChanging();
					this._StoreUSN = value;
					this.SendPropertyChanged("StoreUSN");
					this.OnStoreUSNChanged();
				}
			}
		}
		
		public event PropertyChangingEventHandler PropertyChanging;
		
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void SendPropertyChanging()
		{
			if ((this.PropertyChanging != null))
			{
				this.PropertyChanging(this, emptyChangingEventArgs);
			}
		}
		
		protected virtual void SendPropertyChanged(String propertyName)
		{
			if ((this.PropertyChanged != null))
			{
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}
#pragma warning restore 1591