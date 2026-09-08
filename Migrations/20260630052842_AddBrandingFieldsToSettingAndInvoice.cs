using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JewelleryERP.Migrations
{
    /// <inheritdoc />
    public partial class AddBrandingFieldsToSettingAndInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BillNumber",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CgstAmount",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CgstRate",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MakingChargeTotal",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OwnerSignatureField",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SgstAmount",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SgstRate",
                table: "Invoices",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeightGms",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrossWeightMg",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "HsnCode",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MakingCharge",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeightGms",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetWeightMg",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RatePerGram",
                table: "InvoiceItems",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LoanNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoanDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PrincipalAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    InterestRate = table.Column<decimal>(type: "TEXT", nullable: false),
                    Weight = table.Column<decimal>(type: "TEXT", precision: 18, scale: 3, nullable: false),
                    PresentValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ArticleDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    OwnerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    OwnerAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RedeemerName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    RedeemerAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AuctionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Loans_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ShopName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    GSTIN = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    CurrentGoldRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CurrentSilverRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CGST = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    SGST = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    CurrentBillNumber = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    DefaultInterestRate = table.Column<decimal>(type: "TEXT", precision: 5, scale: 2, nullable: false),
                    ShopLogoPath = table.Column<string>(type: "TEXT", nullable: true),
                    ShopStampPath = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CustomerId",
                table: "Loans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanNumber",
                table: "Loans",
                column: "LoanNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BillNumber",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CgstAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CgstRate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "MakingChargeTotal",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "OwnerSignatureField",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SgstAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SgstRate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "GrossWeightGms",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "GrossWeightMg",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "HsnCode",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "MakingCharge",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "NetWeightGms",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "NetWeightMg",
                table: "InvoiceItems");

            migrationBuilder.DropColumn(
                name: "RatePerGram",
                table: "InvoiceItems");
        }
    }
}
