using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenericReportGenerator.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class AddWeatherReportRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "weather_report_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    City = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    FromDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ToDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "weather_report_requests");
        }
    }
}
