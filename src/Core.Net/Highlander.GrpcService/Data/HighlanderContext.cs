using Highlander.Core.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Highlander.GrpcService.Data
{
    public class HighlanderContext : DbContext
    {
        public DbSet<ItemData> Items { get; set; }
    }

    //  <Type Name = "ItemData" >
    //< Column Name="ItemId" Type="System.Guid" DbType="UniqueIdentifier NOT NULL" IsPrimaryKey="true" CanBeNull="false" />
    //<Column Name = "ItemName" Type="System.String" DbType="NVarChar(255) NOT NULL" CanBeNull="false" />
    //<Column Name = "ItemType" Type="System.Int32" DbType="Int NOT NULL" CanBeNull="false" />
    //<Column Name = "AppScope" Type="System.String" DbType="NVarChar(255)" CanBeNull="true" />
    //<Column Name = "AppProps" Type="System.String" DbType="NVarChar(MAX)" CanBeNull="true" />
    //<Column Name = "Created" Type="System.String" DbType="NVarChar(50) NOT NULL" CanBeNull="false" />
    //<Column Name = "Expires" Type="System.String" DbType="NVarChar(50) NOT NULL" CanBeNull="false" />
    //<Column Name = "DataType" Type="System.String" DbType="NVarChar(255)" CanBeNull="true" />
    //<Column Name = "YData" Type="System.Data.Linq.Binary" DbType="Image" CanBeNull="true" UpdateCheck="Never" />
    //<Column Name = "YSign" Type="System.Data.Linq.Binary" DbType="VarBinary(MAX)" CanBeNull="true" UpdateCheck="Never" />
    //<Column Name = "NetScope" Type="System.String" DbType="NVarChar(255)" CanBeNull="true" />
    //<Column Name = "SysProps" Type="System.String" DbType="NVarChar(MAX)" CanBeNull="true" />
    //<Column Name = "StoreSRN" Type="System.Int32" DbType="Int NOT NULL" CanBeNull="false" />
    //<Column Name = "StoreUSN" Type="System.Int64" DbType="BigInt NOT NULL" CanBeNull="false" />


    public class ItemData
    {
        [Key]
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public int ItemType { get; set; }
        public string AppScope { get; set; }
        public string AppProps { get; set; }
        public string Created { get; set; }
        public string Expires { get; set; }
        public string DataType { get; set; }
        public byte[] YData { get; set; }
        public byte[] YSign { get; set; }
        public string NetScope { get; set; }
        public string SysProps { get; set; }
        public int StoreSRN { get; set; }
        public long StoreUSN { get; set; }

        internal ItemData(CommonItem row)
        {
            ItemId = row.Id;
            ItemName = row.Name;
            ItemType = (int)row.ItemKind;
            if (row.AppProps != null)
                AppProps = row.AppProps.Serialise();
            if (row.SysProps != null)
                SysProps = row.SysProps.Serialise();
            DataType = row.DataTypeName;
            AppScope = row.AppScope;
            NetScope = row.NetScope;
            Created = row.Created.ToString("o");
            Expires = row.Expires.ToString("o");
            if (row.YData != null)
                YData = row.YData.ToArray();
            if (row.YSign != null)
                YSign = row.YSign.ToArray();
            StoreSRN = 1;
            StoreUSN = row.StoreUSN;
        }
    }
}
