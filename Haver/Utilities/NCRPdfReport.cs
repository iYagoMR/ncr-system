using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Haver.Models;
using System;

namespace Haver.Utilities
{
    public class NCRPdfReport : IDocument
    {
        public NCR Model { get; }

        public NCRPdfReport(NCR model)
        {
            Model = model;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(50);

                    page.Header().Column(column =>
                    {
                        column.Item().ShowOnce().Element(ComposeHeader);
                        column.Item().SkipOnce().Height(0);
                    });

                    page.Content().Element(ComposeContent);

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
        }


        void ComposeHeader(IContainer container)
        {
            var titleStyle = TextStyle.Default.FontSize(20).SemiBold().FontColor("#19416D");

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"NCR #{Model.NCRNum}").Style(titleStyle);

                    column.Item().Text(text =>
                    {
                        text.Span("NCR start date: ").SemiBold();
                        text.Span($"{Model.CreatedOn:d}");
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("Report issue date: ").SemiBold();
                        text.Span($"{DateOnly.FromDateTime(DateTime.Now):d}");
                    });
                });
                row.ConstantItem(208).Height(48).Image("wwwroot\\assets\\images\\logo-horizontal-blue.png").FitArea();
            });
        }

        void ComposeContent(IContainer container)
        {
            container.PaddingVertical(40).Column(column =>
            {
                column.Spacing(5);
                if (Model.QualityRepresentative != null)
                {
                    column.Item().Element(QualityRepresentative);
                }
                if(Model.Engineering != null)
                {
                    column.Item().Element(Engineering);
                }
                if(Model.Operations != null)
                {
                    column.Item().Element(Operations);
                }
                if(Model.Procurement != null)
                {
                    column.Item().Element(Procurement);
                }
                if(Model.Reinspection != null)
                {
                    column.Item().Element(Reinspection);
                }
                column.Item().PageBreak();
                column.Item().Element(Multimedia);
            });
        }

        void QualityRepresentative(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");
            var pStyle = TextStyle.Default.FontSize(11).FontColor("#000000");

            container
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).Text($"Quality Representative").Style(headingStyle);

                    //Table - Main NCR Information(QualityRep)
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Process Applicable");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{Model.QualityRepresentative.ProcessApplicable.ProcessName}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("Supplier Name");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{Model.QualityRepresentative.Supplier.SupplierName}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("PO or Prod. No.");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{Model.QualityRepresentative.PoNo}");

                        table.Cell().Row(1).Column(4).Element(BlockTitle).Text("Sales Order No.");
                        table.Cell().Row(2).Column(4).Element(BlockContent).Text($"{Model.QualityRepresentative.SalesOrd}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Second section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(248);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(3).Column(1).Element(PartDescTitle).Text("Description of Item");
                        table.Cell().Row(4).Column(1).Element(PartDescContent).Text($"{Model.QualityRepresentative.Part.PartSummary}");
                        table.Cell().Row(3).Column(2).Element(BlockTitle).Text("Quantity Received");
                        table.Cell().Row(3).Column(3).Element(BlockContent).Text($"{Model.QualityRepresentative.QuantReceived}");
                        table.Cell().Row(4).Column(2).Element(BlockTitle).Text("Quantity Defective");
                        table.Cell().Row(4).Column(3).Element(BlockContent).Text($"{Model.QualityRepresentative.QuantDefective}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer PartDescTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(100)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer PartDescContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(100)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Third section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(120);
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(3).Column(1).Element(DefectDescTitle).Text("Description of Defect");
                        table.Cell().Row(3).Column(2).Element(DefectDescContent).Text($"{Model.QualityRepresentative.DescDefect}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer DefectDescTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer DefectDescContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });

                    //Author
                    column.Item().PaddingTop(12).Text(text =>
                    {
                        text.Span("Author: ").SemiBold();
                        text.Span($"{Model.QualityRepresentative.QualityRepresentativeSign}");
                    });
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Completed on: ").SemiBold();
                        text.Span($"{Model.QualityRepresentative.QualityRepDate:d}");
                    });


                });
        }

        void Engineering(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");

            container
                .PaddingTop(20)
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).Text($"Engineering").Style(headingStyle);

                    //Table Engineering
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Review by HBC Engineering");
                        table.Cell().Row(1).Column(2).Element(BlockContent).Text($"{Model.Engineering.EngReview.Review}");

                        table.Cell().Row(2).Column(1).Element(BlockTitle).Text("Customer require notification?");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{(Model.Engineering.IsCustNotificationNecessary ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("PO or Prod. No.");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{Model.Engineering.Disposition}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Second section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Does the drawing require updating?");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{(Model.Engineering.DrawReqUpdating ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("Original Rev. Number");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{Model.Engineering.OrgRevisionNum}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("Updated Rev. Number");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{Model.Engineering.UpdatedRevisionNum}");

                        table.Cell().Row(1).Column(4).Element(BlockTitle).Text("Name of Engineer");
                        table.Cell().Row(2).Column(4).Element(BlockContent).Text($"{Model.Engineering.RevisionDate}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }

                    });

                    //Author
                    column.Item().PaddingTop(12).Text(text =>
                    {
                        text.Span("Author: ").SemiBold();
                        text.Span($"{Model.Engineering.EngineerSign}");
                    });
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Completed on: ").SemiBold();
                        text.Span($"{Model.Engineering.EngineeringDate:d}");
                    });
                });
        }

        void Operations(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");

            container
                .PaddingTop(20)
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).Text($"Operations").Style(headingStyle);

                    //Table Operations
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Purchasing's Preliminary Decision");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{Model.Operations.PrelDecision.Decision}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("Was a CAR raised?");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{(Model.Operations.CarRaised ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("Follow up required?");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{(Model.Operations.IsFollowUpReq ? "Yes" : "No")}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Second section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Message");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{Model.Operations.Message}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("CAR Number");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{Model.Operations.CarNum}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("Expected date of follow up");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{Model.Operations.ExpecDate}");

                        table.Cell().Row(1).Column(4).Element(BlockTitle).Text("Follow up type");
                        table.Cell().Row(2).Column(4).Element(BlockContent).Text($"{Model.Operations.FollowUpType}");

                        // for simplicity, you can also use extension method described in the "Extending DSL" section
                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(35)
                                .MinHeight(70)
                                .AlignCenter()
                                .AlignMiddle();
                        }

                    });

                    //Author
                    column.Item().PaddingTop(12).Text(text =>
                    {
                        text.Span("Author: ").SemiBold();
                        text.Span($"{Model.Operations.OpManagerSign}");
                    });
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Completed on: ").SemiBold();
                        text.Span($"{Model.Operations.OperationsDate:d}");
                    });
                });
        }

        void Procurement(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");

            container
                .PaddingTop(20)
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).Text($"Procurement").Style(headingStyle);

                    //Table Procurement
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Does the supplier wants the items returned?");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{(Model.Procurement.SuppItemsBack ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("Has supplier return been completed in SAP?");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{(Model.Procurement.SuppReturnCompleted ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(3).Element(BlockTitle).Text("Is credit expected?");
                        table.Cell().Row(2).Column(3).Element(BlockContent).Text($"{(Model.Procurement.IsCreditExpec ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(4).Element(BlockTitle).Text("Charge supplier for expenses?");
                        table.Cell().Row(2).Column(4).Element(BlockContent).Text($"{(Model.Procurement.ChargeSupplier ? "Yes" : "No")}");

                        table.Cell().Row(1).Column(5).Element(BlockTitle).Text("NCR Value");
                        table.Cell().Row(2).Column(5).Element(BlockContent).Text($"${Model.Procurement.NCRValue}");

                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Second section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("RMA number");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{Model.Procurement.RMANo}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("When replaced/reworked items expected to be returned");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{Model.Procurement.ExpecDateOfReturn}");


                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });
                    //Third section of the table
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(200);
                            columns.RelativeColumn();
                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Carrier Informarion");
                        table.Cell().Row(1).Column(2).Element(BlockContent).Text($"{Model.Procurement.CarrierInfo}");

                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }

                    });


                    //Author
                    column.Item().PaddingTop(12).Text(text =>
                        {
                            text.Span("Author: ").SemiBold();
                            text.Span($"{Model.Procurement.ProcurementSign}");
                        });
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Completed on: ").SemiBold();
                        text.Span($"{Model.Procurement.ProcurementDate:d}");
                    });
                });
        }

        void Reinspection(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");

            container
                .PaddingTop(20)
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).Text($"Reinspection").Style(headingStyle);

                    //Table Procurement
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();

                        });

                        table.Cell().Row(1).Column(1).Element(BlockTitle).Text("Reinspection accepted?");
                        table.Cell().Row(2).Column(1).Element(BlockContent).Text($"{Model.Reinspection.ReinspecAccepted}");

                        table.Cell().Row(1).Column(2).Element(BlockTitle).Text("New NCR number");
                        table.Cell().Row(2).Column(2).Element(BlockContent).Text($"{Model.Reinspection.NewNCRNum}");

                        static IContainer BlockTitle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.Grey.Lighten3)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                        static IContainer BlockContent(IContainer container)
                        {
                            return container
                                .Border(1)
                                .Background(Colors.White)
                                .ShowOnce()
                                .MinWidth(50)
                                .MinHeight(50)
                                .AlignCenter()
                                .AlignMiddle();
                        }
                    });

                    //Author
                    column.Item().PaddingTop(12).Text(text =>
                    {
                        text.Span("Author: ").SemiBold();
                        text.Span($"{Model.Reinspection.ReinspecInspectorSign}");
                    });
                    column.Item().PaddingTop(5).Text(text =>
                    {
                        text.Span("Completed on: ").SemiBold();
                        text.Span($"{Model.Reinspection.ReinspectionDate:d}");
                    });
                });
        }

        void Multimedia(IContainer container)
        {
            var headingStyle = TextStyle.Default.FontSize(14).SemiBold().FontColor("#000000");

            container
                .AlignCenter()
                .AlignMiddle()
                .Column(column =>
                {
                    //Heading
                    column.Item().PaddingBottom(12).AlignLeft().Text($"Multimedia").Style(headingStyle);

                    //Quality Rep's Multimedia
                    if ((Model.QualityRepresentative?.QualityPhotos?.Any() ?? false) || (Model.QualityRepresentative?.VideoLinks?.Any() ?? false))
                    {
                        column.Item().PaddingBottom(6).Text("Quality Representative").SemiBold();

                        if (Model.QualityRepresentative.QualityPhotos?.Any() ?? false)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                int rowCounter = 1;

                                int columnCounter = 1;
                                foreach (var photo in Model.QualityRepresentative.QualityPhotos)
                                {
                                    table.Cell().Row((uint)rowCounter).Column((uint)columnCounter).Element(BlockContent).Image(photo.Content);
                                    if (columnCounter == 2)
                                    {
                                        rowCounter++;
                                        columnCounter--;
                                    }
                                    else
                                    {
                                        columnCounter++;
                                    }

                                }

                            });

                        }
                        column.Item().PaddingBottom(5).BorderColor(Colors.White);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            int rowCounter = 1;
                            if (Model.QualityRepresentative.VideoLinks?.Any() ?? false)
                            {
                                foreach (var link in Model.QualityRepresentative.VideoLinks)
                                {
                                    table.Cell().Row((uint)rowCounter).Column(1).Element(BlockContentLink).Text($"- {link.Link}");
                                    rowCounter++;
                                }
                            }

                        });
                    }

                    column.Item().PaddingBottom(22).BorderColor(Colors.White);
                    //Engineering's Multimedia
                    if ((Model.Engineering?.QualityPhotos?.Any() ?? false) || (Model.Engineering?.VideoLinks?.Any() ?? false))
                    {
                        column.Item().PaddingBottom(6).Text("Engineering").SemiBold();

                        if (Model.Engineering.QualityPhotos?.Any() ?? false)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                int rowCounter = 1;
                            
                            
                                int columnCounter = 1;
                                foreach (var photo in Model.Engineering.QualityPhotos)
                                {
                                    table.Cell().Row((uint)rowCounter).Column((uint)columnCounter).Element(BlockContent).Image(photo.Content);
                                    if (columnCounter == 2)
                                    {
                                        rowCounter++;
                                        columnCounter--;
                                    }
                                    else
                                    {
                                        columnCounter++;
                                    }

                                }
                            

                            });
                        }
                        column.Item().PaddingBottom(5).BorderColor(Colors.White);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            int rowCounter = 1;
                            if (Model.Engineering.VideoLinks?.Any() ?? false)
                            {
                                foreach (var link in Model.Engineering.VideoLinks)
                                {
                                    table.Cell().Row((uint)rowCounter).Column(1).Element(BlockContentLink).Text($"- {link.Link}");
                                    rowCounter++;
                                }
                            }

                        });
                    }

                    column.Item().PaddingBottom(22).BorderColor(Colors.White);
                    //Operations's Multimedia
                    if ((Model.Operations?.QualityPhotos?.Any() ?? false) || (Model.Operations?.VideoLinks?.Any() ?? false))
                    {
                        column.Item().PaddingBottom(6).Text("Operations").SemiBold();
                        if (Model.Operations.QualityPhotos?.Any() ?? false) 
                        { 
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                int rowCounter = 1;
                                int columnCounter = 1;
                                foreach (var photo in Model.Operations.QualityPhotos)
                                {
                                    table.Cell().Row((uint)rowCounter).Column((uint)columnCounter).Element(BlockContent).Image(photo.Content);
                                    if (columnCounter == 2)
                                    {
                                        rowCounter++;
                                        columnCounter--;
                                    }
                                    else
                                    {
                                        columnCounter++;
                                    }

                                }

                            });
                        }

                        column.Item().PaddingBottom(5);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            int rowCounter = 1;
                            if (Model.Operations.VideoLinks?.Any() ?? false)
                            {
                                foreach (var link in Model.Operations.VideoLinks)
                                {
                                    table.Cell().Row((uint)rowCounter).Column(1).Element(BlockContentLink).Text($"- {link.Link}");
                                    rowCounter++;
                                }
                            }

                        });
                    }

                    column.Item().PaddingBottom(22);
                    //Procurement's Multimedia
                    if ((Model.Procurement?.QualityPhotos?.Any() ?? false) || (Model.Procurement?.VideoLinks?.Any() ?? false))
                    {
                        column.Item().PaddingBottom(6).Text("Procurement").SemiBold();

                        if (Model.Procurement.QualityPhotos?.Any() ?? false)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                int rowCounter = 1;
                                    int columnCounter = 1;
                                    foreach (var photo in Model.Procurement.QualityPhotos)
                                    {
                                        table.Cell().Row((uint)rowCounter).Column((uint)columnCounter).Element(BlockContent).Image(photo.Content);
                                        if (columnCounter == 2)
                                        {
                                            rowCounter++;
                                            columnCounter--;
                                        }
                                        else
                                        {
                                            columnCounter++;
                                        }

                                    }

                            });
                        }
                        column.Item().PaddingBottom(5).BorderColor(Colors.White);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            int rowCounter = 1;
                            if (Model.Procurement.VideoLinks?.Any() ?? false)
                            {
                                foreach (var link in Model.Procurement.VideoLinks)
                                {
                                    table.Cell().Row((uint)rowCounter).Column(1).Element(BlockContentLink).Text($"- {link.Link}");
                                    rowCounter++;
                                }
                            }

                        });
                    }

                    column.Item().PaddingBottom(22).BorderColor(Colors.White);
                    //Procurement's Multimedia
                    if ((Model.Reinspection?.QualityPhotos?.Any() ?? false) || (Model.Reinspection?.VideoLinks?.Any() ?? false))
                    {
                        column.Item().PaddingBottom(6).Text("Reinspection").SemiBold();

                        if (Model.Reinspection.QualityPhotos?.Any() ?? false)
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                int rowCounter = 1;
                            
                                int columnCounter = 1;
                                foreach (var photo in Model.Reinspection.QualityPhotos)
                                {
                                    table.Cell().Row((uint)rowCounter).Column((uint)columnCounter).Element(BlockContent).Image(photo.Content);
                                    if (columnCounter == 2)
                                    {
                                        rowCounter++;
                                        columnCounter--;
                                    }
                                    else
                                    {
                                        columnCounter++;
                                    }

                                }
                            });
                        }
                        column.Item().PaddingBottom(5).BorderColor(Colors.White);
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                            });

                            int rowCounter = 1;
                            if (Model.Reinspection.VideoLinks?.Any() ?? false)
                            {
                                foreach (var link in Model.Reinspection.VideoLinks)
                                {
                                    table.Cell().Row((uint)rowCounter).Column(1).Element(BlockContentLink).Text($"- {link.Link}");
                                    rowCounter++;
                                }
                            }

                        });
                        
                    }

                    static IContainer BlockContent(IContainer container)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .ShowOnce()
                            .MinWidth(50)
                            .MaxHeight(300)
                            .AlignCenter()
                            .AlignMiddle();
                    }
                    static IContainer BlockContentLink(IContainer container)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.White)
                            .ShowOnce()
                            .MinWidth(50)
                            .MinHeight(40)
                            .AlignCenter()
                            .AlignMiddle();
                    }

                });
        }
    }


}
