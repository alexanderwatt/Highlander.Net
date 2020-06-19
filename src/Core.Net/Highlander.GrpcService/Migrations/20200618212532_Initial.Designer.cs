﻿// <auto-generated />
using System;
using Highlander.GrpcService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Highlander.GrpcService.Migrations
{
    [DbContext(typeof(HighlanderContext))]
    [Migration("20200618212532_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.5");

            modelBuilder.Entity("Highlander.GrpcService.Data.ItemData", b =>
                {
                    b.Property<Guid>("ItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AppProps")
                        .HasColumnType("TEXT");

                    b.Property<string>("AppScope")
                        .HasColumnType("TEXT");

                    b.Property<string>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("DataType")
                        .HasColumnType("TEXT");

                    b.Property<string>("Expires")
                        .HasColumnType("TEXT");

                    b.Property<string>("ItemName")
                        .HasColumnType("TEXT");

                    b.Property<int>("ItemType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("NetScope")
                        .HasColumnType("TEXT");

                    b.Property<int>("StoreSRN")
                        .HasColumnType("INTEGER");

                    b.Property<long>("StoreUSN")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SysProps")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("YData")
                        .HasColumnType("BLOB");

                    b.Property<byte[]>("YSign")
                        .HasColumnType("BLOB");

                    b.HasKey("ItemId");

                    b.ToTable("Items");
                });
#pragma warning restore 612, 618
        }
    }
}
