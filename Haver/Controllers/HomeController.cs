using Haver.Models;
using Haver.Utilities;
using Haver.Data;
using Haver.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using Haver.CustomControllers;
using QuestPDF.Fluent;
using System.IO;

namespace Haver.Controllers
{
    [Authorize]
    public class HomeController : ElephantController
    {
        private readonly HaverContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, HaverContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? page, string actionButton, string sortDirection = "desc", string sortField = "NCRNumber")
        {
            var countersConstructor = new CalculateNCRTimeCounters();

            string[] sortOptions = new[] { "NCRNumber" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Where(n => !n.IsNCRArchived)
                                            .Where(n => n.Status == "Active")
                                            .AsNoTracking();

            // Filter NCR records with Active phase
            var activeNCRs = _context.NCRs.Where(ncr => ncr.Status == "Active")
                                          .Where(ncr => !ncr.IsNCRDraft);

            //Get NCRs in Each section
            var QualNCRs = activeNCRs.Where(ncr => ncr.Phase == "Quality Representative").ToList();
            var EngNCRs = activeNCRs.Include(ncr => ncr.QualityRepresentative).Where(ncr => ncr.Phase == "Engineering").ToList();
            var OperNCRs = activeNCRs.Include(ncr => ncr.Engineering).Include(ncr => ncr.QualityRepresentative).Where(ncr => ncr.Phase == "Operations").ToList();
            var ProcNCRs = activeNCRs.Include(ncr => ncr.Operations).Where(ncr => ncr.Phase == "Procurement").ToList();
            var ReinspNCRs = activeNCRs.Include(ncr => ncr.Procurement).Where(ncr => ncr.Phase == "Reinspection").ToList();

            //Qual Counters
            (int qual24, int qual48, int qual5, int qualTotal) qualCounters = countersConstructor.QualCounters(QualNCRs);
            ViewBag.QualTotal = qualCounters.qualTotal;
            ViewBag.Qual24 = qualCounters.qual24;
            ViewBag.Qual48 = qualCounters.qual48;
            ViewBag.Qual5 = qualCounters.qual5;

            //Eng Counters
            (int eng24, int eng48, int eng5, int engTotal) engCounters = countersConstructor.EngCounters(EngNCRs);
            ViewBag.EngTotal = engCounters.engTotal;
            ViewBag.Eng24 = engCounters.eng24;
            ViewBag.Eng48 = engCounters.eng48;
            ViewBag.Eng5 = engCounters.eng5;

            //oper Counters
            (int oper24, int oper48, int oper5, int operTotal) operCounters = countersConstructor.OperCounters(OperNCRs);
            ViewBag.OperTotal = operCounters.operTotal;
            ViewBag.Oper24 = operCounters.oper24;
            ViewBag.Oper48 = operCounters.oper48;
            ViewBag.Oper5 = operCounters.oper5;

            //proc Counters
            (int proc24, int proc48, int proc5, int procTotal) procCounters = countersConstructor.ProcCounters(ProcNCRs);
            ViewBag.ProcTotal = procCounters.procTotal;
            ViewBag.Proc24 = procCounters.proc24;
            ViewBag.Proc48 = procCounters.proc48;
            ViewBag.Proc5 = procCounters.proc5;

            //reinsp Counters
            (int reinsp24, int reinsp48, int reinsp5, int reinspTotal) reinspCounters = countersConstructor.ReinspCounters(ReinspNCRs);
            ViewBag.ReinspTotal = reinspCounters.reinspTotal;
            ViewBag.Reinsp24 = reinspCounters.reinsp24;
            ViewBag.Reinsp48 = reinspCounters.reinsp48;
            ViewBag.Reinsp5 = reinspCounters.reinsp5;

            //active Counters
            ViewBag.ActiveTotal = qualCounters.qualTotal + engCounters.engTotal + operCounters.operTotal + reinspCounters.reinspTotal;
            ViewBag.Active24 = qualCounters.qual24 + engCounters.eng24 + operCounters.oper24 + reinspCounters.reinsp24;
            ViewBag.Active48 = qualCounters.qual48 + engCounters.eng48 + operCounters.oper48 + reinspCounters.reinsp48;
            ViewBag.Active5 = qualCounters.qual5 + engCounters.eng5 + operCounters.oper5 + reinspCounters.reinsp5;


            //Before we sort, see if we have called for a change of filtering or sorting
            if (!string.IsNullOrEmpty(actionButton)) //Form Submitted!
            {

                page = 1;//Reset Page 

                if (sortOptions.Contains(actionButton))//Change of sort is requested
                {
                    if (actionButton == sortField) //Reverse order on same field
                    {
                        sortDirection = sortDirection == "asc" ? "desc" : "asc";
                    }
                    sortField = actionButton;//Sort by the button clicked
                }
            }

            //Now we know which field and direction to sort by
            if (sortField == "NCRNumber")
            {
                if (sortDirection == "asc")
                {
                    haverContext = haverContext
                        .OrderBy(p => p.NCRNum);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.NCRNum);
                }
            }

            ViewData["sortDirection"] = sortDirection;

            var pagedData = await PaginatedList<NCR>.CreateAsync(haverContext.AsNoTracking(), page ?? 1, 3);

            return View(pagedData);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}