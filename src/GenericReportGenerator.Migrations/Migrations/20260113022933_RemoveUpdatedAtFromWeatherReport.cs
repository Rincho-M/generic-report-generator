using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GenericReportGenerator.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUpdatedAtFromWeatherReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "weather_report");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                table: "weather_report",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}
