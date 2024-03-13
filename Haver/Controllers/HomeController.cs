using Haver.Models;
using Haver.Utilities;
using Haver.Data;
using Haver.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Haver.Controllers
{
    public class HomeController : Controller
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
            string[] sortOptions = new[] { "NCRNumber" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Purchasing)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Where(n => !n.IsNCRArchived)
                                            .Where(n => n.Status == "Active")
                                            .AsNoTracking();
            
            // Filter NCR records with Active phase
            var activeNCRs = _context.NCRs.Where(ncr => ncr.Status == "Active");

            // Count active tables
            ViewBag.ncrCount = _context.NCRs.Count(item => item.Status == "Active");
            ViewBag.QualityRepresentativeCount = activeNCRs.Include(ncr => ncr.QualityRepresentative).Count(item => item.Phase == "Quality Representative");
            ViewBag.EngineeringCount = activeNCRs.Include(ncr => ncr.Engineering).Count(item => item.Phase == "Engineering");
            ViewBag.PurchasingCount = activeNCRs.Include(ncr => ncr.Purchasing).Count(item => item.Phase == "Purchasing");
            ViewBag.ProcurementCount = activeNCRs.Include(ncr => ncr.Procurement).Count(item => item.Phase == "Procurement");
            ViewBag.ReinspectionCount = activeNCRs.Include(ncr => ncr.Reinspection).Count(item => item.Phase == "Reinspection");

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