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
                name: "ConfigurationVariables",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ArchiveNCRsYears = table.Column<int>(type: "INTEGER", nullable: false),
                    OverdueNCRsNotificationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DateToRunNotificationJob = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateToRunArchiveJob = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationVariables", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DraftEngineerings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsCustNotificationNecessary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CustIssueMsg = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    Disposition = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    DrawReqUpdating = table.Column<bool>(type: "INTEGER", nullable: false),
                    OrgRevisionNum = table.Column<int>(type: "INTEGER", nullable: true),
                    RevisionedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedRevisionNum = table.Column<int>(type: "INTEGER", nullable: true),
                    RevisionDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EngineerSign = table.Column<string>(type: "TEXT", maxLength: 55, nullable: true),
                    EngineeringDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EngReviewID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftEngineerings", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DraftOperationsS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarRaised = table.Column<bool>(type: "INTEGER", nullable: false),
                    CarNum = table.Column<int>(type: "INTEGER", nullable: true),
                    IsFollowUpReq = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowUpType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ExpecDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OperationsDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OpManagerSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    PrelDecisionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftOperationsS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DraftProcurements",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SuppItemsBack = table.Column<bool>(type: "INTEGER", nullable: false),
                    RMANo = table.Column<int>(type: "INTEGER", nullable: true),
                    NCRValue = table.Column<int>(type: "INTEGER", nullable: true),
                    CarrierInfo = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    ExpecDateOfReturn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SuppReturnCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCreditExpec = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChargeSupplier = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisposeOnSite = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcurementDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ProcurementSign = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftProcurements", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DraftQualityRepresentatives",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PoNo = table.Column<int>(type: "INTEGER", nullable: true),
                    SalesOrd = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    QuantReceived = table.Column<int>(type: "INTEGER", nullable: true),
                    QuantDefective = table.Column<int>(type: "INTEGER", nullable: true),
                    DescDefect = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    NonConforming = table.Column<bool>(type: "INTEGER", nullable: true),
                    ConfirmingEng = table.Column<bool>(type: "INTEGER", nullable: true),
                    QualityRepDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    QualityRepresentativeSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ProblemID = table.Column<int>(type: "INTEGER", nullable: true),
                    PartID = table.Column<int>(type: "INTEGER", nullable: true),
                    SupplierID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcessApplicableID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftQualityRepresentatives", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DraftReinspections",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReinspecAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    NewNCRNum = table.Column<string>(type: "TEXT", nullable: true),
                    ReinspectionDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ReinspecInspectorSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftReinspections", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "Parts",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PartNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    PartDesc = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parts", x => x.ID);
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
                name: "Procurements",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SuppItemsBack = table.Column<bool>(type: "INTEGER", nullable: false),
                    RMANo = table.Column<int>(type: "INTEGER", nullable: true),
                    NCRValue = table.Column<int>(type: "INTEGER", nullable: true),
                    CarrierInfo = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    ExpecDateOfReturn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SuppReturnCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsCreditExpec = table.Column<bool>(type: "INTEGER", nullable: false),
                    ChargeSupplier = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisposeOnSite = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProcurementDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ProcurementSign = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Procurements", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Reinspections",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReinspecAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    NewNCRNum = table.Column<string>(type: "TEXT", nullable: true),
                    ReinspectionDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ReinspecInspectorSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reinspections", x => x.ID);
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
                name: "EmployeePhotos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeePhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeePhotos_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeThumbnails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeThumbnails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeThumbnails_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    CreateOn = table.Column<DateTime>(type: "TEXT", nullable: false),
                    wasSeen = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmployeeID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Notifications_Employees_EmployeeID",
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
                    EngReviewID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "OperationsS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarRaised = table.Column<bool>(type: "INTEGER", nullable: false),
                    CarNum = table.Column<int>(type: "INTEGER", nullable: true),
                    IsFollowUpReq = table.Column<bool>(type: "INTEGER", nullable: false),
                    FollowUpType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ExpecDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OperationsDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    OpManagerSign = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 3000, nullable: true),
                    PrelDecisionID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationsS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OperationsS_PrelDecisions_PrelDecisionID",
                        column: x => x.PrelDecisionID,
                        principalTable: "PrelDecisions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
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
                    ProcessApplicableID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
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
                    IsNCRArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    ExpecDateReturnReminded = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsNCRDraft = table.Column<bool>(type: "INTEGER", nullable: false),
                    VoidingReason = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    Phase = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedOnDO = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    QualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    OperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    PrevNCRID = table.Column<int>(type: "INTEGER", nullable: true),
                    NewNCRID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftQualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftEngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftOperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftReinspectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SectionUpdated = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NCRs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NCRs_DraftEngineerings_DraftEngineeringID",
                        column: x => x.DraftEngineeringID,
                        principalTable: "DraftEngineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_DraftOperationsS_DraftOperationsID",
                        column: x => x.DraftOperationsID,
                        principalTable: "DraftOperationsS",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_DraftProcurements_DraftProcurementID",
                        column: x => x.DraftProcurementID,
                        principalTable: "DraftProcurements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_DraftQualityRepresentatives_DraftQualityRepresentativeID",
                        column: x => x.DraftQualityRepresentativeID,
                        principalTable: "DraftQualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_DraftReinspections_DraftReinspectionID",
                        column: x => x.DraftReinspectionID,
                        principalTable: "DraftReinspections",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_OperationsS_OperationsID",
                        column: x => x.OperationsID,
                        principalTable: "OperationsS",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Procurements_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NCRs_Reinspections_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspections",
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
                    OperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftQualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftEngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftOperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftReinspectionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityPhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_DraftEngineerings_DraftEngineeringID",
                        column: x => x.DraftEngineeringID,
                        principalTable: "DraftEngineerings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_DraftOperationsS_DraftOperationsID",
                        column: x => x.DraftOperationsID,
                        principalTable: "DraftOperationsS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_DraftProcurements_DraftProcurementID",
                        column: x => x.DraftProcurementID,
                        principalTable: "DraftProcurements",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_DraftQualityRepresentatives_DraftQualityRepresentativeID",
                        column: x => x.DraftQualityRepresentativeID,
                        principalTable: "DraftQualityRepresentatives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_DraftReinspections_DraftReinspectionID",
                        column: x => x.DraftReinspectionID,
                        principalTable: "DraftReinspections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_OperationsS_OperationsID",
                        column: x => x.OperationsID,
                        principalTable: "OperationsS",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Procurements_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_QualityPhotos_Reinspections_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspections",
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
                    OperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    ProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftQualityRepresentativeID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftEngineeringID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftOperationsID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftProcurementID = table.Column<int>(type: "INTEGER", nullable: true),
                    DraftReinspectionID = table.Column<int>(type: "INTEGER", nullable: true),
                    ReinspectionID = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLinks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_VideoLinks_DraftEngineerings_DraftEngineeringID",
                        column: x => x.DraftEngineeringID,
                        principalTable: "DraftEngineerings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLinks_DraftOperationsS_DraftOperationsID",
                        column: x => x.DraftOperationsID,
                        principalTable: "DraftOperationsS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLinks_DraftProcurements_DraftProcurementID",
                        column: x => x.DraftProcurementID,
                        principalTable: "DraftProcurements",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLinks_DraftQualityRepresentatives_DraftQualityRepresentativeID",
                        column: x => x.DraftQualityRepresentativeID,
                        principalTable: "DraftQualityRepresentatives",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLinks_DraftReinspections_DraftReinspectionID",
                        column: x => x.DraftReinspectionID,
                        principalTable: "DraftReinspections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VideoLinks_Engineerings_EngineeringID",
                        column: x => x.EngineeringID,
                        principalTable: "Engineerings",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_OperationsS_OperationsID",
                        column: x => x.OperationsID,
                        principalTable: "OperationsS",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_Procurements_ProcurementID",
                        column: x => x.ProcurementID,
                        principalTable: "Procurements",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_QualityRepresentatives_QualityRepresentativeID",
                        column: x => x.QualityRepresentativeID,
                        principalTable: "QualityRepresentatives",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_VideoLinks_Reinspections_ReinspectionID",
                        column: x => x.ReinspectionID,
                        principalTable: "Reinspections",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeePhotos_EmployeeID",
                table: "EmployeePhotos",
                column: "EmployeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeThumbnails_EmployeeID",
                table: "EmployeeThumbnails",
                column: "EmployeeID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Engineerings_EngReviewID",
                table: "Engineerings",
                column: "EngReviewID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRNumbers_Year_Counter",
                table: "NCRNumbers",
                columns: new[] { "Year", "Counter" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_DraftEngineeringID",
                table: "NCRs",
                column: "DraftEngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_DraftOperationsID",
                table: "NCRs",
                column: "DraftOperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_DraftProcurementID",
                table: "NCRs",
                column: "DraftProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_DraftQualityRepresentativeID",
                table: "NCRs",
                column: "DraftQualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_DraftReinspectionID",
                table: "NCRs",
                column: "DraftReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_EngineeringID",
                table: "NCRs",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_NCRNum",
                table: "NCRs",
                column: "NCRNum",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_OperationsID",
                table: "NCRs",
                column: "OperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_ProcurementID",
                table: "NCRs",
                column: "ProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_QualityRepresentativeID",
                table: "NCRs",
                column: "QualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_NCRs_ReinspectionID",
                table: "NCRs",
                column: "ReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_EmployeeID",
                table: "Notifications",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_OperationsS_PrelDecisionID",
                table: "OperationsS",
                column: "PrelDecisionID");

            migrationBuilder.CreateIndex(
                name: "IX_Parts_PartNumber",
                table: "Parts",
                column: "PartNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Problems_ProblemDescription",
                table: "Problems",
                column: "ProblemDescription",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_DraftEngineeringID",
                table: "QualityPhotos",
                column: "DraftEngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_DraftOperationsID",
                table: "QualityPhotos",
                column: "DraftOperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_DraftProcurementID",
                table: "QualityPhotos",
                column: "DraftProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_DraftQualityRepresentativeID",
                table: "QualityPhotos",
                column: "DraftQualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_DraftReinspectionID",
                table: "QualityPhotos",
                column: "DraftReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_EngineeringID",
                table: "QualityPhotos",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_OperationsID",
                table: "QualityPhotos",
                column: "OperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_QualityPhotos_ProcurementID",
                table: "QualityPhotos",
                column: "ProcurementID");

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
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_DraftEngineeringID",
                table: "VideoLinks",
                column: "DraftEngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_DraftOperationsID",
                table: "VideoLinks",
                column: "DraftOperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_DraftProcurementID",
                table: "VideoLinks",
                column: "DraftProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_DraftQualityRepresentativeID",
                table: "VideoLinks",
                column: "DraftQualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_DraftReinspectionID",
                table: "VideoLinks",
                column: "DraftReinspectionID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_EngineeringID",
                table: "VideoLinks",
                column: "EngineeringID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_OperationsID",
                table: "VideoLinks",
                column: "OperationsID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_ProcurementID",
                table: "VideoLinks",
                column: "ProcurementID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_QualityRepresentativeID",
                table: "VideoLinks",
                column: "QualityRepresentativeID");

            migrationBuilder.CreateIndex(
                name: "IX_VideoLinks_ReinspectionID",
                table: "VideoLinks",
                column: "ReinspectionID");

            ExtraMigration.Steps(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationVariables");

            migrationBuilder.DropTable(
                name: "EmployeePhotos");

            migrationBuilder.DropTable(
                name: "EmployeeThumbnails");

            migrationBuilder.DropTable(
                name: "NCRNumbers");

            migrationBuilder.DropTable(
                name: "NCRs");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "QualityPhotos");

            migrationBuilder.DropTable(
                name: "VideoLinks");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "DraftEngineerings");

            migrationBuilder.DropTable(
                name: "DraftOperationsS");

            migrationBuilder.DropTable(
                name: "DraftProcurements");

            migrationBuilder.DropTable(
                name: "DraftQualityRepresentatives");

            migrationBuilder.DropTable(
                name: "DraftReinspections");

            migrationBuilder.DropTable(
                name: "Engineerings");

            migrationBuilder.DropTable(
                name: "OperationsS");

            migrationBuilder.DropTable(
                name: "Procurements");

            migrationBuilder.DropTable(
                name: "QualityRepresentatives");

            migrationBuilder.DropTable(
                name: "Reinspections");

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
