using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingGateway.IdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "IdentityUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IdentityUsers");
        }
    }
}
