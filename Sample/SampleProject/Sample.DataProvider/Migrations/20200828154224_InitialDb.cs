using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Sample.DataProvider.Migrations
{
    public partial class InitialDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(nullable: true),
                    Timestamp = table.Column<long>(nullable: false),
                    DateHappened = table.Column<DateTime>(nullable: false),
                    Action = table.Column<string>(nullable: true),
                    Area = table.Column<string>(nullable: true),
                    Controller = table.Column<string>(nullable: true),
                    RawUrl = table.Column<string>(nullable: true),
                    Ip = table.Column<string>(nullable: true),
                    UserSessionUniqueId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainEventProviderReporting",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    EventProviderId = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    FullQualifiedType = table.Column<string>(nullable: true),
                    EntityId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventProviderReporting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DomainEventReporting",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false),
                    EventId = table.Column<Guid>(nullable: false),
                    JSonDomainEvent = table.Column<string>(nullable: true),
                    EventProviderId = table.Column<Guid>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    User = table.Column<string>(nullable: true),
                    Timestamp = table.Column<long>(nullable: false),
                    DateHappened = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEventReporting", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregateRootId = table.Column<Guid>(nullable: false),
                    LastUpdate = table.Column<long>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sequenceHiLo",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_parametres = table.Column<string>(nullable: true),
                    intval = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sequenceHiLo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherInfos",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AggregateRootId = table.Column<Guid>(nullable: false),
                    LastUpdate = table.Column<long>(nullable: false),
                    EntityId = table.Column<long>(nullable: false),
                    RegionName = table.Column<string>(nullable: true),
                    Date = table.Column<DateTime>(nullable: false),
                    TemperatureC = table.Column<int>(nullable: false),
                    Summary = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherInfos", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "DomainEventProviderReporting");

            migrationBuilder.DropTable(
                name: "DomainEventReporting");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "sequenceHiLo");

            migrationBuilder.DropTable(
                name: "WeatherInfos");
        }
    }
}
