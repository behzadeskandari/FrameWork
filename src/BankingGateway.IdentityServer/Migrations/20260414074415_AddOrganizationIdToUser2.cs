using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingGateway.IdentityServer.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToUser2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "IdentityUsers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrganizationId",
                table: "IdentityUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
