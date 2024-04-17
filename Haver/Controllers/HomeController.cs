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
using Microsoft.AspNetCore.Identity;

namespace Haver.Controllers
{
    [Authorize]
    public class HomeController : ElephantController
    {
        private readonly HaverContext _context;
        private readonly IMyEmailSender _emailSender;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IMyEmailSender emailSender, HaverContext context, UserManager<IdentityUser> userManager)
        {
            _emailSender = emailSender;
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? page, string actionButton, string sortDirection = "desc", string sortField = "NCRNumber")
        {
            var countersConstructor = new CalculateNCRTimeCounters(_context, _userManager);
            //var notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);

            string[] sortOptions = new[] { "NCRNumber" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Where(n => !n.IsNCRArchived)
                                            .Where(n => n.Status == "Active")
                                            .AsNoTracking();

            //notificationGenerator.NotifyUser();

            //Qual Counters
            (List<NCR> qual24, List<NCR> qual48, List<NCR> qual5, List<NCR> qualTotal, List<NCR> lgActive) qualCounters = countersConstructor.QualCounters();
            ViewBag.QualTotal = qualCounters.qualTotal.Count();
            ViewBag.Qual24 = qualCounters.qual24.Count();
            ViewBag.Qual48 = qualCounters.qual48.Count();
            ViewBag.Qual5 = qualCounters.qual5.Count();

            //Eng Counters
            (List<NCR> eng24, List<NCR> eng48, List<NCR> eng5, List<NCR> engTotal, List<NCR> lgActive) engCounters = countersConstructor.EngCounters();
            ViewBag.EngTotal = engCounters.engTotal.Count();
            ViewBag.Eng24 = engCounters.eng24.Count();
            ViewBag.Eng48 = engCounters.eng48.Count();
            ViewBag.Eng5 = engCounters.eng5.Count();

            //oper Counters
            (List<NCR> oper24, List<NCR> oper48, List<NCR> oper5, List<NCR> operTotal, List<NCR> lgActive) operCounters = countersConstructor.OperCounters();
            ViewBag.OperTotal = operCounters.operTotal.Count();
            ViewBag.Oper24 = operCounters.oper24.Count();
            ViewBag.Oper48 = operCounters.oper48.Count();
            ViewBag.Oper5 = operCounters.oper5.Count();

            //proc Counters
            (List<NCR> proc24, List<NCR> proc48, List<NCR> proc5, List<NCR> procTotal, List<NCR> lgActive) procCounters = countersConstructor.ProcCounters();
            ViewBag.ProcTotal = procCounters.procTotal.Count();
            ViewBag.Proc24 = procCounters.proc24.Count();
            ViewBag.Proc48 = procCounters.proc48.Count();
            ViewBag.Proc5 = procCounters.proc5.Count();

            //reinsp Counters
            (List<NCR> reinsp24, List<NCR> reinsp48, List<NCR> reinsp5, List<NCR> reinspTotal, List<NCR> lgActive) reinspCounters = countersConstructor.ReinspCounters();
            ViewBag.ReinspTotal = reinspCounters.reinspTotal.Count();
            ViewBag.Reinsp24 = reinspCounters.reinsp24.Count();
            ViewBag.Reinsp48 = reinspCounters.reinsp48.Count();
            ViewBag.Reinsp5 = reinspCounters.reinsp5.Count();

            //active Counters
            ViewBag.ActiveTotal = qualCounters.qualTotal.Count() + engCounters.engTotal.Count() + operCounters.operTotal.Count() + procCounters.procTotal.Count() + reinspCounters.reinspTotal.Count();
            ViewBag.Active24 = qualCounters.qual24.Count() + engCounters.eng24.Count() + operCounters.oper24.Count() + procCounters.proc24.Count() + reinspCounters.reinsp24.Count();
            ViewBag.Active48 = qualCounters.qual48.Count() + engCounters.eng48.Count() + operCounters.oper48.Count() + procCounters.proc48.Count() + reinspCounters.reinsp48.Count();
            ViewBag.Active5 = qualCounters.qual5.Count() + engCounters.eng5.Count() + operCounters.oper5.Count() + procCounters.proc5.Count() + reinspCounters.reinsp5.Count();

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