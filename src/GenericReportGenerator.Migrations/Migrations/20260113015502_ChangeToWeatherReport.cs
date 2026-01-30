using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenericReportGenerator.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChangeToWeatherReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather_report_requests");

            migrationBuilder.CreateTable(
                name: "weather_report",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_report", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weather_report_CreatedAt",
                table: "weather_report",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_weather_report_Status",
                table: "weather_report",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather_report");

            migrationBuilder.CreateTable(
                name: "weather_report_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weather_report_requests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_weather_report_requests_CreatedAt",
                table: "weather_report_requests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_weather_report_requests_Status",
                table: "weather_report_requests",
                column: "Status");
        }
    }
}
