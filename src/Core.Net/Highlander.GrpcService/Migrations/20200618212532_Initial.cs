using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Highlander.GrpcService.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    ItemId = table.Column<Guid>(nullable: false),
                    ItemName = table.Column<string>(nullable: true),
                    ItemType = table.Column<int>(nullable: false),
                    AppScope = table.Column<string>(nullable: true),
                    AppProps = table.Column<string>(nullable: true),
                    Created = table.Column<string>(nullable: true),
                    Expires = table.Column<string>(nullable: true),
                    DataType = table.Column<string>(nullable: true),
                    YData = table.Column<byte[]>(nullable: true),
                    YSign = table.Column<byte[]>(nullable: true),
                    NetScope = table.Column<string>(nullable: true),
                    SysProps = table.Column<string>(nullable: true),
                    StoreSRN = table.Column<int>(nullable: false),
                    StoreUSN = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.ItemId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Items");
        }
    }
}
