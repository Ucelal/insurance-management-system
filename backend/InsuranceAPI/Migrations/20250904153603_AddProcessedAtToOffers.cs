using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InsuranceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessedAtToOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InsuranceTypes",
                columns: table => new
                {
                    Ins_Type_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Coverage_Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    User_Id = table.Column<int>(type: "int", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceTypes", x => x.Ins_Type_Id);
                });

            migrationBuilder.CreateTable(
                name: "TokenBlacklist",
                columns: table => new
                {
                    Token_black_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Blacklisted_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expires_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    User_Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokenBlacklist", x => x.Token_black_Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    User_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password_Hash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.User_Id);
                });

            migrationBuilder.CreateTable(
                name: "Coverages",
                columns: table => new
                {
                    Coverage_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Base_Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Insurance_Type_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coverages", x => x.Coverage_Id);
                    table.ForeignKey(
                        name: "FK_Coverages_InsuranceTypes_Insurance_Type_Id",
                        column: x => x.Insurance_Type_Id,
                        principalTable: "InsuranceTypes",
                        principalColumn: "Ins_Type_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    Agent_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Agent_Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.Agent_Id);
                    table.ForeignKey(
                        name: "FK_Agents_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Customer_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id_No = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Customer_Id);
                    table.ForeignKey(
                        name: "FK_Customers_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Offer_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Base_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Final_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount_Rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Coverage_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Agent_Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Customer_Additional_Info = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Requested_Start_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Valid_Until = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Is_Customer_Approved = table.Column<bool>(type: "bit", nullable: false),
                    Customer_Approved_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reviewed_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reviewed_By_Agent_Id = table.Column<int>(type: "int", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Processed_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Customer_Id = table.Column<int>(type: "int", nullable: true),
                    Agent_Id = table.Column<int>(type: "int", nullable: true),
                    Insurance_Type_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Offer_Id);
                    table.ForeignKey(
                        name: "FK_Offers_Agents_Agent_Id",
                        column: x => x.Agent_Id,
                        principalTable: "Agents",
                        principalColumn: "Agent_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Offers_Customers_Customer_Id",
                        column: x => x.Customer_Id,
                        principalTable: "Customers",
                        principalColumn: "Customer_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Offers_InsuranceTypes_Insurance_Type_Id",
                        column: x => x.Insurance_Type_Id,
                        principalTable: "InsuranceTypes",
                        principalColumn: "Ins_Type_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Policy_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Policy_Number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Start_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    End_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Total_Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Offer_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Policy_Id);
                    table.ForeignKey(
                        name: "FK_Policies_Offers_Offer_Id",
                        column: x => x.Offer_Id,
                        principalTable: "Offers",
                        principalColumn: "Offer_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SelectedCoverages",
                columns: table => new
                {
                    Sel_Cov_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Offer_Id = table.Column<int>(type: "int", nullable: true),
                    Coverage_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true),
                    Selected_Limit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Premium = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedCoverages", x => x.Sel_Cov_Id);
                    table.ForeignKey(
                        name: "FK_SelectedCoverages_Coverages_Coverage_Id",
                        column: x => x.Coverage_Id,
                        principalTable: "Coverages",
                        principalColumn: "Coverage_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SelectedCoverages_Offers_Offer_Id",
                        column: x => x.Offer_Id,
                        principalTable: "Offers",
                        principalColumn: "Offer_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Claim_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Incident_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reported_Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estimated_Resolution_Date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Processed_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Approved_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Claim_Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Customer_Id = table.Column<int>(type: "int", nullable: true),
                    Agent_Id = table.Column<int>(type: "int", nullable: true),
                    Policy_Id = table.Column<int>(type: "int", nullable: true),
                    Created_By_User_Id = table.Column<int>(type: "int", nullable: true),
                    Processed_By_User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Claim_Id);
                    table.ForeignKey(
                        name: "FK_Claims_Agents_Agent_Id",
                        column: x => x.Agent_Id,
                        principalTable: "Agents",
                        principalColumn: "Agent_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Claims_Customers_Customer_Id",
                        column: x => x.Customer_Id,
                        principalTable: "Customers",
                        principalColumn: "Customer_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Claims_Policies_Policy_Id",
                        column: x => x.Policy_Id,
                        principalTable: "Policies",
                        principalColumn: "Policy_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Claims_Users_Created_By_User_Id",
                        column: x => x.Created_By_User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                    table.ForeignKey(
                        name: "FK_Claims_Users_Processed_By_User_Id",
                        column: x => x.Processed_By_User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Payment_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Paid_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Created_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Policy_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Payment_Id);
                    table.ForeignKey(
                        name: "FK_Payments_Policies_Policy_Id",
                        column: x => x.Policy_Id,
                        principalTable: "Policies",
                        principalColumn: "Policy_Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Document_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    File_Url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    File_Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Uploaded_At = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expires_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Updated_At = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Customer_Id = table.Column<int>(type: "int", nullable: true),
                    Claim_Id = table.Column<int>(type: "int", nullable: true),
                    Policy_Id = table.Column<int>(type: "int", nullable: true),
                    Uploaded_By_User_Id = table.Column<int>(type: "int", nullable: true),
                    User_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Document_Id);
                    table.ForeignKey(
                        name: "FK_Documents_Claims_Claim_Id",
                        column: x => x.Claim_Id,
                        principalTable: "Claims",
                        principalColumn: "Claim_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Customers_Customer_Id",
                        column: x => x.Customer_Id,
                        principalTable: "Customers",
                        principalColumn: "Customer_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Policies_Policy_Id",
                        column: x => x.Policy_Id,
                        principalTable: "Policies",
                        principalColumn: "Policy_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Users_Uploaded_By_User_Id",
                        column: x => x.Uploaded_By_User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Documents_Users_User_Id",
                        column: x => x.User_Id,
                        principalTable: "Users",
                        principalColumn: "User_Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_User_Id",
                table: "Agents",
                column: "User_Id",
                unique: true,
                filter: "[User_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Agent_Id",
                table: "Claims",
                column: "Agent_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Created_By_User_Id",
                table: "Claims",
                column: "Created_By_User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Customer_Id",
                table: "Claims",
                column: "Customer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Policy_Id",
                table: "Claims",
                column: "Policy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_Processed_By_User_Id",
                table: "Claims",
                column: "Processed_By_User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Coverages_Insurance_Type_Id",
                table: "Coverages",
                column: "Insurance_Type_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_User_Id",
                table: "Customers",
                column: "User_Id",
                unique: true,
                filter: "[User_Id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Claim_Id",
                table: "Documents",
                column: "Claim_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Customer_Id",
                table: "Documents",
                column: "Customer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Policy_Id",
                table: "Documents",
                column: "Policy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Uploaded_By_User_Id",
                table: "Documents",
                column: "Uploaded_By_User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_User_Id",
                table: "Documents",
                column: "User_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_Agent_Id",
                table: "Offers",
                column: "Agent_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_Customer_Id",
                table: "Offers",
                column: "Customer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_Insurance_Type_Id",
                table: "Offers",
                column: "Insurance_Type_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Policy_Id",
                table: "Payments",
                column: "Policy_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_Offer_Id",
                table: "Policies",
                column: "Offer_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedCoverages_Coverage_Id",
                table: "SelectedCoverages",
                column: "Coverage_Id");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedCoverages_Offer_Id",
                table: "SelectedCoverages",
                column: "Offer_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SelectedCoverages");

            migrationBuilder.DropTable(
                name: "TokenBlacklist");

            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "Coverages");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "InsuranceTypes");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
