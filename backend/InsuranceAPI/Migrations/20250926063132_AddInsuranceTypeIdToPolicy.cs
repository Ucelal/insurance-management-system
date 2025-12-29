using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddInsuranceTypeIdToPolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ins_Type_Id",
                table: "Policies",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Ins_Type_Id",
                table: "Policies",
                column: "Ins_Type_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_Reviewed_By_Agent_Id",
                table: "Offers",
                column: "Reviewed_By_Agent_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Agents_Reviewed_By_Agent_Id",
                table: "Offers",
                column: "Reviewed_By_Agent_Id",
                principalTable: "Agents",
                principalColumn: "Agent_Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_InsuranceTypes_Ins_Type_Id",
                table: "Policies",
                column: "Ins_Type_Id",
                principalTable: "InsuranceTypes",
                principalColumn: "Ins_Type_Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Agents_Reviewed_By_Agent_Id",
                table: "Offers");

            migrationBuilder.DropForeignKey(
                name: "FK_Policies_InsuranceTypes_Ins_Type_Id",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_Ins_Type_Id",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Offers_Reviewed_By_Agent_Id",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "Ins_Type_Id",
                table: "Policies");
        }
    }
}
