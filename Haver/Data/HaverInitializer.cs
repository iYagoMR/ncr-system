using Haver.Models;
using Microsoft.EntityFrameworkCore;

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
                         Email = "iago.romao5@gmail.com"
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
                    });

                    context.SaveChanges();
                }



                if (!context.Suppliers.Any())
                {
                    context.Suppliers.AddRange(
                        new Supplier
                        {
                            SupplierName = "Supplier 1"
                        }, new Supplier
                        {
                            SupplierName = "Supplier 2"
                        }, new Supplier
                        {
                            SupplierName = "Supplier 3"
                        }, new Supplier
                        {
                            SupplierName = "Supplier 4"
                        });
                    context.SaveChanges();
                }
                if (!context.Problems.Any())
                {
                    context.Problems.AddRange(
                        new Problem
                        {
                            ProblemDescription = "incorrect fit"
                        }, new Problem
                        {
                            ProblemDescription = "Poor quality surface finish"
                        }, new Problem
                        {
                            ProblemDescription = "Incorrect Dimensions"
                        });
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

                if (!context.Parts.Any())
                {
                    context.Parts.AddRange(
                        new Part
                        {
                            PartDesc = "Part 1",
                            PartNumber = 1231241,
                            SupplierID = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Supplier 3").ID
                        },
                        new Part
                        {
                            PartDesc = "Part 2",
                            PartNumber = 4234125,
                            SupplierID = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Supplier 2").ID
                        },
                        new Part
                        {
                            PartDesc = "Part 3",
                            PartNumber = 1414156,
                            SupplierID = context.Suppliers.FirstOrDefault(s => s.SupplierName == "Supplier 1").ID
                        });
                    context.SaveChanges();
                }


            }
            catch (Exception ex)
            {
                ex = new Exception(ex.Message);
            }
        }
    }
}
