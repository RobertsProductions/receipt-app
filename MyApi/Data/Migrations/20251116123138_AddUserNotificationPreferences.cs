using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyApi.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNotificationPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationChannel",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotificationThresholdDays",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "OptOutOfNotifications",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationChannel",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NotificationThresholdDays",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "OptOutOfNotifications",
                table: "Users");
        }
    }
}
