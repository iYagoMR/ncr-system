using Haver.Models;
using Haver.Utilities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Org.BouncyCastle.Crypto.Tls;
using System;

namespace Haver.Data
{
    public static class HaverInitializer
    {
        public static void Seed(IApplicationBuilder applicationBuilder)
        {
            HaverContext context = applicationBuilder.ApplicationServices.CreateScope()
                .ServiceProvider.GetRequiredService<HaverContext>();
            try
            {
                //context.Database.EnsureDeleted();
                context.Database.Migrate();

                if (!context.Employees.Any()) //Example, might be different names
                {
                    context.Employees.AddRange(
                     new Employee
                     {
                         FirstName = "Iago",
                         LastName = "Romao",
                         Email = "iago.romao5@gmail.com",
                     },
                     new Employee
                     {
                         FirstName = "Fred",
                         LastName = "Flintstone",
                         Email = "admin@outlook.com"
                     },
                    new Employee
                    {
                        FirstName = "Peter",
                        LastName = "Parker",
                        Email = "engineer@outlook.com"
                    },
                    new Employee
                    {
                        FirstName = "Margaret",
                        LastName = "Thompson",
                        Email = "procurement@outlook.com"
                    },
                    new Employee
                    {
                        FirstName = "Mary",
                        LastName = "Grayson",
                        Email = "qualityinsp@outlook.com"
                    },
                    new Employee
                    {
                        FirstName = "Jhoon",
                        LastName = "Cook",
                        Email = "opmanager@outlook.com"
                    });

                    context.SaveChanges();
                }

                if (!context.ConfigurationVariables.Any()) //Example, might be different names
                {
                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                    // Calculate the start of tomorrow's day
                    var tomorrowStart = now.Date.AddDays(1);

                    context.ConfigurationVariables.AddRange(
                    new ConfigurationVariable
                    {
                        ArchiveNCRsYears = 5,
                        OverdueNCRsNotificationDays = 25,
                        DateToRunNotificationJob = tomorrowStart,
                        DateToRunArchiveJob = tomorrowStart,
                    });
                    context.SaveChanges();
                }

                //if (!context.Notifications.Any()) //Example, might be different names
                //{
                //    var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                //    context.Notifications.AddRange(
                //    new Notification
                //    {
                //        Title = "NCR overdue",
                //        Type = "createFill",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas. <a href='/NCR/Details/1'>View NCR details</a>",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "edited",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "rejected",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "overdueFill",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "rejectedTwice",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "close",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    },
                //    new Notification
                //    {
                //        Title = "NCR overdue two",
                //        Type = "overdueNCR",
                //        Message = "asdaknsddddddd asdaasdasda asdmaknsdonaosdn asdasd asdas dasdknaslknd",
                //        CreateOn = nowToronto,
                //        EmployeeID = 1
                //    });

                //    context.SaveChanges();
                //}

                // Seed data for Parts if there aren't any.
                string[] parts = new string[] { "Shaft, ~ 80 T-C 3 36 LOS 0 0", "Balance Weight, M 900 T/F-C 137 0.5", "Body Bracket, ~ 900 F-C ~ ~ F RM ST", "balance weight, M 900 T/F-C 186.7 0.5", "Shaft, ~ 100 F-C 5 0 LOS 0.1875 0", "bracket, ~ H-C SI", "bracket, ~ F-C RH G", "V-Belt Pulley, 2 3V 5.3", "feed box, M 8/9/1100 T/F/L-C 7 0 ST ~ ~", "back plate, M 8/9/1100 T/F-C 7 0 M SD ~" };
                int[] partsNumber = new int[] { 200000046, 200000114, 200000503, 200000541, 200000572, 200000589, 200000626, 200000879, 200000978, 200001234 };
                if (!context.Parts.Any())
                {
                    int partNumberIndexCounter = 0;
                    foreach (string p in parts)
                    {
                        Part part = new Part
                        {
                            PartDesc = p,
                            PartNumber = partsNumber[partNumberIndexCounter]
                        };
                        context.Parts.Add(part);

                        partNumberIndexCounter++;
                    }
                    context.SaveChanges();
                }

                // Seed data for Suppliers if there aren't any.
                string[] suppliers = new string[] { "AJAX TOCCO", "HINGSTON METAL FABRICATORS", "HOTZ ENVIRONMENTAL SERVICES", "BLACK CREEK METAL", "POLYMER EXTRUSIONS INC", "DON CASSELMAN & SON LTD", "WAFIOS MACHINERY CORPORATION", "C.H.R. INDUSTRIES INC.", "PHILPOTT RUBBER COMPANY", "BALDOR ELECTRIC COMPANY" };
                int[] suppliersNumber = new int[] { 700009, 700013, 700027, 700044, 700045, 700087, 700092, 700094, 700098, 700099 };
                if (!context.Suppliers.Any())
                {
                    int supplierNumberIndexCounter = 0;
                    foreach (string s in suppliers)
                    {
                        Supplier supplier = new Supplier
                        {
                            SupplierName = s,
                            SupplierCode = suppliersNumber[supplierNumberIndexCounter]
                        };
                        context.Suppliers.Add(supplier);

                        supplierNumberIndexCounter++;
                    }
                    context.SaveChanges();
                }

                // Seed data for Problems if there aren't any.
                string[] problems = new string[] { "Design Error (Drawing)", "Holes not tapped", "Incorrect dimensions", "Incorrect hardware", "Incorrect hole (size, locations, missing)", "Incorrect thread size", "Not Painted", "Poor Paint finish", "Poor quality surface finish", "Poor Weld quality" };
                if (!context.Problems.Any())
                {
                    foreach (string pr in problems)
                    {
                        Problem problem = new Problem
                        {
                            ProblemDescription = pr,
                        };
                        context.Problems.Add(problem);
                    }
                    context.SaveChanges();
                }

                if (!context.ProcessesApplicable.Any())
                {
                    context.ProcessesApplicable.AddRange(
                        new ProcessApplicable
                        {
                            ProcessName = "Supplier or Rec-Insp"
                        }, new ProcessApplicable
                        {
                            ProcessName = "WIP (Production Order)"
                        });
                    context.SaveChanges();
                }
                if (!context.EngReviews.Any())
                {
                    context.EngReviews.AddRange(
                        new EngReview
                        {
                            Review = "Use As Is"
                        }, new EngReview
                        {
                            Review = "Repair"
                        }, new EngReview
                        {
                            Review = "Rework"
                        }, new EngReview
                        {
                            Review = "Scrap"
                        });
                    context.SaveChanges();
                }
                if (!context.PrelDecisions.Any())
                {
                    context.PrelDecisions.AddRange(
                        new PrelDecision
                        {
                            Decision = "Rework \"In-House"
                        }, new PrelDecision
                        {
                            Decision = "Scrap in House"
                        }, new PrelDecision
                        {
                            Decision = "Defer for HBC Engineering Review"
                        }, new PrelDecision
                        {
                            Decision = "Return to Supplier for either \"rework\" or \"replacement"
                        });
                    context.SaveChanges();
                }



                int ncri = 0;
                int prevYearNCRs = 0;
                int currentYearNCRs = 0;
                bool isNewYear = true;
                if (!context.NCRs.Any())
                {
                    Random rnd = new Random();
                    DateTime startDate = new DateTime(2023, 1, 1);
                    DateTime minDateToActive = new DateTime(2024, 3, 15);
                    DateTime endDate = DateTime.Today.AddDays(-7);
                    DateTime now = DateTime.Today;

                    // Keep track of the latest creation date
                    DateTime latestCreatedOn = startDate;

                    //Create variable to count NCR number
                    int ncrCounter = 0;

                    try
                    {
                        while (ncri < 200)
                        {
                            // Generate random dates within the specified range
                            DateTime createdOn = latestCreatedOn.AddDays(rnd.Next(1, 5));
                            if (createdOn >= minDateToActive)
                            {
                                createdOn = latestCreatedOn.AddDays(rnd.Next(1, 2));
                            }
                            DateOnly createdOnDO = DateOnly.FromDateTime(createdOn);

                            if (createdOn > endDate)
                            {
                                throw new Exception("Date bigger than maximum date");
                            }

                            // Update the latestCreatedOn
                            latestCreatedOn = createdOn;

                            // Reset NCRCounter if new year


                            //count NCRs from 2023
                            if (createdOn.Year == 2023)
                            {
                                prevYearNCRs++;

                            }
                            if (createdOn.Year == 2024)
                            {
                                if (isNewYear == true)
                                {
                                    ncrCounter = 0;
                                    isNewYear = false;
                                }
                                //Count NCRs from 2024
                                currentYearNCRs++;
                            }

                            // Generate random numbers between 30 and 1000 for QuantReceived and QuantDefective
                            int quantReceived = rnd.Next(30, 1001);
                            int quantDefective = rnd.Next(30, 1001);

                            //Initialize vars
                            bool qualityRepFilled = false;
                            bool engineeringFilled = false;
                            bool operationsFilled = false;
                            bool procurementFilled = false;
                            bool reinspectionFilled = false;

                            if (createdOn < minDateToActive)
                            {
                                qualityRepFilled = true;
                                engineeringFilled = true;
                                operationsFilled = true;
                                procurementFilled = true;
                                reinspectionFilled = true;
                            }
                            else
                            {
                                // Randomly determine which sections are filled
                                qualityRepFilled = true;
                                engineeringFilled = rnd.Next(2) == 1;
                                operationsFilled = rnd.Next(2) == 1;
                                procurementFilled = rnd.Next(2) == 1;
                                reinspectionFilled = rnd.Next(2) == 1;
                            }

                            bool fillQualRep = false;
                            bool fillEng = false;
                            bool fillOper = false;
                            bool fillProc = false;
                            bool fillReinsp = false;

                            // Determine the phase based on which sections are filled
                            string phase = "Quality Representative";
                            string status = "Active";
                            if (qualityRepFilled)
                            {
                                fillQualRep = true;
                                if (engineeringFilled)
                                {
                                    fillEng = true;
                                    if (operationsFilled)
                                    {
                                        fillOper = true;
                                        if (procurementFilled)
                                        {
                                            fillProc = true;
                                            if (reinspectionFilled)
                                            {
                                                fillReinsp = true;
                                                phase = "Completed";
                                                status = "Closed";
                                            }
                                            else
                                            {
                                                phase = "Reinspection";
                                            }
                                        }
                                        else
                                        {
                                            phase = "Procurement";
                                        }
                                    }
                                    else
                                    {
                                        phase = "Operations";
                                    }
                                }
                                else
                                {
                                    phase = "Engineering";
                                }
                            }

                            // Create NCR object
                            NCR ncr = new NCR
                            {
                                NCRNum = $"{createdOn.Year}-{(ncrCounter + 1):000}", // Generating NCR number based on year and counter
                                Status = status,
                                CreatedOn = createdOn,
                                CreatedOnDO = createdOnDO,
                                Phase = phase,
                                QualityRepresentative = fillQualRep ? new QualityRepresentative
                                {
                                    PoNo = rnd.Next(1000, 10000), // Example of random data
                                    SalesOrd = rnd.Next(1000, 10000).ToString(),
                                    QuantReceived = quantReceived,
                                    QuantDefective = quantDefective,
                                    DescDefect = "Random description of defect",
                                    QualityRepresentativeSign = "Signature",
                                    QualityRepDate = createdOnDO,
                                    CreatedOn = createdOn,
                                    ProblemID = rnd.Next(1, problems.Length), // Assuming you have the problems array accessible here
                                    PartID = rnd.Next(1, parts.Length), // Assuming you have the parts array accessible here
                                    SupplierID = rnd.Next(1, suppliers.Length), // Assuming you have the suppliers array accessible here
                                    ProcessApplicableID = rnd.Next(1, 3), // Assuming there are two processes
                                } : null,
                                Engineering = fillEng ? new Engineering
                                {
                                    CustIssueMsg = "Random customer issue message",
                                    Disposition = "Random disposition",
                                    RevisionedBy = "Random revisor",
                                    EngineerSign = "Engineer signature",
                                    EngineeringDate = createdOnDO.AddDays(rnd.Next(1, 2)), // Adjusting to be closer to creation date
                                    CreatedOn = createdOn.AddDays(rnd.Next(1, 2)),
                                    EngReviewID = rnd.Next(1, 5), // Assuming you have the EngReviews array accessible here
                                } : null,
                                Operations = fillOper ? new Operations
                                {
                                    OpManagerSign = "Operations manager signature",
                                    OperationsDate = createdOnDO.AddDays(rnd.Next(2, 3)), // Adjusting to be closer to creation date
                                    CreatedOn = createdOn.AddDays(rnd.Next(2, 3)),
                                    PrelDecisionID = rnd.Next(1, 5), // Assuming you have the PrelDecisions array accessible here
                                } : null,
                                Procurement = fillProc ? new Procurement
                                {
                                    CarrierInfo = "Random carrier information",
                                    ProcurementSign = "Procurement signature",
                                    ExpecDateOfReturn = createdOn.AddDays(rnd.Next(5, 15)), // Adjusting to be closer to creation date
                                    CreatedOn = createdOn.AddDays(rnd.Next(3, 4)),
                                    ProcurementDate = createdOnDO.AddDays(rnd.Next(3, 4)), // Adjusting to be closer to creation date
                                    NCRValue = rnd.Next(100, 1000), // Example of random data
                                } : null,
                                Reinspection = fillReinsp ? new Reinspection
                                {
                                    ReinspecInspectorSign = "Reinspection signature",
                                    CreatedOn = createdOn.AddDays(rnd.Next(4, 5)),
                                    ReinspectionDate = createdOnDO.AddDays(rnd.Next(4, 5)), // Adjusting to be closer to creation date
                                } : null
                            };
                            context.NCRs.Add(ncr);
                            ncrCounter++;
                            ncri++;
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    //Create a couple more 
                    for (int x = 0; x < 10; x++)
                    {
                        DateTime createdOnPrevDays = now.AddDays(-2);
                        if (x > 4)
                        {
                            createdOnPrevDays = now.AddDays(-1);
                        }
                        else if (x > 7)
                        {
                            createdOnPrevDays = now;
                        }
                        DateOnly createdOnDOPrevDays = DateOnly.FromDateTime(createdOnPrevDays);

                        // Generate random numbers between 30 and 1000 for QuantReceived and QuantDefective
                        int quantReceived = rnd.Next(30, 1001);
                        int quantDefective = rnd.Next(30, 1001);

                        // Randomly determine which sections are filled
                        bool qualityRepFilled = true;
                        bool engineeringFilled = rnd.Next(2) == 1;
                        bool operationsFilled = rnd.Next(2) == 1;
                        bool procurementFilled = rnd.Next(2) == 1;
                        bool reinspectionFilled = rnd.Next(2) == 1;

                        bool fillQualRep = false;
                        bool fillEng = false;
                        bool fillOper = false;
                        bool fillProc = false;
                        bool fillReinsp = false;

                        // Determine the phase based on which sections are filled
                        string phase = "Quality Representative";
                        string status = "Active";
                        if (qualityRepFilled)
                        {
                            fillQualRep = true;
                            if (engineeringFilled)
                            {
                                fillEng = true;
                                if (operationsFilled)
                                {
                                    fillOper = true;
                                    if (procurementFilled)
                                    {
                                        fillProc = true;
                                        if (reinspectionFilled)
                                        {
                                            fillReinsp = true;
                                            phase = "Completed";
                                            status = "Closed";
                                        }
                                        else
                                        {
                                            phase = "Reinspection";
                                        }
                                    }
                                    else
                                    {
                                        phase = "Procurement";
                                    }
                                }
                                else
                                {
                                    phase = "Operations";
                                }
                            }
                            else
                            {
                                phase = "Engineering";
                            }
                        }

                        // Create NCR object
                        NCR ncr = new NCR
                        {
                            NCRNum = $"{createdOnPrevDays.Year}-{(ncrCounter + 1):000}", // Generating NCR number based on year and counter
                            Status = status,
                            CreatedOn = createdOnPrevDays,
                            CreatedOnDO = createdOnDOPrevDays,
                            Phase = phase,
                            QualityRepresentative = fillQualRep ? new QualityRepresentative
                            {
                                PoNo = rnd.Next(1000, 10000), // Example of random data
                                SalesOrd = rnd.Next(1000, 10000).ToString(),
                                QuantReceived = quantReceived,
                                QuantDefective = quantDefective,
                                DescDefect = "Random description of defect",
                                QualityRepresentativeSign = "Signature",
                                QualityRepDate = createdOnDOPrevDays,
                                CreatedOn = createdOnPrevDays,
                                ProblemID = rnd.Next(1, problems.Length), // Assuming you have the problems array accessible here
                                PartID = rnd.Next(1, parts.Length), // Assuming you have the parts array accessible here
                                SupplierID = rnd.Next(1, suppliers.Length), // Assuming you have the suppliers array accessible here
                                ProcessApplicableID = rnd.Next(1, 3), // Assuming there are two processes
                            } : null,
                            Engineering = fillEng ? new Engineering
                            {
                                CustIssueMsg = "Random customer issue message",
                                Disposition = "Random disposition",
                                RevisionedBy = "Random revisor",
                                EngineerSign = "Engineer signature",
                                EngineeringDate = createdOnDOPrevDays.AddDays(rnd.Next(1)), // Adjusting to be closer to creation date
                                CreatedOn = createdOnPrevDays.AddDays(rnd.Next(1)),
                                EngReviewID = rnd.Next(1, 5), // Assuming you have the EngReviews array accessible here
                            } : null,
                            Operations = fillOper ? new Operations
                            {
                                OpManagerSign = "Operations manager signature",
                                OperationsDate = createdOnDOPrevDays.AddDays(rnd.Next(1)), // Adjusting to be closer to creation date
                                CreatedOn = createdOnPrevDays.AddDays(rnd.Next(1)),
                                PrelDecisionID = rnd.Next(1, 5), // Assuming you have the PrelDecisions array accessible here
                            } : null,
                            Procurement = fillProc ? new Procurement
                            {
                                CarrierInfo = "Random carrier information",
                                ProcurementSign = "Procurement signature",
                                ExpecDateOfReturn = createdOnPrevDays.AddDays(rnd.Next(5, 15)), // Adjusting to be closer to creation date
                                CreatedOn = createdOnPrevDays.AddDays(rnd.Next(1)),
                                ProcurementDate = createdOnDOPrevDays.AddDays(rnd.Next(1)), // Adjusting to be closer to creation date
                                NCRValue = rnd.Next(100, 1000), // Example of random data
                            } : null,
                            Reinspection = fillReinsp ? new Reinspection
                            {
                                ReinspecInspectorSign = "Reinspection signature",
                                CreatedOn = createdOnPrevDays.AddDays(rnd.Next(1)),
                                ReinspectionDate = createdOnDOPrevDays.AddDays(rnd.Next(1)), // Adjusting to be closer to creation date
                            } : null
                        };
                        context.NCRs.Add(ncr);
                        ncrCounter++;
                        ncri++;
                        currentYearNCRs++;
                    }

                    context.SaveChanges();
                }

                if (!context.NCRNumbers.Any())
                {
                    int i = 1;
                    while (i < prevYearNCRs + 1)
                    {
                        NCRNumber number = new()
                        {
                            Year = 2023,
                            Counter = i
                        };
                        context.NCRNumbers.Add(number);
                        i++;
                    }
                    i = 1;
                    while (i < currentYearNCRs + 1)
                    {
                        NCRNumber number = new()
                        {
                            Year = 2024,
                            Counter = i
                        };
                        context.NCRNumbers.Add(number);
                        i++;
                    }
                    context.SaveChanges();
                }

                ////Seed NCRs
                //if (!context.NCRs.Any())
                //{
                //    for (int i = 0; i < 100; i++)
                //    {
                //        NCR ncr = new()
                //        {
                //            NCRNum = ,
                //            Status = "Active",
                //            CreatedOn = ,
                //            CreatedOnDO = ,
                //            Phase = "",
                //            QualityRepresentative = new QualityRepresentative
                //            {
                //                PoNo = ,
                //                SalesOrd = ,
                //                QuantReceived = ,
                //                QuantDefective = ,
                //                DescDefect = ,
                //                QualityRepresentativeSign = ,
                //                QualityRepDate = ,
                //                CreatedOn = ,
                //                ProblemID = ,
                //                PartID = ,
                //                SupplierID = ,
                //                ProcessApplicableID = ,
                //            },
                //            Engineering = new Engineering
                //            {
                //                CustIssueMsg = ,
                //                Disposition = ,
                //                RevisionedBy = ,
                //                EngineerSign = ,
                //                EngineeringDate = ,
                //                CreatedOn = ,
                //                EngReviewID = ,
                //            },
                //            Operations = new Operations
                //            {
                //                OpManagerSign = ,
                //                OperationsDate = ,
                //                CreatedOn = ,
                //                PrelDecisionID = ,
                //            },
                //            Procurement = new Procurement
                //            {
                //                CarrierInfo = ,
                //                ProcurementSign = ,
                //                ExpecDateOfReturn = ,
                //                CreatedOn = ,
                //                ProcurementDate = ,
                //                NCRValue = ,
                //            }
                //        };
                //        context.NCRs.Add(ncr);
                //    }
                //    context.SaveChanges();
                //}

            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message);
            }
        }
    }
}
