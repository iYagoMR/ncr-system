using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Haver.Data.HMigrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Prescriber = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstAid = table.Column<int>(type: "INTEGER", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EngReviews",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Review = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EngReviews", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NCRNumbers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Counter = table.Column<int>(type: "INTEGER", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NCRNumbers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PrelDecisions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Decision = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrelDecisions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Problems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProblemDescription = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Problems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProcessesApplicable",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProcessName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessesApplicable", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Procurement",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SuppItemsBack = table.Column<bool>(type: "INTEGER", nullable: false),
                    RMANo = table.Column<int>(type: "INTEGER", nullable: true),
                    CarrierInfo = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    ExpecDateOfReturn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SuppReturnCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCreditExpec = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChargeSupplier = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcurementDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ProcurementSign = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procurement", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Reinspection",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReinspecAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    NewNCRNum = table.Column<string>(type: "TEXT", nullable: true),
                    ReinspectionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ReinspecInspectorSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reinspection", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SupplierName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SupplierCode = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Role = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PushEndpoint = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PushP256DH = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PushAuth = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Engineerings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsCustNotificationNecessary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustIssueMsg = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    Disposition = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    DrawReqUpdating = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrgRevisionNum = table.Column<int>(type: "INTEGER", nullable: true),
                    RevisionedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedRevisionNum = table.Column<int>(type: "INTEGER", nullable: true),
                    RevisionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngineerSign = table.Column<string>(type: "TEXT", maxLength: 55, nullable: false),
                    EngineeringDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EngReviewID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Engineerings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Engineerings_EngReviews_EngReviewID",
                        column: x => x.EngReviewID,
                        principalTable: "EngReviews",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Purchasings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarRaised = table.Column<bool>(type: "INTEGER", nullable: false),
                    CarNum = table.Column<int>(type: "INTEGER", nullable: true),
                    IsFollowUpReq = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowUpType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ExpecDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PurchasingDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    OpManagerSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    PrelDecisionID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchasings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Purchasings_PrelDecisions_PrelDecisionID",
                        column: x => x.PrelDecisionID,
                        principalTable: "PrelDecisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Parts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    PartDesc = table.Column<string>(type: "TEXT", nullable: false),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Parts_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "QualityRepresentatives",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PoNo = table.Column<int>(type: "INTEGER", nullable: false),
                    SalesOrd = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    QuantReceived = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantDefective = table.Column<int>(type: "INTEGER", nullable: false),
                    DescDefect = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: false),
                    NonConforming = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConfirmingEng = table.Column<bool>(type: "INTEGER", nullable: false),
                    QualityRepDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    QualityRepresentativeSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ProblemID = table.Column<int>(type: "INTEGER", nullable: false),
                    PartID = table.Column<int>(type: "INTEGER", nullable: false),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessApplicableID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityRepresentatives", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QualityRepresentatives_Parts_PartID",
                        column: x => x.PartID,
                        principalTable: "Parts",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityRepresentatives_Problems_ProblemID",
                        column: x => x.ProblemID,
                        principalTable: "Problems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityRepresentatives_ProcessesApplicable_ProcessApplicableID",
                        column: x => x.ProcessApplicableID,
                        principalTable: "ProcessesApplicable",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityRepresentatives_Suppliers_SupplierID",
                        column: x => x.SupplierID,
                        principalTable: "Suppliers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NCRs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NCRNum = table.Column<string>(type: "TEXT", nullable: true),
                    IsEngineerRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNCRArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNCRDraft = table.Column<bool>(type: "INTEGER", nullable: false),
                    VoidingReason = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Phase = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    QualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    PurchasingID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    PrevNCRID = table.Column<int>(type: "INTEGER", nullable: true),
                    NewNCRID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NCRs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NCRs_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Procurement_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurement",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Purchasings_PurchasingID",
                        column: x => x.PurchasingID,
                        principalTable: "Purchasings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Reinspection_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspection",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "QualityPhotos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    QualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    PurchasingID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityPhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Procurement_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurement",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Purchasings_PurchasingID",
                        column: x => x.PurchasingID,
                        principalTable: "Purchasings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Reinspection_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspection",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "VideoLinks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Link = table.Column<string>(type: "TEXT", nullable: true),
                    QualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    PurchasingID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLinks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VideoLinks_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_Procurement_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurement",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_Purchasings_PurchasingID",
                        column: x => x.PurchasingID,
                        principalTable: "Purchasings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_Reinspection_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspection",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engineerings_EngReviewID",
                table: "Engineerings",
                column: "EngReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_EngineeringID",
                table: "NCRs",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_ProcurementID",
                table: "NCRs",
                column: "ProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_PurchasingID",
                table: "NCRs",
                column: "PurchasingID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_QualityRepresentativeID",
                table: "NCRs",
                column: "QualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_ReinspectionID",
                table: "NCRs",
                column: "ReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_SupplierID",
                table: "Parts",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_Purchasings_PrelDecisionID",
                table: "Purchasings",
                column: "PrelDecisionID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_EngineeringID",
                table: "QualityPhotos",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_ProcurementID",
                table: "QualityPhotos",
                column: "ProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_PurchasingID",
                table: "QualityPhotos",
                column: "PurchasingID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_QualityRepresentativeID",
                table: "QualityPhotos",
                column: "QualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_ReinspectionID",
                table: "QualityPhotos",
                column: "ReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRepresentatives_PartID",
                table: "QualityRepresentatives",
                column: "PartID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRepresentatives_ProblemID",
                table: "QualityRepresentatives",
                column: "ProblemID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRepresentatives_ProcessApplicableID",
                table: "QualityRepresentatives",
                column: "ProcessApplicableID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityRepresentatives_SupplierID",
                table: "QualityRepresentatives",
                column: "SupplierID");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_EmployeeID",
                table: "Subscriptions",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_EngineeringID",
                table: "VideoLinks",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_ProcurementID",
                table: "VideoLinks",
                column: "ProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_PurchasingID",
                table: "VideoLinks",
                column: "PurchasingID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_QualityRepresentativeID",
                table: "VideoLinks",
                column: "QualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_ReinspectionID",
                table: "VideoLinks",
                column: "ReinspectionID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NCRNumbers");

            migrationBuilder.DropTable(
                name: "NCRs");

            migrationBuilder.DropTable(
                name: "QualityPhotos");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "VideoLinks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Engineerings");

            migrationBuilder.DropTable(
                name: "Procurement");

            migrationBuilder.DropTable(
                name: "Purchasings");

            migrationBuilder.DropTable(
                name: "QualityRepresentatives");

            migrationBuilder.DropTable(
                name: "Reinspection");

            migrationBuilder.DropTable(
                name: "EngReviews");

            migrationBuilder.DropTable(
                name: "PrelDecisions");

            migrationBuilder.DropTable(
                name: "Parts");

            migrationBuilder.DropTable(
                name: "Problems");

            migrationBuilder.DropTable(
                name: "ProcessesApplicable");

            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}
