using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Haver.Data;
using Haver.Models;
using Haver.DraftModels;
using Haver.ViewModels;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Haver.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Xml.Linq;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using SQLitePCL;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using NuGet.Packaging;
using Microsoft.AspNetCore.CookiePolicy;
using OfficeOpenXml.Packaging.Ionic.Zip;
using Org.BouncyCastle.Asn1.X509;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using NuGet.DependencyResolver;
using Microsoft.AspNetCore.Routing.Template;
using System.Diagnostics;
using QuestPDF.Fluent;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.AspNetCore.Http;
using Haver.CustomControllers;
using Microsoft.EntityFrameworkCore.Storage;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities.Encoders;
using System.Data;
using Microsoft.AspNetCore.Components.Routing;
using System.Composition;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using Image = SixLabors.ImageSharp.Image;

namespace Haver.Controllers
{
    [Authorize]
    public class NCRController : ElephantController
    {
        private readonly IMyEmailSender _emailSender;
        private readonly HaverContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NCRController(HaverContext context, IMyEmailSender emailSender, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _emailSender = emailSender;
            _userManager = userManager;
        }

        //GET: NCR
        public async Task<IActionResult> Index(string ncrsByActiveTime, string SearchString, string SupplierID, int? page, int? pageSizeID, string SelectedOption
            , string actionButton, string active, string sortDirection = "desc", string sortField = "CREATED ON")
        {

            PopulateList();

            ViewData["activeCheck"] = active;

            int? supSelected = null;

            if (SupplierID != null)
            {
                try
                {
                    int indexSup = SupplierID.IndexOf(' ');
                    string supCode = SupplierID.Substring(0, indexSup);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supCode));
                    if(getSup != null)
                    {
                        supSelected = getSup.ID;
                    }
                }
                catch (Exception)
                {

                }
            }
            if (SupplierID != null)
            {
                ViewData["SupplierSelected"] = SupplierID;
            }
            if(SearchString != null)
            {
                ViewData["NcrNumSelected"] = SearchString;
            }
            

            //Dropdown data
            var ncrsDL = _context.NCRs.ToList();
            ViewBag.NCRsToDL = ncrsDL;

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Problem)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Include(n => n.Procurement)
                                            .Include(n => n.Reinspection)
                                            .Where(n => !n.IsNCRArchived)
                                            .AsNoTracking();

            //Create time variables for each "orverdue period"
            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            var threeDaysAgo = nowToronto.AddDays(-3).Date;
            var twoDaysAgo = nowToronto.AddDays(-2).Date;
            var oneDayAgo = nowToronto.AddDays(-1).Date;


            if (supSelected.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == supSelected);
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                haverContext = haverContext.Where(p => p.NCRNum.Contains(SearchString));
            }
            if (!string.IsNullOrEmpty(SelectedOption))
            {
                haverContext = haverContext.Where(p => p.Phase.Contains(SelectedOption));
            }
            if (active == null)
            {
                haverContext = haverContext.Where(p => p.Status.Contains("Active"));
            }
            if (active != null)
            {
                haverContext = haverContext.Where(p => p.ID != null);
            }
            if (ncrsByActiveTime != null)
            {
                //Fieltering overdue NCRs, by time and by phase
                if (ncrsByActiveTime == "+3d")
                {
                    List<NCR> ncrsToDisplay = new List<NCR>();
                    if(haverContext.Count() > 0)
                    {
                        try
                        {
                            foreach (var ncr in haverContext)
                            {
                                TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                if (SelectedOption == "Quality Representative")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                }
                                else if (SelectedOption == "Engineering")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                }
                                else if (SelectedOption == "Operations")
                                {
                                    if (ncr.Engineering != null && ncr.QualityRepresentative.ConfirmingEng == false)
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.Engineering.CreatedOn);
                                    }
                                    else if(ncr.Engineering == null && ncr.QualityRepresentative.ConfirmingEng == true)
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                    }
                                }
                                else if (SelectedOption == "Procurement")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Operations.CreatedOn);
                                }
                                else if (SelectedOption == "Reinspection")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Procurement.CreatedOn);
                                }

                                var LastFilled = differenceLastFilled.Days;

                                if (LastFilled > 2)
                                {
                                    ncrsToDisplay.Add(ncr);
                                }
                            }
                        }
                        catch(Exception ex)
                        {

                        }
                        haverContext = haverContext.Where(ncr => ncrsToDisplay.Contains(ncr));
                    }

                }
                else if (ncrsByActiveTime == "48h")
                {
                    List<NCR> ncrsToDisplay = new List<NCR>();
                    if (haverContext.Count() > 0)
                    {
                        foreach (var ncr in haverContext)
                        {
                            try
                            {
                                TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                if (SelectedOption == "Quality Representative")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                }
                                else if (SelectedOption == "Engineering")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                }
                                else if (SelectedOption == "Operations")
                                {
                                    if (ncr.QualityRepresentative.ConfirmingEng == true)
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                    }
                                    else
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.Engineering.CreatedOn);
                                    }
                                }
                                else if (SelectedOption == "Procurement")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Operations.CreatedOn);
                                }
                                else if (SelectedOption == "Reinspection")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Procurement.CreatedOn);
                                }

                                var LastFilled = differenceLastFilled.Days;

                                if (LastFilled == 2)
                                {
                                    ncrsToDisplay.Add(ncr);
                                }
                            }
                            catch(Exception ex)
                            {

                            }
                        }
                        haverContext = haverContext.Where(ncr => ncrsToDisplay.Contains(ncr));
                    }


                }
                else if (ncrsByActiveTime == "24h")
                {
                    List<NCR> ncrsToDisplay = new List<NCR>();
                    if (haverContext.Count() > 0)
                    {
                        foreach (var ncr in haverContext)
                        {
                            try
                            {
                                TimeSpan differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                if (SelectedOption == "Quality Representative")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.CreatedOn);
                                }
                                else if (SelectedOption == "Engineering")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                }
                                else if (SelectedOption == "Operations")
                                {
                                    if (ncr.QualityRepresentative.ConfirmingEng == true)
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.QualityRepresentative.CreatedOn);
                                    }
                                    else
                                    {
                                        differenceLastFilled = (TimeSpan)(nowToronto - ncr.Engineering.CreatedOn);
                                    }
                                }
                                else if (SelectedOption == "Procurement")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Operations.CreatedOn);
                                }
                                else if (SelectedOption == "Reinspection")
                                {
                                    differenceLastFilled = (TimeSpan)(nowToronto - ncr.Procurement.CreatedOn);
                                }

                                var LastFilled = differenceLastFilled.Days;

                                if (LastFilled == 1)
                                {
                                    ncrsToDisplay.Add(ncr);
                                }
                            }
                            catch(Exception ex)
                            {

                            }
                            
                        }
                        haverContext = haverContext.Where(ncr => ncrsToDisplay.Contains(ncr));
                    }

                }
            }

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
            if (sortField == "NCR NO.")
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
            if (sortField == "CREATED ON")
            {
                if (sortDirection == "asc")
                {
                    haverContext = haverContext
                        .OrderBy(p => p.CreatedOn)
                        .ThenBy(p => p.NCRNum);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn)
                        .ThenByDescending(p => p.NCRNum);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<NCR>.CreateAsync(haverContext.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: NCR/Archived
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archived(string SearchString, string? SupplierID, int? page, int? pageSizeID, string SelectedOption
            , string actionButton, string active, string sortDirection = "desc", string sortField = "CREATED ON", DateTime? StartDate = null, DateTime? EndDate = null)
        {
            PopulateList();

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Problem)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Where(n => n.IsNCRArchived)
                                            .AsNoTracking();

            int? supSelected = null;

            if (SupplierID != null)
            {
                try
                {
                    int indexSup = SupplierID.IndexOf(' ');
                    string supCode = SupplierID.Substring(0, indexSup);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supCode));
                    if (getSup != null)
                    {
                        supSelected = getSup.ID;
                    }
                }
                catch (Exception)
                {

                }
            }
            if (SupplierID != null)
            {
                ViewData["SupplierSelected"] = SupplierID;
            }
            if (SearchString != null)
            {
                ViewData["NcrNumSelected"] = SearchString;
            }

            //Dropdown data
            var ncrsDL = _context.NCRs.ToList();
            ViewBag.NCRsToDL = ncrsDL;

            if (supSelected.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == supSelected);
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                haverContext = haverContext.Where(p => p.NCRNum.Contains(SearchString));
            }
            if (!string.IsNullOrEmpty(SelectedOption))
            {
                haverContext = haverContext.Where(p => p.Phase.Contains(SelectedOption));
            }
            if (StartDate != null)
            {
                haverContext = haverContext.Where(p => p.CreatedOn >= StartDate);
            }
            if (EndDate != null)
            {
                haverContext = haverContext.Where(p => p.CreatedOn <= EndDate);
            }

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
            if (sortField == "NCR NO.")
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
            if (sortField == "CREATED ON")
            {
                if (sortDirection == "asc")
                {
                    haverContext = haverContext
                        .OrderBy(p => p.CreatedOn)
                        .ThenBy(p => p.NCRNum);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn)
                        .ThenByDescending(p => p.NCRNum);
                }
            }

            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<NCR>.CreateAsync(haverContext.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        [Authorize(Roles = "Admin")]
        // GET: NCR/Archiving
        public async Task<IActionResult> Archiving(string SearchString, string SupplierID, int? page, int? pageSizeID, string SelectedOption
            , string actionButton, string active, string sortDirection = "desc", string sortField = "CREATED ON", DateTime? StartDate = null, DateTime? EndDate = null)
        {
            PopulateList();

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Problem)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Where(n => !n.IsNCRArchived)
                                            .AsNoTracking();

            int? supSelected = null;

            if (SupplierID != null)
            {
                try
                {
                    int indexSup = SupplierID.IndexOf(' ');
                    string supCode = SupplierID.Substring(0, indexSup);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supCode));
                    if (getSup != null)
                    {
                        supSelected = getSup.ID;
                    }
                }
                catch (Exception)
                {

                }
            }
            if (SupplierID != null)
            {
                ViewData["SupplierSelected"] = SupplierID;
            }
            if (SearchString != null)
            {
                ViewData["NcrNumSelected"] = SearchString;
            }

            //Dropdown data
            var ncrsDL = _context.NCRs.ToList();
            ViewBag.NCRsToDL = ncrsDL;

            if (supSelected.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == supSelected);
            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                haverContext = haverContext.Where(p => p.NCRNum.Contains(SearchString));
            }
            if (!string.IsNullOrEmpty(SelectedOption))
            {
                haverContext = haverContext.Where(p => p.Phase.Contains(SelectedOption));
            }
            if (StartDate != null)
            {
                haverContext = haverContext.Where(p => p.CreatedOn >= StartDate);
            }
            if (EndDate != null)
            {
                haverContext = haverContext.Where(p => p.CreatedOn <= EndDate);
            }
            if (active != null)
            {
                haverContext = haverContext.Where(p => p.Status.Contains("Active"));
            }
            if (active == null)
            {
                haverContext = haverContext.Where(p => p.ID != null);
            }

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
            if (sortField == "NCR NO.")
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
            if (sortField == "CREATED ON")
            {
                if (sortDirection == "asc")
                {
                    haverContext = haverContext
                        .OrderBy(p => p.CreatedOn)
                        .ThenBy(p => p.NCRNum);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn)
                        .ThenByDescending(p => p.NCRNum);
                }
            }
            //Set sort for next time
            ViewData["sortField"] = sortField;
            ViewData["sortDirection"] = sortDirection;

            int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID);
            ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
            var pagedData = await PaginatedList<NCR>.CreateAsync(haverContext.AsNoTracking(), page ?? 1, pageSize);

            return View(pagedData);
        }

        // GET: NCR/Analytics
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> Analytics(int? page, string actionButton, string sortDirection = "desc", string sortField = "NCRNumber")
        {

            //Create list for periods
            List<string> items = new List<string>() { "Monthly", "6 Months", "Yearly", "3 Years" };
            ViewBag.Items = items;
            //Retrieve list values for parts
            var parts = _context.Parts.ToList();
            ViewBag.Parts = parts;
            //Retrieve list values for suppliers
            var suppliers = _context.Suppliers.ToList();
            ViewBag.Suppliers = suppliers;

            string[] sortOptions = new[] { "NCRNumber" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Operations)
                                            .Include(n => n.QualityRepresentative)
                                                .Include(n => n.QualityRepresentative.Supplier)
                                            .Include(n => n.Procurement)
                                            .Include(n => n.Reinspection)
                                            .Where(n => n.Status == "Active")
                                            .AsNoTracking();

            // Filter NCR records with Active phase
            var activeNCRs = _context.NCRs.Where(ncr => ncr.Status == "Active");

            // Count active tables
            ViewBag.ncrCount = _context.NCRs.Count(item => item.Status == "Active");
            ViewBag.QualityRepresentativeCount = activeNCRs.Include(ncr => ncr.QualityRepresentative).Count(item => item.Phase == "Quality Representative");
            ViewBag.EngineeringCount = activeNCRs.Include(ncr => ncr.Engineering).Count(item => item.Phase == "Engineering");
            ViewBag.OperationsCount = activeNCRs.Include(ncr => ncr.Operations).Count(item => item.Phase == "Operations");
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

        public IActionResult SuppliersList(string actionButton, string sortDirection = "desc", string sortField = "TOTAL", string period = "Monthly", int page = 1)
        {
            try
            {
                string[] sortOptions = new[] { "TOTAL" };

                //Method to get time span
                var timeSpan = GetTimeSpan(period);

                //Get parts
                var suppliers = _context.Suppliers.ToList();
                var defectivePartsList = new List<PartsDefectiveVM>();

                //Retrieve a list containing the parts Linked to each Quality Representative section
                foreach (var supplier in suppliers)
                {
                    List<QualityRepresentative> items = GetQualRepsBySupplier(Convert.ToString(supplier.SupplierCode), timeSpan, false);

                    int sumQuantDefective = items.Sum(p => p.QuantDefective);

                    //Method to calculate PoP change
                    (double periodChange, int total, int previousTotal) periodChange = PoPChange(Convert.ToString(supplier.SupplierCode), timeSpan, "supplier");

                    PartsDefectiveVM partDefective = new PartsDefectiveVM
                    {
                        SupplierName = supplier.SupplierName,
                        PartsDefectiveAmount = sumQuantDefective,
                        PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : Math.Abs(periodChange.periodChange),
                        IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true
                    };

                    // Add to the list, only if quantity defective is bigger than zero
                    if (sumQuantDefective > 0)
                    {
                        defectivePartsList.Add(partDefective);
                    }
                }

                //Before we sort, see if we have called for a change of filtering or sorting
                if (!string.IsNullOrEmpty(actionButton)) //Form Submitted!
                {

                    //page = 1;//Reset Page 

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
                if (sortField == "TOTAL")
                {
                    if (sortDirection == "asc")
                    {
                        defectivePartsList = defectivePartsList.OrderBy(d => d.PartsDefectiveAmount).ToList();
                    }
                    else
                    {
                        defectivePartsList = defectivePartsList.OrderByDescending(d => d.PartsDefectiveAmount).ToList();
                    }
                }

                ViewData["sortDirection"] = sortDirection;

                // Pagination
                int pageSize = 4; // Number of items per page
                int recordsInTheList = defectivePartsList.Count();
                int numbOfPages = (int)Math.Ceiling((double)recordsInTheList / pageSize);

                var pagedData = defectivePartsList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var response = new ComposedAnalyticsListsVM
                {
                    PagedData = pagedData,
                    NumbOfPages = numbOfPages,
                    PageNum = page
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public IActionResult SuppliersChartModel(string period = null, string supplierSelected = null)
        {
            PartsDefectiveVM partsDefectiveVM = new PartsDefectiveVM();
            try
            {

                //Format partSelected
                int index = supplierSelected.IndexOf(' ');
                string supplierNumber = supplierSelected.Substring(0, index);

                //Method to get time span
                var timeSpan = GetTimeSpan(period);
                //Format date
                string formattedStartDate = timeSpan[0].ToString("MMM d, yyyy"); // Format start date
                string formattedEndDate = timeSpan[1].ToString("MMM d, yyyy"); // Format end date

                // Put in a list all the Qual Rep forms associated with the Part
                var qualRepsLinkedToSupplier = GetQualRepsBySupplier(supplierNumber, timeSpan, false);

                //Is data enough?
                partsDefectiveVM.EnoughData = qualRepsLinkedToSupplier.Count() <= 1 ? false : true;

                (double periodChange, int total, int previousTotal) periodChange = PoPChange(supplierNumber, timeSpan, "supplier");

                //Assign values to ViewModel
                partsDefectiveVM.StartDate = formattedStartDate;
                partsDefectiveVM.EndDate = formattedEndDate;
                partsDefectiveVM.PartsDefectiveAmount = periodChange.total;
                partsDefectiveVM.PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : Math.Abs(periodChange.periodChange);
                partsDefectiveVM.IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true;

                return Ok(partsDefectiveVM);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public List<object> GenSuppliersChart(string period, string supplierSelected)
        {
            try
            {
                if (supplierSelected != null)
                {
                    //Format partSelected
                    int index = supplierSelected.IndexOf(' ');
                    string supplierNumber = supplierSelected.Substring(0, index);

                    //Method to get time span
                    var timeSpan = GetTimeSpan(period);

                    // Put in a list all the Qual Rep forms associated with the Part
                    var qualRepsLinkedToSupplier = GetQualRepsBySupplier(supplierNumber, timeSpan, false);

                    if (qualRepsLinkedToSupplier.Count() == 0)
                    {
                        return null;
                    }

                    List<object> data = new List<object>();
                    List<int> total = new List<int>();
                    List<string> recordDates = new List<string>();
                    List<string> chartLabels = new List<string>();

                    // Iterate through each day within the time span
                    for (DateOnly date = timeSpan[0]; date <= timeSpan[1];)
                    {
                        // Add the string representation of the date to the list
                        if (period == "Monthly")
                        {
                            chartLabels.Add(date.ToString("MM-dd"));
                            date = date.AddDays(1);
                        }
                        //else if (period == "3 Years")
                        //{
                        //    // Calculate the quarter
                        //    int quarter = (date.Month - 1) / 3 + 1;
                        //    string quarterLabel = $"{date.Year}-{quarter}";
                        //    chartLabels.Add(quarterLabel);

                        //    date = date.AddMonths(1);
                        //}
                        else
                        {
                            chartLabels.Add(date.ToString("yyyy-MM"));
                            date = date.AddMonths(1);
                        }
                    }


                    //Loop trhough the qualRep's and add to the lists the total and months
                    foreach (var supplier in qualRepsLinkedToSupplier)
                    {
                        total.Add(supplier.QuantDefective);

                        //Match the index from the chartLabels with the QualityRepDate's
                        if (period == "Monthly")
                        {
                            //var chartLabel = chartLabels.Where(cl => DateOnly.Parse(cl) == qualRep.QualityRepDate
                            //                                      || DateOnly.Parse(cl).AddDays(1) == qualRep.QualityRepDate
                            //                                      || DateOnly.Parse(cl).AddDays(2) == qualRep.QualityRepDate).FirstOrDefault();

                            var chartIndex = chartLabels.IndexOf(supplier.QualityRepDate.ToString("MM-dd"));
                            recordDates.Add(chartIndex.ToString());
                        }
                        //else if(period == "3 Years")
                        //{
                        //    var chartLabel = chartLabels.FirstOrDefault(cl => DateOnly.Parse(cl).ToString("yyyy-MM") == qualRep.QualityRepDate.ToString("yyyy-MM"));
                        //    var chartIndex = chartLabels.IndexOf(chartLabel);
                        //    recordDates.Add(chartIndex.ToString());
                        //}
                        else
                        {
                            var chartLabel = chartLabels.FirstOrDefault(cl => DateOnly.Parse(cl).ToString("yyyy-MM") == supplier.QualityRepDate.ToString("yyyy-MM"));
                            var chartIndex = chartLabels.IndexOf(chartLabel);
                            recordDates.Add(chartIndex.ToString());
                        }
                    }

                    data.Add(chartLabels);
                    data.Add(total);
                    data.Add(recordDates);

                    return data;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public IActionResult GetNCRTimeList(string actionButton, string sortDirection = "desc", string sortField = "SINCE CREATION", int page = 1)
        {
            try
            {
                string[] sortOptions = new[] { "SINCE CREATION", "LAST FILLED" };


                //Get parts
                var NCRs = _context.NCRs
                    .Where(ncr => ncr.Status == "Active")
                    .Where(ncr => !ncr.IsNCRArchived)
                    .Include(ncr => ncr.QualityRepresentative.Problem)
                    .Include(ncr => ncr.QualityRepresentative)
                    .Include(ncr => ncr.Engineering)
                    .Include(ncr => ncr.Procurement)
                    .Include(ncr => ncr.Operations)
                    .Include(ncr => ncr.Reinspection)
                    .ToList();
                var ncrTimeList = new List<NCRTimeListVM>();

                var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                //Retrieve a list containing the parts Linked to each Quality Representative section
                foreach (var NCR in NCRs)
                {
                    //Get the number of days since last filled
                    TimeSpan differenceLastFilled;
                    NCRTimeListVM ncrRecord = new NCRTimeListVM();

                    if (NCR.Phase == "Engineering")
                    {
                        differenceLastFilled = (TimeSpan)(nowToronto - NCR.QualityRepresentative?.CreatedOn);
                        ncrRecord.LastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Operations")
                    {

                        differenceLastFilled = (TimeSpan)(nowToronto - NCR.Engineering?.CreatedOn);
                        ncrRecord.LastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Procurement")
                    {
                        differenceLastFilled = (TimeSpan)(nowToronto - NCR.Operations?.CreatedOn);
                        ncrRecord.LastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Reinspection")
                    {
                        differenceLastFilled = (TimeSpan)(nowToronto - NCR.Procurement?.CreatedOn);
                        ncrRecord.LastFilled = differenceLastFilled.Days;
                    }

                    //Get the number of days since created
                    TimeSpan differenceCreated;
                    int sinceCreated = 0;

                    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

                    differenceCreated = now - Convert.ToDateTime(NCR.CreatedOn);
                    sinceCreated = differenceCreated.Days;

                    ncrRecord.NCRNo = NCR.NCRNum;
                    ncrRecord.Problem = NCR.QualityRepresentative?.Problem.ProblemDescription;
                    ncrRecord.Phase = NCR.Phase;
                    ncrRecord.SinceCreated = sinceCreated;

                    // Add to the list
                    if (sinceCreated >= 1)
                    {
                        ncrTimeList.Add(ncrRecord);
                    }
                }

                //Before we sort, see if we have called for a change of filtering or sorting
                if (!string.IsNullOrEmpty(actionButton)) //Form Submitted!
                {

                    //page = 1;//Reset Page 

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
                if (sortField == "SINCE CREATION")
                {
                    if (sortDirection == "asc")
                    {
                        ncrTimeList = ncrTimeList.OrderBy(d => d.SinceCreated).ToList();
                    }
                    else
                    {
                        ncrTimeList = ncrTimeList.OrderByDescending(d => d.SinceCreated).ToList();
                    }
                }
                else if (sortField == "LAST FILLED")
                {
                    if (sortDirection == "asc")
                    {
                        ncrTimeList = ncrTimeList.OrderBy(d => d.LastFilled).ToList();
                    }
                    else
                    {
                        ncrTimeList = ncrTimeList.OrderByDescending(d => d.LastFilled).ToList();
                    }
                }

                //Set sort for next time
                ViewData["sortField"] = sortField;
                ViewData["sortDirection"] = sortDirection;

                // Pagination
                int pageSize = 5; // Number of items per page
                int recordsInTheList = ncrTimeList.Count();
                int numbOfPages = (int)Math.Ceiling((double)recordsInTheList / pageSize);

                var pagedData = ncrTimeList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var response = new ComposedAnalyticsListsVM
                {
                    NCRTPagedData = pagedData,
                    NumbOfPages = numbOfPages,
                    PageNum = page
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public IActionResult GetPartsDefective(string actionButton, string sortDirection = "desc", string sortField = "TOTAL", string period = "Monthly", int page = 1)
        {
            try
            {
                string[] sortOptions = new[] { "TOTAL" };

                //Method to get time span
                var timeSpan = GetTimeSpan(period);

                //Get parts
                var parts = _context.Parts.ToList();
                var defectivePartsList = new List<PartsDefectiveVM>();

                //Retrieve a list containing the parts Linked to each Quality Representative section
                foreach (var part in parts)
                {
                    List<QualityRepresentative> items = GetQualRepsByPart(Convert.ToString(part.PartNumber), timeSpan, false);

                    int sumQuantDefective = items.Sum(p => p.QuantDefective);

                    //Method to calculate PoP change
                    (double periodChange, int total, int previousTotal) periodChange = PoPChange(Convert.ToString(part.PartNumber), timeSpan, "part");

                    PartsDefectiveVM partDefective = new PartsDefectiveVM
                    {
                        PartNumber = part.PartNumber,
                        PartsDefectiveAmount = sumQuantDefective,
                        PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : Math.Abs(periodChange.periodChange),
                        IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true
                    };

                    // Add to the list
                    if (sumQuantDefective > 0)
                    {
                        defectivePartsList.Add(partDefective);
                    }
                }

                //Before we sort, see if we have called for a change of filtering or sorting
                if (!string.IsNullOrEmpty(actionButton)) //Form Submitted!
                {

                    //page = 1;//Reset Page 

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
                if (sortField == "TOTAL")
                {
                    if (sortDirection == "asc")
                    {
                        defectivePartsList = defectivePartsList.OrderBy(d => d.PartsDefectiveAmount).ToList();
                    }
                    else
                    {
                        defectivePartsList = defectivePartsList.OrderByDescending(d => d.PartsDefectiveAmount).ToList();
                    }
                }

                ViewData["sortDirection"] = sortDirection;

                // Pagination
                int pageSize = 5; // Number of items per page
                int recordsInTheList = defectivePartsList.Count();
                int numbOfPages = (int)Math.Ceiling((double)recordsInTheList / pageSize);

                var pagedData = defectivePartsList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var response = new ComposedAnalyticsListsVM
                {
                    PagedData = pagedData,
                    NumbOfPages = numbOfPages,
                    PageNum = page
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public IActionResult PartsDefective(string period = null, string partSelected = null)
        {
            PartsDefectiveVM partsDefectiveVM = new PartsDefectiveVM();
            try
            {

                //Format partSelected
                int index = partSelected.IndexOf(' ');
                string partNumber = partSelected.Substring(0, index);

                //Method to get time span
                var timeSpan = GetTimeSpan(period);
                //Format date
                string formattedStartDate = timeSpan[0].ToString("MMM d, yyyy"); // Format start date
                string formattedEndDate = timeSpan[1].ToString("MMM d, yyyy"); // Format end date

                // Put in a list all the Qual Rep forms associated with the Part
                var qualRepsLinkedToPart = GetQualRepsByPart(partNumber, timeSpan, false);

                //Is data enough?
                partsDefectiveVM.EnoughData = qualRepsLinkedToPart.Count() <= 1 ? false : true;

                (double periodChange, int total, int previousTotal) periodChange = PoPChange(partNumber, timeSpan, "part");

                //Assign values to ViewModel
                partsDefectiveVM.StartDate = formattedStartDate;
                partsDefectiveVM.EndDate = formattedEndDate;
                partsDefectiveVM.PartsDefectiveAmount = periodChange.total;
                partsDefectiveVM.PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : Math.Abs(periodChange.periodChange);
                partsDefectiveVM.IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true;

                return Ok(partsDefectiveVM);
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public List<object> GenChart(string period, string partSelected)
        {
            try
            {
                if (partSelected != null)
                {
                    //Format partSelected
                    int index = partSelected.IndexOf(' ');
                    string partNumber = partSelected.Substring(0, index);

                    //Method to get time span
                    var timeSpan = GetTimeSpan(period);

                    // Put in a list all the Qual Rep forms associated with the Part
                    var qualRepsLinkedToPart = GetQualRepsByPart(partNumber, timeSpan, false);

                    if (qualRepsLinkedToPart.Count() == 0)
                    {
                        return null;
                    }

                    List<object> data = new List<object>();
                    List<int> total = new List<int>();
                    List<string> recordDates = new List<string>();
                    List<string> chartLabels = new List<string>();

                    // Iterate through each day within the time span
                    for (DateOnly date = timeSpan[0]; date <= timeSpan[1];)
                    {
                        // Add the string representation of the date to the list
                        if (period == "Monthly")
                        {
                            chartLabels.Add(date.ToString("MM-dd"));
                            date = date.AddDays(1);
                        }
                        //else if (period == "3 Years")
                        //{
                        //    // Calculate the quarter
                        //    int quarter = (date.Month - 1) / 3 + 1;
                        //    string quarterLabel = $"{date.Year}-{quarter}";
                        //    chartLabels.Add(quarterLabel);

                        //    date = date.AddMonths(1);
                        //}
                        else
                        {
                            chartLabels.Add(date.ToString("yyyy-MM"));
                            date = date.AddMonths(1);
                        }
                    }


                    //Loop trhough the qualRep's and add to the lists the total and months
                    foreach (var qualRep in qualRepsLinkedToPart)
                    {
                        total.Add(qualRep.QuantDefective);

                        //Match the index from the chartLabels with the QualityRepDate's
                        if (period == "Monthly")
                        {
                            //var chartLabel = chartLabels.Where(cl => DateOnly.Parse(cl) == qualRep.QualityRepDate
                            //                                      || DateOnly.Parse(cl).AddDays(1) == qualRep.QualityRepDate
                            //                                      || DateOnly.Parse(cl).AddDays(2) == qualRep.QualityRepDate).FirstOrDefault();

                            var chartIndex = chartLabels.IndexOf(qualRep.QualityRepDate.ToString("MM-dd"));
                            recordDates.Add(chartIndex.ToString());
                        }
                        //else if(period == "3 Years")
                        //{
                        //    var chartLabel = chartLabels.FirstOrDefault(cl => DateOnly.Parse(cl).ToString("yyyy-MM") == qualRep.QualityRepDate.ToString("yyyy-MM"));
                        //    var chartIndex = chartLabels.IndexOf(chartLabel);
                        //    recordDates.Add(chartIndex.ToString());
                        //}
                        else
                        {
                            var chartLabel = chartLabels.FirstOrDefault(cl => DateOnly.Parse(cl).ToString("yyyy-MM") == qualRep.QualityRepDate.ToString("yyyy-MM"));
                            var chartIndex = chartLabels.IndexOf(chartLabel);
                            recordDates.Add(chartIndex.ToString());
                        }
                    }

                    data.Add(chartLabels);
                    data.Add(total);
                    data.Add(recordDates);

                    return data;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        public (double periodChange, int total, int previousTotal) PoPChange(string objNumber, List<DateOnly> timeSpan, string objectName)
        {
            List<QualityRepresentative> qualRepsLinked = new List<QualityRepresentative>();
            List<QualityRepresentative> previousQualRepsLinked = new List<QualityRepresentative>();

            if (objectName == "part")
            {
                // Put in a list all the Qual Rep forms associated with the Part
                qualRepsLinked = GetQualRepsByPart(objNumber, timeSpan, false);
                // Put in a list all the Qual Rep forms associated with the from the previous timeSpan
                previousQualRepsLinked = GetQualRepsByPart(objNumber, timeSpan, true);
            }
            else
            {
                // Put in a list all the Qual Rep forms associated with the Supplier
                qualRepsLinked = GetQualRepsBySupplier(objNumber, timeSpan, false);
                // Put in a list all the Qual Rep forms associated with the previous timeSpan
                previousQualRepsLinked = GetQualRepsBySupplier(objNumber, timeSpan, true);
            }

            int total = 0;
            int previousTotal = 0;

            //Loop trhough the qualRep's and add to the lists the total and months
            foreach (var QualRep in qualRepsLinked)
            {
                total += QualRep.QuantDefective;
            }

            foreach (var QualRep in previousQualRepsLinked)
            {
                previousTotal += QualRep.QuantDefective;
            }

            //Calculate the percentage change compared to last month and decide if it is positive or negative
            double prevTMinusTotal = previousTotal - total;
            double periodChange = Math.Round(((prevTMinusTotal / previousTotal) * 100), 2);

            return (periodChange, total, previousTotal);
        }

        public List<QualityRepresentative> GetQualRepsByPart(string partNumber, List<DateOnly> timeSpan, bool previousTimeSpan)
        {
            //Get part
            var partUsed = _context.Parts.FirstOrDefault(p => p.PartNumber == Convert.ToInt32(partNumber));
            if (partUsed == null)
            {
                return null;
            }

            List<QualityRepresentative> qualRepsLinkedToPart;

            //Get QualityRepresentative's associated to this part
            if (previousTimeSpan)
            {
                qualRepsLinkedToPart = _context.QualityRepresentatives.Where(p => p.PartID == partUsed.ID)
                                                            .Where(p => p.QualityRepDate >= timeSpan[2])
                                                            .Where(p => p.QualityRepDate <= timeSpan[0])
                                                            .ToList();
            }
            else
            {
                qualRepsLinkedToPart = _context.QualityRepresentatives.Where(p => p.PartID == partUsed.ID)
                                                                            .Where(p => p.QualityRepDate >= timeSpan[0])
                                                                            .Where(p => p.QualityRepDate <= timeSpan[1])
                                                                            .ToList();
            }

            return qualRepsLinkedToPart;
        }
        public List<QualityRepresentative> GetQualRepsBySupplier(string supplierNumber, List<DateOnly> timeSpan, bool previousTimeSpan)
        {
            //Get part
            var supplierUsed = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supplierNumber));
            if (supplierUsed == null)
            {
                return null;
            }

            List<QualityRepresentative> qualRepsLinkedToSupplier;

            //Get QualityRepresentative's associated to this part
            if (previousTimeSpan)
            {
                qualRepsLinkedToSupplier = _context.QualityRepresentatives.Where(p => p.SupplierID == supplierUsed.ID)
                                                            .Where(p => p.QualityRepDate >= timeSpan[2])
                                                            .Where(p => p.QualityRepDate <= timeSpan[0])
                                                            .ToList();
            }
            else
            {
                qualRepsLinkedToSupplier = _context.QualityRepresentatives.Where(p => p.SupplierID == supplierUsed.ID)
                                                                            .Where(p => p.QualityRepDate >= timeSpan[0])
                                                                            .Where(p => p.QualityRepDate <= timeSpan[1])
                                                                            .ToList();
            }
            return qualRepsLinkedToSupplier;
        }

        public List<DateOnly> GetTimeSpan(string period)
        {

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            //Initialize time variables
            DateOnly startDate = DateOnly.FromDateTime(now);
            DateOnly endDate = DateOnly.FromDateTime(now);
            DateOnly previousDate = DateOnly.FromDateTime(now);

            //Define period chosen
            if (period == "Monthly")
            {
                startDate = DateOnly.FromDateTime(now.AddMonths(-1));
                previousDate = DateOnly.FromDateTime(now.AddMonths(-2));
            }
            else if (period == "6 Months")
            {
                startDate = DateOnly.FromDateTime(now.AddMonths(-6));
                previousDate = DateOnly.FromDateTime(now.AddMonths(-12));
            }
            else if (period == "Yearly")
            {
                startDate = DateOnly.FromDateTime(now.AddMonths(-12));
                previousDate = DateOnly.FromDateTime(now.AddMonths(-24));
            }
            else if (period == "3 Years")
            {
                startDate = DateOnly.FromDateTime(now.AddMonths(-36));
                previousDate = DateOnly.FromDateTime(now.AddMonths(-72));
            }

            List<DateOnly> dates = new List<DateOnly> { startDate, endDate, previousDate };
            return dates;
        }

        // GET: NCR/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.NCRs == null)
            {
                return NotFound();
            }

            var nCR = await _context.NCRs
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.EngReview)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Operations)
                    .Include(p => p.Operations.QualityPhotos)
                    .Include(p => p.Operations.PrelDecision)
                    .Include(p => p.Operations.VideoLinks)
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.ProcessApplicable)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                    .Include(qr => qr.QualityRepresentative.Part)
                .Include(n => n.Procurement)
                    .Include(pr => pr.Procurement.QualityPhotos)
                    .Include(pr => pr.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(r => r.Reinspection.QualityPhotos)
                    .Include(r => r.Reinspection.VideoLinks)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (nCR == null)
            {
                return NotFound();
            }

            return View(nCR);
        }

        // GET: NCR/CreateQualityRepresentative
        [Authorize(Roles = "Admin,Quality Inspector")]
        public IActionResult CreateQualityRepresentative(int? id, Byte[] rowVersion)
        {
            string[] selectedOptions = Array.Empty<string>();

            NCRNumber displayNCRNumber = new NCRNumber();

            PopulateUserEmailData(selectedOptions);

            if (id != null)
            {

                NCR quality = GetNCR((int)id);
                if (quality == null)
                {
                    TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                    return RedirectToAction(nameof(Index));
                }
                if (quality.Phase != "Quality Representative")
                {
                    TempData["ErrorAlert"] = "Unable to complete task. The NCR was already filled.";
                    return RedirectToAction(nameof(Index));
                }
                if (quality.IsNCRArchived == true)
                {
                    TempData["ErrorAlert"] = "Unable to complete task. The NCR was Archived.";
                    return RedirectToAction(nameof(Index));
                }
                if (quality.Status != "Active")
                {
                    TempData["ErrorAlert"] = "Unable to complete task. The is not active anymore.";
                    return RedirectToAction(nameof(Index));
                }

                //Check for concurrency error
                if (rowVersion != null)
                {
                    if (!quality.RowVersion.SequenceEqual(rowVersion))
                    {
                        TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                        return RedirectToAction("Index");
                    }
                }

                if (quality.IsNCRDraft)
                {
                    quality.QualityRepresentative = new QualityRepresentative();
                    quality.QualityRepresentative.PoNo = (int)quality.DraftQualityRepresentative.PoNo;
                    quality.QualityRepresentative.SalesOrd = quality.DraftQualityRepresentative.SalesOrd;
                    quality.QualityRepresentative.QuantReceived = (int)(quality.DraftQualityRepresentative.QuantReceived);
                    quality.QualityRepresentative.QuantDefective = (int)(quality.DraftQualityRepresentative.QuantDefective);
                    quality.QualityRepresentative.DescDefect = quality.DraftQualityRepresentative.DescDefect;
                    quality.QualityRepresentative.NonConforming = (bool)(quality.DraftQualityRepresentative.NonConforming);
                    quality.QualityRepresentative.ConfirmingEng = (bool)(quality.DraftQualityRepresentative.ConfirmingEng);
                    quality.QualityRepresentative.QualityRepDate = (DateOnly)quality.DraftQualityRepresentative.QualityRepDate;
                    quality.QualityRepresentative.QualityRepresentativeSign = quality.DraftQualityRepresentative.QualityRepresentativeSign;
                    quality.QualityRepresentative.ProblemID = (int)quality.DraftQualityRepresentative.ProblemID;
                    quality.QualityRepresentative.PartID = (int)quality.DraftQualityRepresentative.PartID;
                    quality.QualityRepresentative.SupplierID = (int)quality.DraftQualityRepresentative.SupplierID;
                    quality.QualityRepresentative.ProcessApplicableID = (int)quality.DraftQualityRepresentative.ProcessApplicableID;
                    quality.QualityRepresentative.QualityPhotos = quality.DraftQualityRepresentative.QualityPhotos;
                    quality.QualityRepresentative.VideoLinks = quality.DraftQualityRepresentative.VideoLinks;

                    //Get part, suppplier and problem
                    var getPart = _context.Parts.FirstOrDefault(p => p.ID == quality.QualityRepresentative.PartID);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.ID == quality.QualityRepresentative.SupplierID);
                    var getProblem = _context.Problems.FirstOrDefault(p => p.ID == quality.QualityRepresentative.ProblemID);
                    if (getPart != null)
                    {
                        quality.QualityRepresentative.Part = getPart;
                    }
                    if (getSup != null)
                    {
                        quality.QualityRepresentative.Supplier = getSup;
                    }
                    if (getProblem != null)
                    {
                        quality.QualityRepresentative.Problem = getProblem;
                    }
                }
                else if(quality.PrevNCRID != null)
                {
                    NCR prevNCR = GetNCR((int)quality.PrevNCRID);

                    quality.QualityRepresentative = new QualityRepresentative();

                    quality.QualityRepresentative = new QualityRepresentative
                    {
                        PoNo = prevNCR.QualityRepresentative.PoNo,
                        SalesOrd = prevNCR.QualityRepresentative.SalesOrd,
                        QuantReceived = prevNCR.QualityRepresentative.QuantReceived,
                        QuantDefective = prevNCR.QualityRepresentative.QuantDefective,
                        DescDefect = prevNCR.QualityRepresentative.DescDefect,
                        ProblemID = prevNCR.QualityRepresentative.ProblemID,
                        PartID = prevNCR.QualityRepresentative.PartID,
                        SupplierID = prevNCR.QualityRepresentative.SupplierID,
                        ProcessApplicableID = prevNCR.QualityRepresentative.ProcessApplicableID,
                    };

                    TempData["InfoAlertCreate"] = $"For your convenience, some of the data from the previous NCR was passed to this one, you can change it if necessary.";
                }

                if (quality.PrevNCRID == null)
                {
                    NewNCRNumber(quality, displayNCRNumber);

                }
                PopulateList();

                return View("Create", quality);
            }

            NCR genNCR = new NCR();

            PopulateList();

            NewNCRNumber(genNCR, displayNCRNumber);

            return View("Create", genNCR);

        }

        // POST: NCR/CreateQualityRepresentative
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Quality Inspector")]
        public async Task<IActionResult> CreateQualityRepresentative(NCR quality, NCR genNCR, int[] imagesToRemove, int[] linksToRemove, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, string sectionEdited, Byte[] RowVersion, string problemSel, string partSel, string supSel)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }
            //get partSelected
            if (partSel != null)
            {
                try
                {
                    int indexPart = partSel.IndexOf(' ');
                    string partNumber = partSel.Substring(0, indexPart);
                    var getPart = _context.Parts.FirstOrDefault(p => p.PartNumber == Convert.ToInt32(partNumber));
                    if(getPart != null)
                    {
                        quality.QualityRepresentative.PartID = getPart.ID;
                        genNCR.QualityRepresentative.PartID = getPart.ID;
                    }
                }
                catch (Exception)
                {

                }
            }
            //get supplierSelected
            if(supSel != null)
            {
                try
                {
                    int indexSup = supSel.IndexOf(' ');
                    string supCode = supSel.Substring(0, indexSup);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supCode));
                    if (getSup != null)
                    {
                        quality.QualityRepresentative.SupplierID = getSup.ID;
                        genNCR.QualityRepresentative.SupplierID = getSup.ID;
                    }
                }
                catch (Exception)
                {

                }
            }
            //get problem Selected
            if (problemSel != null)
            {
                try
                {
                    var getProblem = _context.Problems.FirstOrDefault(p => p.ProblemDescription == problemSel);
                    if(getProblem != null)
                    {
                        quality.QualityRepresentative.ProblemID = getProblem.ID;
                        genNCR.QualityRepresentative.ProblemID = getProblem.ID;
                    }
                }
                catch(Exception)
                {

                }
            }

            try
            {
                if (ModelState.IsValid)
                {
                    var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;
                    var usersInEngineeringRole = _userManager.GetUsersInRoleAsync("Engineer").Result;
                    //Initialize Notification sender
                    NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
                    List<NCR> ncrToNotify = new List<NCR>();
                    //Initialize NCR generator
                    NCRNumber createNCRNumber = new NCRNumber();

                    string emailSubject = $"NCR {genNCR.NCRNum} - {Subject}";
                    TempData["SuccessAlert"] = $"A new NCR of number {genNCR.NCRNum} was started.";

                    _context.Entry(genNCR).Property("RowVersion").OriginalValue = RowVersion;
                    _context.Entry(quality).Property("RowVersion").OriginalValue = RowVersion;

                    //Check for concurrency errors
                    if (RowVersion != null)
                    {
                        var refreshedModel = GetNCR(genNCR.ID);
                        if (refreshedModel == null)
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                        else if (!refreshedModel.RowVersion.SequenceEqual(RowVersion))
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                    }
                    NewNCRNumber(genNCR, createNCRNumber);

                    PopulateUserEmailData(selectedOptions);

                    //Save draft for the first time
                    if (draft != null && quality.IsNCRDraft == false)
                    {
                        NCR ncrToUpdate = GetNCR(quality.ID);
                        if(ncrToUpdate != null)
                        {
                            _context.Entry(ncrToUpdate).Property("RowVersion").OriginalValue = RowVersion;
                        }
                        //Create draft model and pass input data to it
                        DraftQualityRepresentative draftQual = new DraftQualityRepresentative
                        {
                            PoNo = genNCR.QualityRepresentative.PoNo,
                            SalesOrd = genNCR.QualityRepresentative.SalesOrd,
                            QuantReceived = genNCR.QualityRepresentative.QuantReceived,
                            QuantDefective = genNCR.QualityRepresentative.QuantDefective,
                            DescDefect = genNCR.QualityRepresentative.DescDefect,
                            NonConforming = genNCR.QualityRepresentative.NonConforming,
                            ConfirmingEng = genNCR.QualityRepresentative.ConfirmingEng,
                            QualityRepDate = genNCR.QualityRepresentative.QualityRepDate,
                            QualityRepresentativeSign = genNCR.QualityRepresentative.QualityRepresentativeSign,
                            ProblemID = genNCR.QualityRepresentative.ProblemID,
                            PartID = genNCR.QualityRepresentative.PartID,
                            SupplierID = genNCR.QualityRepresentative.SupplierID,
                            ProcessApplicableID = genNCR.QualityRepresentative.ProcessApplicableID,
                        };

                        if (ncrToUpdate != null && ncrToUpdate.PrevNCRID != null)
                        {
                            //Get ncr to update

                            ncrToUpdate.IsNCRDraft = true;
                            ncrToUpdate.DraftQualityRepresentative = draftQual;

                            //Add photos and pictures
                            await AddQualityPhotos(ncrToUpdate.DraftQualityRepresentative, Pictures);
                            await AddVideoLinks(ncrToUpdate.DraftQualityRepresentative, Links);

                            if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.DraftQualityRepresentative)) 
                            { 
                                _context.NCRs.Update(ncrToUpdate);
                            };
                        }
                        else
                        {
                            var nowTorontoOne = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                            //Initialize draft model
                            NCR ncrModelDraft = new NCR();

                            //Set up NCR model for draft
                            ncrModelDraft.IsNCRDraft = true;
                            ncrModelDraft.CreatedOnDO = DateOnly.FromDateTime(nowTorontoOne);
                            ncrModelDraft.CreatedOn = nowTorontoOne;
                            ncrModelDraft.Status = "Active";
                            ncrModelDraft.Phase = "Quality Representative";
                            ncrModelDraft.NCRNum = null;
                            ncrModelDraft.DraftQualityRepresentative = draftQual;

                            //Add photos and pictures
                            await AddQualityPhotos(ncrModelDraft.DraftQualityRepresentative, Pictures);
                            await AddVideoLinks(ncrModelDraft.DraftQualityRepresentative, Links);
                            _context.NCRs.Add(ncrModelDraft);
                        }

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && quality.IsNCRDraft == true)
                    {
                        //Get ncr to update
                        NCR ncrToUpdate = GetNCR(quality.ID);
                        if (ncrToUpdate != null)
                        {
                            _context.Entry(ncrToUpdate).Property("RowVersion").OriginalValue = RowVersion;
                        }
                        //Pass data to the draft properties
                        ncrToUpdate.DraftQualityRepresentative.PoNo = quality.QualityRepresentative.PoNo;
                        ncrToUpdate.DraftQualityRepresentative.SalesOrd = quality.QualityRepresentative.SalesOrd;
                        ncrToUpdate.DraftQualityRepresentative.QuantReceived = quality.QualityRepresentative.QuantReceived;
                        ncrToUpdate.DraftQualityRepresentative.QuantDefective = quality.QualityRepresentative.QuantDefective;
                        ncrToUpdate.DraftQualityRepresentative.DescDefect = quality.QualityRepresentative.DescDefect;
                        ncrToUpdate.DraftQualityRepresentative.NonConforming = quality.QualityRepresentative.NonConforming;
                        ncrToUpdate.DraftQualityRepresentative.ConfirmingEng = quality.QualityRepresentative.ConfirmingEng;
                        ncrToUpdate.DraftQualityRepresentative.QualityRepDate = quality.QualityRepresentative.QualityRepDate;
                        ncrToUpdate.DraftQualityRepresentative.QualityRepresentativeSign = quality.QualityRepresentative.QualityRepresentativeSign;
                        ncrToUpdate.DraftQualityRepresentative.ProblemID = quality.QualityRepresentative.ProblemID;
                        ncrToUpdate.DraftQualityRepresentative.PartID = quality.QualityRepresentative.PartID;
                        ncrToUpdate.DraftQualityRepresentative.SupplierID = quality.QualityRepresentative.SupplierID;
                        ncrToUpdate.DraftQualityRepresentative.ProcessApplicableID = quality.QualityRepresentative.ProcessApplicableID;

                        //Update the draft model
                        if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.DraftQualityRepresentative))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrToUpdate.DraftQualityRepresentative, Pictures);
                            await AddVideoLinks(ncrToUpdate.DraftQualityRepresentative, Links);

                            _context.NCRs.Update(ncrToUpdate);
                        }

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && quality.IsNCRDraft == true)
                    {
                        //Get NCR to Update
                        NCR ncrToUpdate = GetNCR(quality.ID);
                        if (ncrToUpdate != null)
                        {
                            _context.Entry(ncrToUpdate).Property("RowVersion").OriginalValue = RowVersion;
                        }
                        //Create quality object inside ncrToUpdate
                        QualityRepresentative draftQual = new QualityRepresentative
                        {
                            PoNo = quality.QualityRepresentative.PoNo,
                            SalesOrd = quality.QualityRepresentative.SalesOrd,
                            QuantReceived = quality.QualityRepresentative.QuantReceived,
                            QuantDefective = quality.QualityRepresentative.QuantDefective,
                            DescDefect = quality.QualityRepresentative.DescDefect,
                            NonConforming = quality.QualityRepresentative.NonConforming,
                            ConfirmingEng = quality.QualityRepresentative.ConfirmingEng,
                            QualityRepDate = quality.QualityRepresentative.QualityRepDate,
                            QualityRepresentativeSign = quality.QualityRepresentative.QualityRepresentativeSign,
                            ProblemID = quality.QualityRepresentative.ProblemID,
                            PartID = quality.QualityRepresentative.PartID,
                            SupplierID = quality.QualityRepresentative.SupplierID,
                            ProcessApplicableID = quality.QualityRepresentative.ProcessApplicableID,
                            QualityPhotos = ncrToUpdate.DraftQualityRepresentative.QualityPhotos,
                            VideoLinks = ncrToUpdate.DraftQualityRepresentative.VideoLinks
                        };

                        //Set new quality representative model and pass NCR to next phase
                        ncrToUpdate.QualityRepresentative = draftQual;
                        ncrToUpdate.IsNCRDraft = false;
                        if (ncrToUpdate.QualityRepresentative.ConfirmingEng)
                        {
                            ncrToUpdate.Phase = "Operations";
                        }
                        else
                        {
                            ncrToUpdate.Phase = "Engineering";
                        }

                        if(ncrToUpdate.NCRNum == null)
                        {
                            var nowTorontoTwo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                            ncrToUpdate.CreatedOn = nowTorontoTwo;
                            ncrToUpdate.CreatedOnDO = DateOnly.FromDateTime(nowTorontoTwo);
                            ncrToUpdate.NCRNum = genNCR.NCRNum;
                            _context.NCRNumbers.Add(createNCRNumber);
                        }

                        //Update NCR
                        if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.QualityRepresentative))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrToUpdate.QualityRepresentative, Pictures);
                            await AddVideoLinks(ncrToUpdate.QualityRepresentative, Links);

                            _context.NCRs.Update(ncrToUpdate);
                        }
                        
                        await _context.SaveChangesAsync();

                        ncrToNotify.Add(ncrToUpdate);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInEngineeringRole, "create", null, emailContent);

                        //emailSubject = $"NCR {ncrToUpdate.NCRNum} - {Subject}";
                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                        TempData["SuccessAlert"] = $"A new NCR of number {ncrToUpdate.NCRNum} was started.";

                        return RedirectToAction("Index");
                    }

                    //If in Quality Representative Phase, fill NCR
                    if (quality.Phase == "Quality Representative")
                    {
                        NCR ncrModelFill = GetNCR(quality.ID);
                        if (ncrModelFill != null)
                        {
                            _context.Entry(ncrModelFill).Property("RowVersion").OriginalValue = RowVersion;
                        }

                        if (quality.QualityRepresentative.ConfirmingEng)
                        {
                            ncrModelFill.Phase = "Operations";
                        }
                        else
                        {
                            ncrModelFill.Phase = "Engineering";
                        }

                        ncrModelFill.QualityRepresentative = quality.QualityRepresentative;

                        await AddQualityPhotos(ncrModelFill.QualityRepresentative, Pictures);
                        await AddVideoLinks(ncrModelFill.QualityRepresentative, Links);

                        _context.NCRs.Update(ncrModelFill);
                        await _context.SaveChangesAsync();

                        //Send notification
                        ncrToNotify.Add(ncrModelFill);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInEngineeringRole, "create", null, emailContent);

                        emailSubject = $"NCR {ncrModelFill.NCRNum} - {Subject}";
                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                        TempData["SuccessAlert"] = $"A new NCR of number {ncrModelFill.NCRNum} was started.";

                        return RedirectToAction("Index");
                    }

                    var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                    //If engineering not required, skip it
                    NCR ncrModel = new NCR
                    {
                        PrevNCRID = genNCR.PrevNCRID,
                        Status = "Active",
                        Phase = "Engineering",
                        NCRNum = genNCR.NCRNum,
                        CreatedOnDO = DateOnly.FromDateTime(nowToronto),
                        CreatedOn = nowToronto,
                        QualityRepresentative = genNCR.QualityRepresentative
                    };
                    NCR ncrModelNoEng = new NCR
                    {
                        PrevNCRID = genNCR.PrevNCRID,
                        Status = "Active",
                        NCRNum = genNCR.NCRNum,
                        Phase = "Operations",
                        CreatedOnDO = DateOnly.FromDateTime(nowToronto),
                        CreatedOn = nowToronto,
                        QualityRepresentative = genNCR.QualityRepresentative
                    };

                    await AddQualityPhotos(genNCR.QualityRepresentative, Pictures);
                    await AddVideoLinks(genNCR.QualityRepresentative, Links);

                    //Decide if engineering should be included
                    if (genNCR.QualityRepresentative.ConfirmingEng == true)
                    {   
                        _context.NCRs.Add(ncrModelNoEng);
                        //Add ncr notification list
                        ncrToNotify.Add(ncrModelNoEng);
                        
                    }
                    else
                    {
                        _context.NCRs.Add(ncrModel);
                        //Add ncr notification list
                        ncrToNotify.Add(ncrModel);
                    }
                    _context.NCRNumbers.Add(createNCRNumber);
                    
                    await _context.SaveChangesAsync();

                    //Send notification
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInEngineeringRole, "create", null, emailContent);

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                    
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try to submit again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                NCR ncrError = GetNCR(genNCR.ID);
                if (ncrError == null)
                {
                    TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Entry(ncrError).Reload();

                if (!NCRExists(ncrError.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrError.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrError.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try to submit again, and if the problem persists, see your system administrator.");
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency when reloading the page
            PopulateUserEmailData(selectedOptions);
            PopulateList();


            NCR ncrModelRepeat = GetNCR(quality.ID);
            if (ncrModelRepeat != null)
            {
                _context.Entry(ncrModelRepeat).Reload();

                if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
                {
                    if (!NCRExists(ncrModelRepeat.ID))
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                    }
                    else if (ncrModelRepeat.IsNCRArchived)
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                    }
                    else if (ncrModelRepeat.Status == "Voided")
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                    }
                    else
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                    }
                    return RedirectToAction(nameof(Index));
                }
                ncrModelRepeat.QualityRepresentative = quality.QualityRepresentative;
                ncrModelRepeat.QualityRepresentative = genNCR.QualityRepresentative;
                TempData["SectionEdited"] = sectionEdited;

                //Get part, suppplier and problem
                var getPart = _context.Parts.FirstOrDefault(p => p.ID == quality.QualityRepresentative.PartID);
                var getSup = _context.Suppliers.FirstOrDefault(p => p.ID == quality.QualityRepresentative.SupplierID);
                var getProblem = _context.Problems.FirstOrDefault(p => p.ID == quality.QualityRepresentative.ProblemID);
                if (getPart != null)
                {
                    ncrModelRepeat.QualityRepresentative.Part = getPart;
                }
                if (getSup != null)
                {
                    ncrModelRepeat.QualityRepresentative.Supplier = getSup;
                }
                if (getProblem != null)
                {
                    ncrModelRepeat.QualityRepresentative.Problem = getProblem;
                }

                //If NCR is a draft
                if (ncrModelRepeat.IsNCRDraft)
                {
                    ncrModelRepeat.QualityRepresentative.QualityPhotos = ncrModelRepeat.DraftQualityRepresentative.QualityPhotos;
                    ncrModelRepeat.QualityRepresentative.VideoLinks = ncrModelRepeat.DraftQualityRepresentative.VideoLinks;
                    return View("Create", ncrModelRepeat);
                }
            }
            //Optmize repetition
            //Get part, suppplier and problem
            var getPartTwo = _context.Parts.FirstOrDefault(p => p.ID == quality.QualityRepresentative.PartID);
            var getSupTwo = _context.Suppliers.FirstOrDefault(p => p.ID == quality.QualityRepresentative.SupplierID);
            var getProblemTwo = _context.Problems.FirstOrDefault(p => p.ID == quality.QualityRepresentative.ProblemID);
            if (getPartTwo != null)
            {
                genNCR.QualityRepresentative.Part = getPartTwo;
            }
            if (getSupTwo != null)
            {
                genNCR.QualityRepresentative.Supplier = getSupTwo;
            }
            if (getProblemTwo != null)
            {
                genNCR.QualityRepresentative.Problem = getProblemTwo;
            }

            return View("Create", genNCR);
        }

        // GET: NCR/CreateEngineering
        [Authorize(Roles = "Admin,Engineer")]
        public IActionResult CreateEngineering(int id, Byte[] rowVersion)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR eng = GetNCR(id);
            if (eng == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            if (eng.Phase != "Engineering")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was already filled.";
                return RedirectToAction(nameof(Index));
            }
            if (eng.IsNCRArchived == true)
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was Archived.";
                return RedirectToAction(nameof(Index));
            }
            if (eng.Status != "Active")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The is not active anymore.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            if(rowVersion != null)
            {
                if (!eng.RowVersion.SequenceEqual(rowVersion))
                {
                    TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                    return RedirectToAction("Index");
                }
            }

            if (eng.IsNCRDraft == true)
            {
                eng.Engineering = new Engineering();
                eng.Engineering.IsCustNotificationNecessary = eng.DraftEngineering.IsCustNotificationNecessary;
                eng.Engineering.CustIssueMsg = eng.DraftEngineering.CustIssueMsg;
                eng.Engineering.Disposition = eng.DraftEngineering.Disposition;
                eng.Engineering.DrawReqUpdating = eng.DraftEngineering.DrawReqUpdating;
                eng.Engineering.OrgRevisionNum = eng.DraftEngineering.OrgRevisionNum;
                eng.Engineering.RevisionedBy = eng.DraftEngineering.RevisionedBy;
                eng.Engineering.UpdatedRevisionNum = eng.DraftEngineering.UpdatedRevisionNum;
                eng.Engineering.RevisionDate = eng.DraftEngineering.RevisionDate;
                eng.Engineering.EngineerSign = eng.DraftEngineering.EngineerSign;
                eng.Engineering.EngineeringDate = (DateOnly)eng.DraftEngineering.EngineeringDate;
                eng.Engineering.EngReviewID = (int)eng.DraftEngineering.EngReviewID;
                eng.Engineering.QualityPhotos = eng.DraftEngineering.QualityPhotos;
                eng.Engineering.VideoLinks = eng.DraftEngineering.VideoLinks;
            }

            PopulateList();

            return View("Create", eng);
        }

        // POST: NCR/CreateEngineering
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineer")]
        public async Task<IActionResult> CreateEngineering(NCR eng, List<IFormFile> Pictures, NCRNumber createNCRNumber, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove, string sectionEdited, Byte[] RowVersion)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    PopulateUserEmailData(selectedOptions);
                    NCR ncrModel = GetNCR(eng.ID);

                    //Intialize Notification sender
                    NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
                    List<NCR> ncrToNotify = new List<NCR>();
                    var usersInOperationsRole = _userManager.GetUsersInRoleAsync("Operations Manager").Result;

                    string emailSubject = $"NCR {ncrModel.NCRNum} - {Subject}";
                    TempData["SuccessAlert"] = $"NCR {ncrModel.NCRNum}. 'Engineering' section was successfully filled.";

                    if(ncrModel != null)
                    {
                        _context.Entry(ncrModel).Property("RowVersion").OriginalValue = RowVersion;
                    }
                    _context.Entry(eng).Property("RowVersion").OriginalValue = RowVersion;

                    //Check for concurrency errors
                    if (RowVersion != null)
                    {
                        var refreshedModel = GetNCR(eng.ID);
                        if (refreshedModel == null)
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                        else if (!refreshedModel.RowVersion.SequenceEqual(RowVersion))
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                    }

                    //Save draft for the first time
                    if (draft != null && eng.IsNCRDraft == false)
                    {
                        //Create draft model and pass input data to it
                        DraftEngineering draftEng = new DraftEngineering
                        {
                            IsCustNotificationNecessary = eng.Engineering.IsCustNotificationNecessary,
                            CustIssueMsg = eng.Engineering.CustIssueMsg,
                            Disposition = eng.Engineering.Disposition,
                            DrawReqUpdating = eng.Engineering.DrawReqUpdating,
                            OrgRevisionNum = eng.Engineering.OrgRevisionNum,
                            RevisionedBy = eng.Engineering.RevisionedBy,
                            UpdatedRevisionNum = eng.Engineering.UpdatedRevisionNum,
                            RevisionDate = eng.Engineering.RevisionDate,
                            EngineerSign = eng.Engineering.EngineerSign,
                            EngineeringDate = eng.Engineering.EngineeringDate,
                            EngReviewID = eng.Engineering.EngReviewID,
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftEngineering = draftEng;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftEngineering, Pictures);
                        await AddVideoLinks(ncrModel.DraftEngineering, Links);

                        _context.NCRs.Update(ncrModel);
                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && eng.IsNCRDraft == true)
                    {
                        //Pass data to the draft properties
                        ncrModel.DraftEngineering.IsCustNotificationNecessary = eng.Engineering.IsCustNotificationNecessary;
                        ncrModel.DraftEngineering.CustIssueMsg = eng.Engineering.CustIssueMsg;
                        ncrModel.DraftEngineering.Disposition = eng.Engineering.Disposition;
                        ncrModel.DraftEngineering.DrawReqUpdating = eng.Engineering.DrawReqUpdating;
                        ncrModel.DraftEngineering.OrgRevisionNum = eng.Engineering.OrgRevisionNum;
                        ncrModel.DraftEngineering.RevisionedBy = eng.Engineering.RevisionedBy;
                        ncrModel.DraftEngineering.UpdatedRevisionNum = eng.Engineering.UpdatedRevisionNum;
                        ncrModel.DraftEngineering.RevisionDate = eng.Engineering.RevisionDate;
                        ncrModel.DraftEngineering.EngineerSign = eng.Engineering.EngineerSign;
                        ncrModel.DraftEngineering.EngineeringDate = eng.Engineering.EngineeringDate;
                        ncrModel.DraftEngineering.EngReviewID = eng.Engineering.EngReviewID;

                        //Update the draft model
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.DraftEngineering))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.DraftEngineering, Pictures);
                            await AddVideoLinks(ncrModel.DraftEngineering, Links);

                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && eng.IsNCRDraft == true)
                    {

                        //Create quality object inside ncrToUpdate
                        Engineering draftEng = new Engineering
                        {
                            IsCustNotificationNecessary = eng.Engineering.IsCustNotificationNecessary,
                            CustIssueMsg = eng.Engineering.CustIssueMsg,
                            Disposition = eng.Engineering.Disposition,
                            DrawReqUpdating = eng.Engineering.DrawReqUpdating,
                            OrgRevisionNum = eng.Engineering.OrgRevisionNum,
                            RevisionedBy = eng.Engineering.RevisionedBy,
                            UpdatedRevisionNum = eng.Engineering.UpdatedRevisionNum,
                            RevisionDate = eng.Engineering.RevisionDate,
                            EngineerSign = eng.Engineering.EngineerSign,
                            EngineeringDate = eng.Engineering.EngineeringDate,
                            EngReviewID = eng.Engineering.EngReviewID,
                            QualityPhotos = ncrModel.DraftEngineering.QualityPhotos,
                            VideoLinks = ncrModel.DraftEngineering.VideoLinks
                        };

                        //Set new quality representative model and pass NCR to next phase
                        ncrModel.Engineering = draftEng;
                        ncrModel.IsNCRDraft = false;
                        ncrModel.Phase = "Operations";

                        //Update NCR
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.Engineering))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.Engineering, Pictures);
                            await AddVideoLinks(ncrModel.Engineering, Links);

                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        //Send Notification
                        ncrToNotify.Add(ncrModel);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInOperationsRole, "fill", null, emailContent);

                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Operations";
                    ncrModel.Engineering = eng.Engineering;

                    await AddQualityPhotos(ncrModel.Engineering, Pictures);
                    await AddVideoLinks(ncrModel.Engineering, Links);

                    _context.NCRs.Update(ncrModel);
                    await _context.SaveChangesAsync();

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                    //Send Notification
                    ncrToNotify.Add(ncrModel);
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInOperationsRole, "fill", null, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                NCR ncrError = GetNCR(eng.ID);
                _context.Entry(ncrError).Reload();

                if (!NCRExists(ncrError.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrError.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrError.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            
            //Check for concurrency when reloading the page
            NCR ncrModelRepeat = GetNCR(eng.ID);
            if (ncrModelRepeat == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            _context.Entry(ncrModelRepeat).Reload();

            if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
            {
                if (!NCRExists(ncrModelRepeat.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrModelRepeat.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrModelRepeat.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            
            ncrModelRepeat.Engineering = eng.Engineering;
            TempData["SectionEdited"] = sectionEdited;

            //If NCR is a draft
            if (ncrModelRepeat.IsNCRDraft)
            {
                ncrModelRepeat.Engineering.QualityPhotos = ncrModelRepeat.DraftEngineering.QualityPhotos;
                ncrModelRepeat.Engineering.VideoLinks = ncrModelRepeat.DraftEngineering.VideoLinks;
            }

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", ncrModelRepeat);
        }

        // GET: NCR/CreateOperations
        [Authorize(Roles = "Admin,Operations Manager")]
        public IActionResult CreateOperations(int id, Byte[] rowVersion)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR oper = GetNCR(id);
            if (oper == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            if (oper.Phase != "Operations")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was already filled.";
                return RedirectToAction(nameof(Index));
            }
            if (oper.IsNCRArchived == true)
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was Archived.";
                return RedirectToAction(nameof(Index));
            }
            if (oper.Status != "Active")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The is not active anymore.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            if (rowVersion != null)
            {
                if (!oper.RowVersion.SequenceEqual(rowVersion))
                {
                    TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                    return RedirectToAction("Index");
                }
            }

            if (oper.IsNCRDraft == true)
            {
                oper.Operations = new Operations();
                oper.Operations.CarRaised = oper.DraftOperations.CarRaised;
                oper.Operations.CarNum = oper.DraftOperations.CarNum;
                oper.Operations.IsFollowUpReq = oper.DraftOperations.IsFollowUpReq;
                oper.Operations.FollowUpType = oper.DraftOperations.FollowUpType;
                oper.Operations.ExpecDate = oper.DraftOperations.ExpecDate;
                oper.Operations.OperationsDate = (DateOnly)oper.DraftOperations.OperationsDate;
                oper.Operations.OpManagerSign = oper.DraftOperations.OpManagerSign;
                oper.Operations.Message = oper.DraftOperations.Message;
                oper.Operations.PrelDecisionID = (int)oper.DraftOperations.PrelDecisionID;
                oper.Operations.QualityPhotos = oper.DraftOperations.QualityPhotos;
                oper.Operations.VideoLinks = oper.DraftOperations.VideoLinks;
            }

            PopulateList();

            return View("Create", oper);
        }

        // POST: NCR/CreateOperations
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Operations Manager")]
        public async Task<IActionResult> CreateOperations(NCR oper, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove, string sectionEdited, Byte[] RowVersion)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }

            try
            {
                if (ModelState.IsValid)
                {

                    PopulateUserEmailData(selectedOptions);
                    NCR ncrModel = GetNCR(oper.ID);


                    //Intialize Notification sender
                    var usersInProcurementRole = _userManager.GetUsersInRoleAsync("Procurement").Result;
                    NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
                    List<NCR> ncrToNotify = new List<NCR>();

                    string emailSubject = $"NCR {ncrModel.NCRNum} - {Subject}";
                    TempData["SuccessAlert"] = $"NCR {ncrModel.NCRNum}. 'Operations' section was successfully filled.";

                    if (ncrModel != null)
                    {
                        _context.Entry(ncrModel).Property("RowVersion").OriginalValue = RowVersion;
                    }
                    _context.Entry(oper).Property("RowVersion").OriginalValue = RowVersion;

                    //Check for concurrency errors
                    if (RowVersion != null)
                    {
                        var refreshedModel = GetNCR(oper.ID);
                        if (refreshedModel == null)
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                        else if (!refreshedModel.RowVersion.SequenceEqual(RowVersion))
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                    }

                    //Save draft for the first time
                    if (draft != null && oper.IsNCRDraft == false)
                    {
                        //Create draft model and pass input data to it
                        DraftOperations draftOpr = new DraftOperations
                        {
                            CarRaised = oper.Operations.CarRaised,
                            CarNum = oper.Operations.CarNum,
                            IsFollowUpReq = oper.Operations.IsFollowUpReq,
                            FollowUpType = oper.Operations.FollowUpType,
                            ExpecDate = oper.Operations.ExpecDate,
                            OperationsDate = oper.Operations.OperationsDate,
                            OpManagerSign = oper.Operations.OpManagerSign,
                            Message = oper.Operations.Message,
                            PrelDecisionID = oper.Operations.PrelDecisionID,
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftOperations = draftOpr;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftOperations, Pictures);
                        await AddVideoLinks(ncrModel.DraftOperations, Links);

                        _context.NCRs.Update(ncrModel);
                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && oper.IsNCRDraft == true)
                    {
                        //Pass data to the draft properties
                        ncrModel.DraftOperations.CarRaised = oper.Operations.CarRaised;
                        ncrModel.DraftOperations.CarNum = oper.Operations.CarNum;
                        ncrModel.DraftOperations.IsFollowUpReq = oper.Operations.IsFollowUpReq;
                        ncrModel.DraftOperations.FollowUpType = oper.Operations.FollowUpType;
                        ncrModel.DraftOperations.ExpecDate = oper.Operations.ExpecDate;
                        ncrModel.DraftOperations.OperationsDate = oper.Operations.OperationsDate;
                        ncrModel.DraftOperations.OpManagerSign = oper.Operations.OpManagerSign;
                        ncrModel.DraftOperations.Message = oper.Operations.Message;
                        ncrModel.DraftOperations.PrelDecisionID = oper.Operations.PrelDecisionID;

                        //Update the draft model
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.DraftOperations))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.DraftOperations, Pictures);
                            await AddVideoLinks(ncrModel.DraftOperations, Links);

                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";

                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && oper.IsNCRDraft == true)
                    {
                        //Create quality object inside ncrToUpdate
                        Operations draftOper = new Operations
                        {
                            CarRaised = oper.Operations.CarRaised,
                            CarNum = oper.Operations.CarNum,
                            IsFollowUpReq = oper.Operations.IsFollowUpReq,
                            FollowUpType = oper.Operations.FollowUpType,
                            ExpecDate = oper.Operations.ExpecDate,
                            OperationsDate = oper.Operations.OperationsDate,
                            OpManagerSign = oper.Operations.OpManagerSign,
                            Message = oper.Operations.Message,
                            PrelDecisionID = oper.Operations.PrelDecisionID,
                            QualityPhotos = ncrModel.DraftOperations.QualityPhotos,
                            VideoLinks = ncrModel.DraftOperations.VideoLinks
                        };

                        //Set new quality representative model and pass NCR to next phase
                        ncrModel.Operations = draftOper;
                        ncrModel.IsNCRDraft = false;
                        ncrModel.Phase = "Procurement";

                        //Update NCR
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.Operations))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.Operations, Pictures);
                            await AddVideoLinks(ncrModel.Operations, Links);
                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        //Send Notification
                        ncrToNotify.Add(ncrModel);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInProcurementRole, "fill", null, emailContent);

                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Procurement";
                    ncrModel.Operations = oper.Operations;

                    await AddQualityPhotos(ncrModel.Operations, Pictures);
                    await AddVideoLinks(ncrModel.Operations, Links);
                    _context.NCRs.Update(ncrModel);
                    await _context.SaveChangesAsync();

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                    //Send Notification
                    ncrToNotify.Add(ncrModel);
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInProcurementRole, "fill", null, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                NCR ncrError = GetNCR(oper.ID);
                if (ncrError == null)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Entry(ncrError).Reload();

                if (!NCRExists(ncrError.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrError.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrError.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency when reloading the page
            NCR ncrModelRepeat = GetNCR(oper.ID);
            if (ncrModelRepeat == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            _context.Entry(ncrModelRepeat).Reload();

            if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
            {
                if (!NCRExists(ncrModelRepeat.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrModelRepeat.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrModelRepeat.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }

            ncrModelRepeat.Operations = oper.Operations;
            TempData["SectionEdited"] = sectionEdited;

            //If NCR is a draft
            if (ncrModelRepeat.IsNCRDraft)
            {
                ncrModelRepeat.Operations.QualityPhotos = ncrModelRepeat.DraftOperations.QualityPhotos;
                ncrModelRepeat.Operations.VideoLinks = ncrModelRepeat.DraftOperations.VideoLinks;
            }

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", ncrModelRepeat);
        }

        // GET: NCR/CreateProcurement
        [Authorize(Roles = "Admin,Procurement")]
        public IActionResult CreateProcurement(int id, Byte[] rowVersion)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR proc = GetNCR(id);
            if (proc == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            if (proc.Phase != "Procurement")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was already filled.";
                return RedirectToAction(nameof(Index));
            }
            if (proc.IsNCRArchived == true)
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was Archived.";
                return RedirectToAction(nameof(Index));
            }
            if (proc.Status != "Active")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The is not active anymore.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            if (rowVersion != null)
            {
                //Check for concurrency error
                if (!proc.RowVersion.SequenceEqual(rowVersion))
                {
                    TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                    return RedirectToAction("Index");
                }
            }

            if (proc.IsNCRDraft == true)
            {
                proc.Procurement = new Procurement();
                proc.Procurement.SuppItemsBack = proc.DraftProcurement.SuppItemsBack;
                proc.Procurement.RMANo = proc.DraftProcurement.RMANo;
                proc.Procurement.NCRValue = proc.DraftProcurement.NCRValue;
                proc.Procurement.DisposeOnSite = proc.DraftProcurement.DisposeOnSite;
                proc.Procurement.ExpecDateOfReturn = (DateTime)proc.DraftProcurement.ExpecDateOfReturn;
                proc.Procurement.CarrierInfo = proc.DraftProcurement.CarrierInfo;
                proc.Procurement.SuppReturnCompleted = proc.DraftProcurement.SuppReturnCompleted;
                proc.Procurement.IsCreditExpec = proc.DraftProcurement.IsCreditExpec;
                proc.Procurement.ChargeSupplier = proc.DraftProcurement.ChargeSupplier;
                proc.Procurement.ProcurementDate = (DateOnly)proc.DraftProcurement.ProcurementDate;
                proc.Procurement.ProcurementSign = proc.DraftProcurement.ProcurementSign;
                proc.Procurement.QualityPhotos = proc.DraftProcurement.QualityPhotos;
                proc.Procurement.VideoLinks = proc.DraftProcurement.VideoLinks;
            }

            PopulateList();

            return View("Create", proc);
        }

        // POST: NCR/CreateProcurement
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Procurement")]
        public async Task<IActionResult> CreateProcurement(NCR proc, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove, string sectionEdited, Byte[] RowVersion)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //Intialize Notification sender
                    NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
                    List<NCR> ncrToNotify = new List<NCR>();
                    var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;

                    PopulateUserEmailData(selectedOptions);
                    NCR ncrModel = GetNCR(proc.ID);

                    string emailSubject = $"NCR {ncrModel.NCRNum} - {Subject}";
                    TempData["SuccessAlert"] = $"NCR {ncrModel.NCRNum}. 'Procurement' section was successfully filled.";

                    if (ncrModel != null)
                    {
                        _context.Entry(ncrModel).Property("RowVersion").OriginalValue = RowVersion;
                    }
                    _context.Entry(proc).Property("RowVersion").OriginalValue = RowVersion;

                    //Check for concurrency errors
                    if (RowVersion != null)
                    {
                        var refreshedModel = GetNCR(proc.ID);
                        if (refreshedModel == null)
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                        else if (!refreshedModel.RowVersion.SequenceEqual(RowVersion))
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                    }

                    //Save draft for the first time
                    if (draft != null && proc.IsNCRDraft == false)
                    {
                        //Create draft model and pass input data to it
                        DraftProcurement draftProc = new DraftProcurement
                        {
                            SuppItemsBack = proc.Procurement.SuppItemsBack,
                            RMANo = proc.Procurement.RMANo,
                            NCRValue = proc.Procurement.NCRValue,
                            DisposeOnSite = proc.Procurement.DisposeOnSite,
                            CarrierInfo = proc.Procurement.CarrierInfo,
                            ExpecDateOfReturn = proc.Procurement.ExpecDateOfReturn,
                            SuppReturnCompleted = proc.Procurement.SuppReturnCompleted,
                            IsCreditExpec = proc.Procurement.IsCreditExpec,
                            ChargeSupplier = proc.Procurement.ChargeSupplier,
                            ProcurementDate = proc.Procurement.ProcurementDate,
                            ProcurementSign = proc.Procurement.ProcurementSign,

                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftProcurement = draftProc;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftProcurement, Pictures);
                        await AddVideoLinks(ncrModel.DraftProcurement, Links);

                        _context.NCRs.Update(ncrModel);
                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && proc.IsNCRDraft == true)
                    {
                        //Pass data to the draft properties
                        ncrModel.DraftProcurement.SuppItemsBack = proc.Procurement.SuppItemsBack;
                        ncrModel.DraftProcurement.RMANo = proc.Procurement.RMANo;
                        ncrModel.DraftProcurement.NCRValue = proc.Procurement.NCRValue;
                        ncrModel.DraftProcurement.DisposeOnSite = proc.Procurement.DisposeOnSite;
                        ncrModel.DraftProcurement.CarrierInfo = proc.Procurement.CarrierInfo;
                        ncrModel.DraftProcurement.ExpecDateOfReturn = proc.Procurement.ExpecDateOfReturn;
                        ncrModel.DraftProcurement.SuppReturnCompleted = proc.Procurement.SuppReturnCompleted;
                        ncrModel.DraftProcurement.IsCreditExpec = proc.Procurement.IsCreditExpec;
                        ncrModel.DraftProcurement.ChargeSupplier = proc.Procurement.ChargeSupplier;
                        ncrModel.DraftProcurement.ProcurementDate = proc.Procurement.ProcurementDate;
                        ncrModel.DraftProcurement.ProcurementSign = proc.Procurement.ProcurementSign;

                        //Update the draft model
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.DraftProcurement))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.DraftProcurement, Pictures);
                            await AddVideoLinks(ncrModel.DraftProcurement, Links);
                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && proc.IsNCRDraft == true)
                    {
                        //Create quality object inside ncrToUpdate
                        Procurement draftProc = new Procurement
                        {
                            SuppItemsBack = proc.Procurement.SuppItemsBack,
                            RMANo = proc.Procurement.RMANo,
                            NCRValue = proc.Procurement.NCRValue,
                            DisposeOnSite = proc.Procurement.DisposeOnSite,
                            CarrierInfo = proc.Procurement.CarrierInfo,
                            ExpecDateOfReturn = proc.Procurement.ExpecDateOfReturn,
                            SuppReturnCompleted = proc.Procurement.SuppReturnCompleted,
                            IsCreditExpec = proc.Procurement.IsCreditExpec,
                            ChargeSupplier = proc.Procurement.ChargeSupplier,
                            ProcurementDate = proc.Procurement.ProcurementDate,
                            ProcurementSign = proc.Procurement.ProcurementSign,
                            QualityPhotos = ncrModel.DraftProcurement.QualityPhotos,
                            VideoLinks = ncrModel.DraftProcurement.VideoLinks
                        };

                        //Set new quality representative model and pass NCR to next phase
                        ncrModel.Procurement = draftProc;
                        ncrModel.IsNCRDraft = false;
                        ncrModel.Phase = "Reinspection";

                        //Update NCR
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.Operations))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.Operations, Pictures);
                            await AddVideoLinks(ncrModel.Operations, Links);
                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();

                        //Send Notification
                        ncrToNotify.Add(ncrModel);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "fill", null, emailContent);

                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Reinspection";
                    ncrModel.Procurement = proc.Procurement;

                    await AddQualityPhotos(ncrModel.Procurement, Pictures);
                    await AddVideoLinks(ncrModel.Procurement, Links);

                    _context.NCRs.Update(ncrModel);
                    await _context.SaveChangesAsync();

                    //Send Notification
                    ncrToNotify.Add(ncrModel);
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "fill", null, emailContent);

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                NCR ncrError = GetNCR(proc.ID);
                if (ncrError == null)
                {
                    TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Entry(ncrError).Reload();

                if (!NCRExists(ncrError.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrError.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrError.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency when reloading the page
            NCR ncrModelRepeat = GetNCR(proc.ID);
            if (ncrModelRepeat == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            _context.Entry(ncrModelRepeat).Reload();

            if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
            {
                if (!NCRExists(ncrModelRepeat.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrModelRepeat.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrModelRepeat.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }

            ncrModelRepeat.Procurement = proc.Procurement;
            TempData["SectionEdited"] = sectionEdited;

            //If NCR is a draft
            if (ncrModelRepeat.IsNCRDraft)
            {
                ncrModelRepeat.Procurement.QualityPhotos = ncrModelRepeat.DraftProcurement.QualityPhotos;
                ncrModelRepeat.Procurement.VideoLinks = ncrModelRepeat.DraftProcurement.VideoLinks;
            }

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", ncrModelRepeat);
        }

        // GET: NCR/CreateReinspection
        [Authorize(Roles = "Admin,Quality Inspector")]
        public IActionResult CreateReinspection(int id, Byte[] rowVersion)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR reinspec = GetNCR(id);
            if (reinspec == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            if (reinspec.Phase != "Reinspection")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was already filled.";
                return RedirectToAction(nameof(Index));
            }
            if (reinspec.IsNCRArchived == true)
            {
                TempData["ErrorAlert"] = "Unable to complete task. The NCR was Archived.";
                return RedirectToAction(nameof(Index));
            }
            if (reinspec.Status != "Active")
            {
                TempData["ErrorAlert"] = "Unable to complete task. The is not active anymore.";
                return RedirectToAction(nameof(Index));
            }
            if(rowVersion != null)
            {
                //Check for concurrency error
                if (!reinspec.RowVersion.SequenceEqual(rowVersion))
                {
                    TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                    return RedirectToAction("Index");
                }
            }

            if (reinspec.IsNCRDraft == true)
            {
                reinspec.Reinspection = new Reinspection();
                reinspec.Reinspection.ReinspecAccepted = reinspec.DraftReinspection.ReinspecAccepted;
                reinspec.Reinspection.NewNCRNum = reinspec.DraftReinspection.NewNCRNum;
                reinspec.Reinspection.ReinspectionDate = (DateOnly)reinspec.DraftReinspection.ReinspectionDate;
                reinspec.Reinspection.ReinspecInspectorSign = reinspec.DraftReinspection.ReinspecInspectorSign;
                reinspec.Reinspection.QualityPhotos = reinspec.DraftReinspection.QualityPhotos;
                reinspec.Reinspection.VideoLinks = reinspec.DraftReinspection.VideoLinks;
            }

            PopulateList();

            return View("Create", reinspec);
        }

        // POST: NCR/CreateReinspection
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Quality Inspector")]
        public async Task<IActionResult> CreateReinspection(NCR reinspec, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove, Byte[] RowVersion, string sectionEdited)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    //Intialize Notification sender
                    NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
                    List<NCR> ncrToNotify = new List<NCR>();
                    var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;

                    PopulateUserEmailData(selectedOptions);

                    NCR ncrModel = GetNCR(reinspec.ID);
                    NCRNumber createNCRNumber = new NCRNumber();

                    string emailSubject = $"NCR {ncrModel.NCRNum} - {Subject}";
                    TempData["SuccessAlert"] = $"NCR {ncrModel.NCRNum}. Reinspection accepted and NCR closed.";

                    _context.Entry(reinspec).Property("RowVersion").OriginalValue = RowVersion;
                    if (ncrModel != null)
                    {
                        _context.Entry(ncrModel).Property("RowVersion").OriginalValue = RowVersion;
                    }

                    //Check for concurrency errors
                    if (RowVersion != null)
                    {
                        var refreshedModel = GetNCR(reinspec.ID);
                        if(refreshedModel == null)
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                        else if (!refreshedModel.RowVersion.SequenceEqual(RowVersion))
                        {
                            throw new DbUpdateConcurrencyException("Concurrency exception occurred.");
                        }
                    }
                    NewNCRNumber(reinspec, createNCRNumber);

                    //Save draft for the first time
                    if (draft != null && reinspec.IsNCRDraft == false)
                    {
                        //Create draft model and pass input data to it
                        DraftReinspection draftReinspec = new DraftReinspection
                        {
                            ReinspecAccepted = reinspec.Reinspection.ReinspecAccepted,
                            NewNCRNum = reinspec.Reinspection.NewNCRNum,
                            ReinspectionDate = reinspec.Reinspection.ReinspectionDate,
                            ReinspecInspectorSign = reinspec.Reinspection.ReinspecInspectorSign,
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftReinspection = draftReinspec;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftReinspection, Pictures);
                        await AddVideoLinks(ncrModel.DraftReinspection, Links);

                        _context.NCRs.Update(ncrModel);
                        await _context.SaveChangesAsync();
                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && reinspec.IsNCRDraft == true)
                    {
                        //Pass data to the draft properties
                        ncrModel.DraftReinspection.ReinspecAccepted = reinspec.Reinspection.ReinspecAccepted;
                        ncrModel.DraftReinspection.NewNCRNum = reinspec.Reinspection.NewNCRNum;
                        ncrModel.DraftReinspection.ReinspectionDate = reinspec.Reinspection.ReinspectionDate;
                        ncrModel.DraftReinspection.ReinspecInspectorSign = reinspec.Reinspection.ReinspecInspectorSign;

                        //Update the draft model
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.DraftProcurement))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.DraftReinspection, Pictures);
                            await AddVideoLinks(ncrModel.DraftReinspection, Links);

                            _context.NCRs.Update(ncrModel);
                        }

                        await _context.SaveChangesAsync();
                        TempData["SuccessAlert"] = $"NCR saved as draft. To resume completing this NCR, select the 'fill' option from the three dots menu.";
                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && reinspec.IsNCRDraft == true)
                    {
                        //Create quality object inside ncrToUpdate
                        Reinspection draftReinspec = new Reinspection
                        {
                            ReinspecAccepted = reinspec.Reinspection.ReinspecAccepted,
                            NewNCRNum = reinspec.Reinspection.NewNCRNum,
                            ReinspectionDate = reinspec.Reinspection.ReinspectionDate,
                            ReinspecInspectorSign = reinspec.Reinspection.ReinspecInspectorSign,
                            QualityPhotos = ncrModel.DraftReinspection.QualityPhotos,
                            VideoLinks = ncrModel.DraftReinspection.VideoLinks
                        };

                        ncrModel.Reinspection = draftReinspec;
                        ncrModel.IsNCRDraft = false;
                        ncrModel.Phase = "Complete";

                        //Decide if NCR was rejected or not
                        if (reinspec.Reinspection.ReinspecAccepted == true)
                        {
                            ncrModel.Status = "Closed";
                        }
                        else if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                        {
                            ncrModel.Status = "Rejected";
                        }
                        else if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID != null)
                        {
                            ncrModel.Status = "Rejected Again";
                        }

                        //Update NCR
                        if (await TryUpdateModelAsync<NCR>(ncrModel, "", ncr => ncr.Reinspection))
                        {
                            if (imagesToRemove != null)
                            {
                                foreach (var image in imagesToRemove)
                                {
                                    // Find the image in the QualityRepresentative collection by ID
                                    var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                                    _context.QualityPhotos.Remove(imageToRemove);
                                }
                            }
                            if (linksToRemove != null)
                            {
                                foreach (var link in linksToRemove)
                                {
                                    // Find the image by ID
                                    var linkToRemove = await _context.VideoLinks.FindAsync(link);
                                    _context.VideoLinks.Remove(linkToRemove);
                                }
                            }

                            await AddQualityPhotos(ncrModel.Reinspection, Pictures);
                            await AddVideoLinks(ncrModel.Reinspection, Links);

                        }

                        //In case reinspection failed repeat NCR process
                        if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                        {
                            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                            NCR repeatNCR = new NCR
                            {
                                PrevNCRID = ncrModel.ID,
                                Status = "Active",
                                NCRNum = reinspec.NCRNum,
                                Phase = "Quality Representative",
                                CreatedOnDO = DateOnly.FromDateTime(nowToronto),
                                CreatedOn = nowToronto
                            };

                            _context.NCRNumbers.Add(createNCRNumber);
                            _context.NCRs.Add(repeatNCR);

                            if (await TryUpdateModelAsync<NCR>(ncrModel, ""))
                            {
                                ncrModel.IsNCRDraft = false;
                                ncrModel.NewNCRID = repeatNCR.ID;
                                _context.NCRs.Update(ncrModel);
                            }

                            await _context.SaveChangesAsync();

                            //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                            TempData["WarningAlert"] = $"NCR {ncrModel.NCRNum}. Reinspection rejected. A new NCR, numbered {repeatNCR.NCRNum}, was created and linked to the previous one.";
                            TempData["InfoAlertCreate"] = $"For your convenience, some of the data from the previous NCR was passed to this one, you can change it if necessary.";

                            //Send Notification
                            ncrToNotify.Add(repeatNCR);
                            notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "rejected", null, emailContent);

                            return RedirectToAction("CreateQualityRepresentative", new { id = repeatNCR.ID});
                        }

                        _context.NCRs.Update(ncrModel);
                        await _context.SaveChangesAsync();

                        ncrToNotify.Add(ncrModel);

                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                        if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID != null)
                        {
                            TempData["WarningAlert"] = $"NCR {ncrModel.NCRNum}. Reinspection rejected for a second time. The NCR was closed.";
                            //Send Notification
                            notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "rejected", null, emailContent);
                            return RedirectToAction("Index");
                        }

                        //Send Notification
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "closed", null, emailContent);

                        return RedirectToAction("Index");

                    }

                    ncrModel.Phase = "Complete";

                    //Decide if NCR was rejected or not
                    if (reinspec.Reinspection.ReinspecAccepted == true)
                    {
                        ncrModel.Status = "Closed";
                    }
                    else if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                    {
                        ncrModel.Status = "Rejected";
                    }
                    else if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID != null)
                    {
                        ncrModel.Status = "Rejected Again";
                    }

                    //Use this to verify if the reinpection hasn't yet been completed through the draft conditions
                    ncrModel.Reinspection = reinspec.Reinspection;

                    await AddQualityPhotos(ncrModel.Reinspection, Pictures);
                    await AddVideoLinks(ncrModel.Reinspection, Links);

                    //In case reinpection failed repeat NCR process
                    if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                    {
                        var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
                        NCR repeatNCR = new NCR
                        {
                            PrevNCRID = ncrModel.ID,
                            Status = "Active",
                            NCRNum = reinspec.NCRNum,
                            Phase = "Quality Representative",
                            CreatedOnDO = DateOnly.FromDateTime(nowToronto),
                            CreatedOn = nowToronto
                        };

                        _context.NCRs.Add(repeatNCR);
                        _context.NCRNumbers.Add(createNCRNumber);

                        if (await TryUpdateModelAsync<NCR>(ncrModel, ""))
                        {
                            ncrModel.NewNCRID = repeatNCR.ID;
                            _context.NCRs.Update(ncrModel);
                        }

                        //Test this
                        await _context.SaveChangesAsync();


                        //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                        TempData["WarningAlert"] = $"NCR {ncrModel.NCRNum}. Reinspection rejected. A new NCR, numbered {repeatNCR.NCRNum}, was created and linked to the previous one.";
                        TempData["InfoAlertCreate"] = $"For your convenience, some of the data from the previous NCR was passed to this one, you can change it if necessary.";

                        //Send Notification
                        ncrToNotify.Add(repeatNCR);
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "rejected", null, emailContent);

                        _context.Entry(ncrModel).Reload();
                        return RedirectToAction("CreateQualityRepresentative", new { id = repeatNCR.ID});
                    }

                    //Test this
                    _context.NCRs.Update(ncrModel);
                    await _context.SaveChangesAsync();

                    //Send Notification
                    ncrToNotify.Add(ncrModel);

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);
                    if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID != null)
                    {
                        TempData["WarningAlert"] = $"NCR {ncrModel.NCRNum}. Reinspection rejected for a second time. The NCR was closed.";
                        notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "rejectedAgain", null, emailContent);
                        return RedirectToAction("Index");
                    }

                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "close", null, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                NCR ncrError = GetNCR(reinspec.ID);
                if (ncrError == null)
                {
                    TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Entry(ncrError).Reload();

                if (!NCRExists(ncrError.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrError.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrError.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            NCR ncrModelRepeat = GetNCR(reinspec.ID);
            if (ncrModelRepeat == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            _context.Entry(ncrModelRepeat).Reload();

            if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
            {
                if (!NCRExists(ncrModelRepeat.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrModelRepeat.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrModelRepeat.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }

            ncrModelRepeat.Reinspection = reinspec.Reinspection;
            TempData["SectionEdited"] = sectionEdited;

            //If NCR is a draft
            if (ncrModelRepeat.IsNCRDraft)
            {
                ncrModelRepeat.Reinspection.QualityPhotos = ncrModelRepeat.DraftReinspection.QualityPhotos;
                ncrModelRepeat.Reinspection.VideoLinks = ncrModelRepeat.DraftReinspection.VideoLinks;
            }

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", ncrModelRepeat);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id, Byte[] rowVersion)
        {
            int[] imagesToRemove = Array.Empty<int>();
            int[] linksToRemove = Array.Empty<int>();
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            if (id == null || _context.NCRs == null)
            {
                return NotFound();
            }

            var ncr = await _context.NCRs
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.EngReview)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Operations)
                    .Include(p => p.Operations.QualityPhotos)
                    .Include(p => p.Operations.PrelDecision)
                    .Include(p => p.Operations.VideoLinks)
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.ProcessApplicable)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                    .Include(qr => qr.QualityRepresentative.Part)
                .Include(n => n.Procurement)
                    .Include(e => e.Procurement.QualityPhotos)
                    .Include(p => p.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(r => r.Reinspection.QualityPhotos)
                    .Include(r => r.Reinspection.VideoLinks)
                .FirstOrDefaultAsync(p => p.ID == id);


            if (ncr == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            if (!ncr.RowVersion.SequenceEqual(rowVersion))
            {
                TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                return RedirectToAction("Index");
            }

            PopulateList();

            return View(ncr);
        }

        // POST: NCR/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Byte[] RowVersion, List<IFormFile> Pictures, string[] selectedOptions, int[] imagesToRemove, int[] linksToRemove, string Subject, string emailContent, string[] Links, string sectionEdited, string partSel, string supSel, string problemSel)
        {
            PopulateUserEmailData(selectedOptions);

            //Go get the customer to update
            var ncrToUpdate = await _context.NCRs
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.EngReview)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Operations)
                    .Include(p => p.Operations.QualityPhotos)
                    .Include(p => p.Operations.PrelDecision)
                    .Include(p => p.Operations.VideoLinks)
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.ProcessApplicable)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                    .Include(qr => qr.QualityRepresentative.Part)
                .Include(n => n.Procurement)
                    .Include(e => e.Procurement.QualityPhotos)
                    .Include(p => p.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(r => r.Reinspection.QualityPhotos)
                    .Include(r => r.Reinspection.VideoLinks)
                .FirstOrDefaultAsync(p => p.ID == id);

            //get partSelected
            if (partSel != null)
            {
                try
                {
                    int indexPart = partSel.IndexOf(' ');
                    string partNumber = partSel.Substring(0, indexPart);
                    var getPart = _context.Parts.FirstOrDefault(p => p.PartNumber == Convert.ToInt32(partNumber));
                    if (getPart != null)
                    {
                        ncrToUpdate.QualityRepresentative.PartID = getPart.ID;
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("QualityRepresentative.PartID", "Invalid Part Selection");
                }
            }
            //get supplierSelected
            if (supSel != null)
            {
                try
                {
                    int indexSup = supSel.IndexOf(' ');
                    string supCode = supSel.Substring(0, indexSup);
                    var getSup = _context.Suppliers.FirstOrDefault(p => p.SupplierCode == Convert.ToInt32(supCode));
                    if (getSup != null)
                    {
                        ncrToUpdate.QualityRepresentative.SupplierID = getSup.ID;
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("QualityRepresentative.SupplierID", "Invalid Supplier Selection");
                }
            }
            //get problem Selected
            if (problemSel != null)
            {
                try
                {
                    var getProblem = _context.Problems.FirstOrDefault(p => p.ProblemDescription == problemSel);
                    if (getProblem != null)
                    {
                        ncrToUpdate.QualityRepresentative.ProblemID = getProblem.ID;
                    }
                }
                catch (Exception)
                {
                    ModelState.AddModelError("QualityRepresentative.ProblemID", "Invalid Problem Selection");
                }
            }

            //Intialize Notification sender
            var usersInAdminRole = _userManager.GetUsersInRoleAsync("Admin").Result;
            var usersInQualInspRole = _userManager.GetUsersInRoleAsync("Quality Inspector").Result;
            var usersInEngineeringRole = _userManager.GetUsersInRoleAsync("Engineer").Result;
            var usersInOperationsRole = _userManager.GetUsersInRoleAsync("Operations Manager").Result;
            var usersInProcurementRole = _userManager.GetUsersInRoleAsync("Procurement").Result;
            NotificationGenerator notificationGenerator = new NotificationGenerator(_context, _emailSender, _userManager);
            List<NCR> ncrToNotify = new List<NCR>();

            //Check that we got the NCR or exit with a not found error
            if (ncrToUpdate == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Check for concurrency error
            if (!ncrToUpdate.RowVersion.SequenceEqual(RowVersion))
            {
                TempData["ErrorAlert"] = "This NCR was edited or modified by another user. Try again.";
                return RedirectToAction("Index");
            }

            //Check if the section being edited is null
            else if (sectionEdited == "QualityRepresentative" && !QualRepExists(ncrToUpdate.QualityRepresentative.ID))
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            else if (sectionEdited == "Engineering" && !EngineeringExists(ncrToUpdate.Engineering.ID))
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            else if (sectionEdited == "Operations" && !OperationsExists(ncrToUpdate.Operations.ID))
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            else if (sectionEdited == "Procurement" && !ProcurementExists(ncrToUpdate.Procurement.ID))
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }
            else if (sectionEdited == "Reinspection" && !ReinspectionExists(ncrToUpdate.Reinspection.ID))
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            //Put the original RowVersion value in the OriginalValues collection for the entity
            _context.Entry(ncrToUpdate).Property("RowVersion").OriginalValue = RowVersion;

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.RowVersion, ncr => ncr.QualityRepresentative, ncr => ncr.Engineering, ncr => ncr.Operations, ncr => ncr.Procurement, ncr => ncr.Reinspection))
            {
                try
                {
                    if (imagesToRemove != null)
                    {
                        foreach (var image in imagesToRemove)
                        {
                            // Find the image in the QualityRepresentative collection by ID
                            var imageToRemove = await _context.QualityPhotos.FindAsync(image);
                            _context.QualityPhotos.Remove(imageToRemove);
                        }
                    }
                    if (linksToRemove != null)
                    {
                        foreach (var link in linksToRemove)
                        {
                            // Find the image by ID
                            var linkToRemove = await _context.VideoLinks.FindAsync(link);
                            _context.VideoLinks.Remove(linkToRemove);
                        }
                    }

                    switch (sectionEdited)
                    {
                        case "Engineering":
                            await AddQualityPhotos(ncrToUpdate.Engineering, Pictures);
                            await AddVideoLinks(ncrToUpdate.Engineering, Links);
                            break;
                        case "Operations":
                            await AddQualityPhotos(ncrToUpdate.Operations, Pictures);
                            await AddVideoLinks(ncrToUpdate.Operations, Links);
                            break;
                        case "QualityRepresentative":
                            await AddQualityPhotos(ncrToUpdate.QualityRepresentative, Pictures);
                            await AddVideoLinks(ncrToUpdate.QualityRepresentative, Links);
                            break;
                        case "Procurement":
                            await AddQualityPhotos(ncrToUpdate.Procurement, Pictures);
                            await AddVideoLinks(ncrToUpdate.Procurement, Links);
                            break;
                        case "Reinspection":
                            await AddQualityPhotos(ncrToUpdate.Reinspection, Pictures);
                            await AddVideoLinks(ncrToUpdate.Reinspection, Links);
                            break;
                        default:
                            // Handle if sectionEdited doesn't match any case
                            break;
                    }
                    string emailSubject = $"NCR {ncrToUpdate.NCRNum} - {Subject}";

                    ncrToUpdate.SectionUpdated = sectionEdited;

                    _context.Update(ncrToUpdate);
                    await _context.SaveChangesAsync();

                    //Send Notification
                    ncrToNotify.Add(ncrToUpdate);
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInQualInspRole, "edited", "noAdmin");
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInEngineeringRole, "edited", "noAdmin");
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInOperationsRole, "edited", "noAdmin");
                    notificationGenerator.SendNotificationsForNCRList(ncrToNotify, (List<IdentityUser>)usersInProcurementRole, "edited", "noAdmin");
                    notificationGenerator.SendNotificationsForNCRListAdmin(ncrToNotify, (List<IdentityUser>)usersInAdminRole, "edited");

                    //SendNotificationEmail(selectedOptions, emailSubject, emailContent);

                    //Send on to details with the section edited open
                    TempData["SectionEdited"] = sectionEdited;
                    return RedirectToAction("Details", new { id = ncrToUpdate.ID });
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
                }
                catch (DbUpdateConcurrencyException)
                {
                    NCR ncrError = GetNCR(ncrToUpdate.ID);
                    if (ncrError == null)
                    {
                        TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                        return RedirectToAction(nameof(Index));
                    }
                    _context.Entry(ncrError).Reload();

                    if (!NCRExists(ncrError.ID))
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit doesn't exist anymore.";
                    }
                    else if(ncrError.IsNCRArchived)
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit was archived by another user.";
                    }
                    else if (ncrError.Status == "Voided")
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit was voided by another user.";
                    }
                    else
                    {
                        TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Please try to submit again.";
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
                catch (Exception)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                    return RedirectToAction(nameof(Index));
                }
            }
            PopulateList();

            //Check for concurrency error
            NCR ncrModelRepeat = GetNCR(id);
            if (ncrModelRepeat == null)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Please try again, if error persist see your system administrator.";
                return RedirectToAction(nameof(Index));
            }

            _context.Entry(ncrModelRepeat).Reload();

            if (!ncrModelRepeat.RowVersion.SequenceEqual(RowVersion))
            {
                if (!NCRExists(ncrModelRepeat.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create doesn't exist anymore.";
                }
                else if (ncrModelRepeat.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was archived by another user.";
                }
                else if (ncrModelRepeat.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit or create was modified by another user. Please try to submit again.";
                }
                return RedirectToAction(nameof(Index));
            }

            if (sectionEdited != null)
            {
                TempData["SectionEdited"] = sectionEdited;
            }
            return View(ncrToUpdate);
        }

        private SelectList SupplierList(int? selectedId)
        {
            return new SelectList(_context
                .Suppliers
                .OrderBy(m => m.SupplierName), "ID", "SupplierName", selectedId);
        }
        [HttpGet]
        public JsonResult GetSuppliers(int? id)
        {
            return Json(SupplierList(id));
        }
        private SelectList ProblemList(int? selectedId)
        {
            return new SelectList(_context
                .Problems
                .OrderBy(m => m.ProblemDescription), "ID", "ProblemDescription", selectedId);
        }
        private SelectList ProcessApplicableList(int? selectedId)
        {
            return new SelectList(_context
                .ProcessesApplicable
                .OrderBy(m => m.ProcessName), "ID", "ProcessName", selectedId);
        }
        private SelectList PartList(int? selectedId)
        {
            return new SelectList(_context
                .Parts
                .OrderBy(m => m.PartNumber), "ID", "PartSummary", selectedId);
        }

        private SelectList EngReviewList(int? selectedId)
        {
            return new SelectList(_context
                .EngReviews
                .OrderBy(m => m.Review), "ID", "Review", selectedId);
        }
        private SelectList PrelDecisionList(int? selectedId)
        {
            return new SelectList(_context
                .PrelDecisions
                .OrderBy(m => m.Decision), "ID", "Decision", selectedId);
        }
        private SelectList PartSummaryList(string partSummary)
        {
            return new SelectList(_context
                .Parts
                .OrderBy(m => m.PartNumber), "PartSummary", "PartSummary", partSummary);
        }
        private SelectList SupplierSummaryList(string partSummary)
        {
            return new SelectList(_context
                .Suppliers
                .OrderBy(m => m.SupplierCode), "SupplierSummary", "SupplierSummary", partSummary);
        }
        private SelectList ProblemDescriptionList(string partSummary)
        {
            return new SelectList(_context
                .Problems
                .OrderBy(m => m.ProblemDescription), "ProblemDescription", "ProblemDescription", partSummary);
        }

        private void PopulateList(NCR? ncr = null)
        {
            QualityRepresentative qualityRepresentative = null;
            Engineering engineering = null;
            Operations Operations = null;

            ViewData["SupplierID"] = SupplierList(qualityRepresentative?.SupplierID);
            ViewData["ProblemID"] = ProblemList(qualityRepresentative?.ProblemID);
            ViewData["ProcessApplicableID"] = ProcessApplicableList(qualityRepresentative?.ProcessApplicableID);
            ViewData["PartID"] = PartList(qualityRepresentative?.PartID);
            ViewData["EngReviewID"] = EngReviewList(engineering?.EngReviewID);
            ViewData["PrelDecisionID"] = PrelDecisionList(Operations?.PrelDecisionID);
            //Draft Models
            if (ncr != null)
            {
                ViewData["SupplierID"] = SupplierList(ncr.DraftQualityRepresentative?.SupplierID);
                ViewData["ProblemID"] = ProblemList(ncr.DraftQualityRepresentative?.ProblemID);
                ViewData["ProcessApplicableID"] = ProcessApplicableList(ncr.DraftQualityRepresentative?.ProcessApplicableID);
                ViewData["PartID"] = PartList(ncr.DraftQualityRepresentative?.PartID);
            }
            ViewData["PartSummary"] = PartSummaryList(qualityRepresentative?.Part.PartSummary);
            ViewData["SupplierSummary"] = SupplierSummaryList(qualityRepresentative?.Supplier.SupplierSummary);
            ViewData["ProblemDescription"] = ProblemDescriptionList(qualityRepresentative?.Problem.ProblemDescription);
        }

        //Helper method to create new NCR number
        private async void NewNCRNumber(NCR ncr, NCRNumber createNCRNumber)
        {
            //Get last NCR number
            NCRNumber lastNCRNum = await _context.NCRNumbers
                .OrderByDescending(n => n.ID)
                .FirstOrDefaultAsync();

            var nowToronto = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));

            //Create new NCR number
            createNCRNumber.Year = nowToronto.Year;

            if (lastNCRNum == null)
            {
                ncr.NCRNum = createNCRNumber.GenerateNCRNumber(false, 0);
            }
            else if (lastNCRNum != null)
            {
                if (lastNCRNum.Year != nowToronto.Year)
                {
                    ncr.NCRNum = createNCRNumber.GenerateNCRNumber(true, lastNCRNum.Counter);
                }
                else
                {
                    ncr.NCRNum = createNCRNumber.GenerateNCRNumber(false, lastNCRNum.Counter);
                }
            }
        }

        // Method to send notification email
        private async void SendNotificationEmail(string[] selectedOptions, string Subject, string emailContent)
        {
            if (string.IsNullOrEmpty(Subject) || string.IsNullOrEmpty(emailContent))
            {
                ViewData["Message"] = "You must enter both a Subject and some message Content before sending the message.";
            }
            else
            {
                var selectedOptionsHS = new HashSet<string>(selectedOptions);
                List<EmailAddress> staff = (from n in _context.Employees
                                            where !selectedOptionsHS.Contains(n.ID.ToString())
                                            select new EmailAddress
                                            {
                                                Name = n.FullName,
                                                Address = n.Email
                                            }).ToList();
                int staffCount = staff.Count;
                int selectedCount = selectedOptionsHS.Count;
                if (staffCount > 0)
                {
                    try
                    {
                        var msg = new EmailMessage()
                        {
                            ToAddresses = staff,
                            Subject = Subject,
                            Content = "<p>" + emailContent + "</p><p>Please complete your section of the form</p>"
                        };
                        await _emailSender.SendToManyAsync(msg);
                    }
                    catch (Exception ex)
                    {
                        string errMsg = ex.GetBaseException().Message;
                        ViewData["Message"] = "Error: Could not send email message to the selected customers.";
                    }
                }
            }
        }

        // Method to populate users inside the selector box
        private async void PopulateUserEmailData(string[] selectedOptions)
        {
            var allOptions = _context.Employees
                .Where(d => d.Email != null)
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName);

            //var currentOptionsHS = new HashSet<int>(allOptions.Select(b => b.ID));
            var selectedOptionsHS = new HashSet<string>(selectedOptions);

            //Instead of one list with a boolean, we will make two lists
            var selected = new List<ListOptionVM>();
            var available = new List<ListOptionVM>();
            //Append default users to the list

            foreach (var c in allOptions)
            {
                var user = await _userManager.FindByEmailAsync(c.Email);

                if (user != null)
                {
                    //Get the user role
                    List<string> userRoles = (List<string>)await _userManager.GetRolesAsync(user);
                    string rolesAsString = userRoles != null && userRoles.Any() ? string.Join(", ", userRoles) : "No role";

                    if (selectedOptionsHS.Contains(c.ID.ToString()))
                    {
                        selected.Add(new ListOptionVM
                        {
                            ID = c.ID,
                            DisplayText = $"{c.Email} - {rolesAsString}"
                        });
                    }
                    else if (c.Active == true)
                    {
                        available.Add(new ListOptionVM
                        {
                            ID = c.ID,
                            DisplayText = $"{c.Email} - {rolesAsString}"
                        });
                    }
                }
            }

            ViewData["selOpts"] = new MultiSelectList(selected.OrderBy(s => s.DisplayText), "ID", "DisplayText");
            ViewData["availOpts"] = new MultiSelectList(available.OrderBy(s => s.DisplayText), "ID", "DisplayText");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ArchiveManyNCRs(int[] NCRsID)
        {
            var NCRsToArchive = await _context.NCRs
                .Where(n => NCRsID.Contains(n.ID))
                .ToListAsync();

            if (NCRsToArchive == null)
            {
                TempData["ErrorAlert"] = "The record or records you attempted to archive were modified by another user. Try again and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            int successCount = 0;
            int failedCount = 0;

            foreach (var ncr in NCRsToArchive)
            {
                try
                {
                    if (await TryUpdateModelAsync<NCR>(ncr, ""))
                    {
                        ncr.IsNCRArchived = true;

                        _context.NCRs.Update(ncr);
                    }
                    await _context.SaveChangesAsync();

                    successCount++;
                }
                catch (Exception)
                {
                    failedCount++;
                }
            }

            TempData["ArchivingInfoAlert"] = $"{successCount} NCRs were succesfully archived and {failedCount} failed to be archived, out of {failedCount + successCount} total.";
            if(failedCount > 1)
            {
                TempData["ArchivingWarningAlert"] = $"Failed archivation attempts can be caused by a variety of different factors, try again if problems persist, see your system administrator";
            }

            return RedirectToAction("Archiving");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ArchiveNCR(int id, string rowVersion)
        {
            NCR ncrToArchive = _context.NCRs.FirstOrDefault(n => n.ID == id);

            if (ncrToArchive == null)
            {
                return NotFound();
            }

            Byte[] rowVersionByteArray = Convert.FromBase64String(rowVersion);
            _context.Entry(ncrToArchive).Property("RowVersion").OriginalValue = rowVersionByteArray;

            try
            {
                if (await TryUpdateModelAsync<NCR>(ncrToArchive, ""))
                {
                    ncrToArchive.IsNCRArchived = true;

                    _context.NCRs.Update(ncrToArchive);

                }
                await _context.SaveChangesAsync();
                TempData["SuccessAlert"] = "NCR was successfully archived. Refresh the page to see changes. The archived NCRs can be seen by checking the 'Show inactive NCRs' checkbox.";
            }
            catch (RetryLimitExceededException /* dex */)
            {
                TempData["ErrorAlert"] = "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.";
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(ncrToArchive).Reload();

                if (!NCRExists(ncrToArchive.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit doesn't exist anymore.";
                }
                else if (ncrToArchive.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was archived by another user.";
                }
                else if (ncrToArchive.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorAlert"] = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Try again, and if the problem persists see your system administrator.";
            }

            var response = new
            {
                successAlert = TempData["SuccessAlert"] as string,
                errorAlert = TempData["ErrorAlert"] as string,
            };

            return Json(response);
        }

        public async Task<IActionResult> CancelDraft(int id, Byte[] RowVersion)
        {
            var ncrDraft = _context.NCRs
                .Include(n => n.DraftQualityRepresentative)
                    .Include(n => n.DraftQualityRepresentative.QualityPhotos)
                    .Include(n => n.DraftQualityRepresentative.VideoLinks)
                .Include(n => n.DraftEngineering)
                    .Include(n => n.DraftEngineering.QualityPhotos)
                    .Include(n => n.DraftEngineering.VideoLinks)
                .Include(n => n.DraftOperations)
                    .Include(n => n.DraftOperations.QualityPhotos)
                    .Include(n => n.DraftOperations.VideoLinks)
                .Include(n => n.DraftProcurement)
                    .Include(n => n.DraftProcurement.QualityPhotos)
                    .Include(n => n.DraftProcurement.VideoLinks)
                .Include(n => n.DraftReinspection)
                    .Include(n => n.DraftReinspection.QualityPhotos)
                    .Include(n => n.DraftReinspection.VideoLinks)
                .FirstOrDefault(n => n.ID == id);

            if (ncrDraft == null)
            {
                TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            _context.Entry(ncrDraft).Property("RowVersion").OriginalValue = RowVersion;

            try
            {
                if(ncrDraft.IsNCRDraft == true && ncrDraft != null)
                {
                    //If phase is Quality Representative, delete the NCR
                    if (ncrDraft.Phase == "Quality Representative")
                    {
                        ncrDraft.IsNCRDraft = false;

                        if (await TryUpdateModelAsync<NCR>(ncrDraft, "", ncr => ncr.DraftQualityRepresentative))
                        {
                            foreach (var image in ncrDraft.DraftQualityRepresentative.QualityPhotos)
                            {
                                // Find the image in the QualityRepresentative collection by ID
                                var imageToRemove = await _context.QualityPhotos.FirstOrDefaultAsync(img => img.ID == image.ID);
                                _context.QualityPhotos.Remove(imageToRemove);
                            }
                            foreach (var video in ncrDraft.DraftQualityRepresentative.VideoLinks)
                            {
                                // Find the image in the QualityRepresentative collection by ID
                                var videoToRemove = await _context.VideoLinks.FirstOrDefaultAsync(vd => vd.ID == video.ID);
                                _context.VideoLinks.Remove(videoToRemove);
                            }
                        }

                        _context.DraftQualityRepresentatives.Remove(ncrDraft.DraftQualityRepresentative);
                        _context.NCRs.Remove(ncrDraft);

                        await _context.SaveChangesAsync();
                        TempData["SuccessAlert"] = "Draft was successfully canceled.";
                        return RedirectToAction("Index");
                    }
                    //If phase is Engineering delete the Engineering draft
                    else if (ncrDraft.Phase == "Engineering")
                    {
                        ncrDraft.IsNCRDraft = false;

                        _context.DraftEngineerings.Remove(ncrDraft.DraftEngineering);
                    }
                    //If phase is Operations delete the Operations draft
                    else if (ncrDraft.Phase == "Operations")
                    {
                        ncrDraft.IsNCRDraft = false;

                        _context.DraftOperationsS.Remove(ncrDraft.DraftOperations);
                    }
                    //If phase is Procurement delete the Procurement draft
                    else if (ncrDraft.Phase == "Procurement")
                    {
                        ncrDraft.IsNCRDraft = false;

                        _context.DraftProcurements.Remove(ncrDraft.DraftProcurement);
                    }
                    //If phase is Operations delete the Operations draft
                    else if (ncrDraft.Phase == "Reinspection")
                    {
                        ncrDraft.IsNCRDraft = false;

                        _context.DraftReinspections.Remove(ncrDraft.DraftReinspection);
                    }

                    _context.NCRs.Update(ncrDraft);
                    await _context.SaveChangesAsync();
                    TempData["SuccessAlert"] = "Draft was successfully canceled.";

                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                TempData["ErrorAlert"] = "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.";
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(ncrDraft).Reload();

                if (!NCRExists(ncrDraft.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit doesn't exist anymore.";
                }
                else if (ncrDraft.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was archived by another user.";
                }
                else if (ncrDraft.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorAlert"] = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Try again, and if the problem persists see your system administrator.";
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VoidNCR(int id, string voidingReason, string rowVersion, string cancel)
        {
            var ncrToVoid = _context.NCRs.FirstOrDefault(ncr => ncr.ID == id);

            if (ncrToVoid == null)
            {
                if(cancel == null)
                {
                    return NotFound();
                }

                TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                return RedirectToAction("Index");
            }

            Byte[] rowVersionByteArray = Convert.FromBase64String(rowVersion);
            _context.Entry(ncrToVoid).Property("RowVersion").OriginalValue = rowVersionByteArray;

            try
            {
                if (ncrToVoid.Status == "Voided")
                {
                    if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
                    {
                        ncrToVoid.VoidingReason = null;
                        ncrToVoid.Status = "Active";

                        _context.NCRs.Update(ncrToVoid);

                    }
                    await _context.SaveChangesAsync();

                    TempData["SuccessAlert"] = "NCR Void was successfuly canceled.";

                    if (cancel != null)
                    {
                        return RedirectToAction("Index");
                    }
                }

                if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
                {
                    ncrToVoid.VoidingReason = voidingReason;
                    ncrToVoid.Status = "Voided";

                    _context.NCRs.Update(ncrToVoid);

                }
                await _context.SaveChangesAsync();

                TempData["SuccessAlert"] = "NCR was successfuly voided. Refresh the page to see changes. The voided NCRs can be seen by checking the 'Show inactive NCRs' checkbox.";

            }
            catch (RetryLimitExceededException /* dex */)
            {
                TempData["ErrorAlert"] = "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.";
            }
            catch (DbUpdateConcurrencyException)
            {
                _context.Entry(ncrToVoid).Reload();

                if (!NCRExists(ncrToVoid.ID))
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit doesn't exist anymore.";
                }
                else if (ncrToVoid.IsNCRArchived)
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was archived by another user.";
                }
                else if (ncrToVoid.Status == "Voided")
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was voided by another user.";
                }
                else
                {
                    TempData["ErrorAlert"] = "The record you attempted to edit was modified by another user. Try again and if the problem persists see your system administrator.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["ErrorAlert"] = "Unable to save changes. Try again, and if the problem persists see your system administrator.";
            }
            catch (Exception)
            {
                TempData["ErrorAlert"] = "Unable to complete task. Try again, and if the problem persists see your system administrator.";

            }

            if (cancel != null)
            {
                return RedirectToAction("Index");
            }

            var response = new
            {
                successAlert = TempData["SuccessAlert"] as string,
                errorAlert = TempData["ErrorAlert"] as string,
            };

            return Json(response);
        }

        public static Task AddVideoLinks<T>(T module, string[] links) where T : class
        {
            var videoLinksProperty = module.GetType().GetProperty("VideoLinks");
            var videoLinks = videoLinksProperty.GetValue(module) as List<VideoLink>;
            if (videoLinks == null)
            {
                videoLinks = new List<VideoLink>();
            }
            if (links != null && links.Any())
            {
                foreach (var link in links)
                {
                    if (!string.IsNullOrEmpty(link))
                    {
                        videoLinks.Add(new VideoLink { Link = link.Trim() }); // Trim to remove leading/trailing whitespaces
                    }
                }
            }

            videoLinksProperty.SetValue(module, videoLinks);
            return Task.CompletedTask;
        }

        // Method to add Photos
        public static async Task AddQualityPhotos<T>(T module, List<IFormFile> photos) where T : class
        {
            var qualityPhotosProperty = module.GetType().GetProperty("QualityPhotos");
            if (qualityPhotosProperty != null && qualityPhotosProperty.PropertyType == typeof(List<Photo>))
            {
                var qualityPhotos = qualityPhotosProperty.GetValue(module) as List<Photo>;
                if (qualityPhotos == null)
                {
                    qualityPhotos = new List<Photo>();
                }

                foreach (var photo in photos)
                {
                    string mimeType = photo.ContentType;
                    long fileLength = photo.Length;

                    if (!(string.IsNullOrEmpty(mimeType) || fileLength == 0))
                    {
                        if (mimeType.Contains("image"))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await photo.CopyToAsync(memoryStream);
                                memoryStream.Position = 0;

                                using (var image = Image.Load(memoryStream))
                                {
                                    // Resize and compress the image
                                    var options = new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder()
                                    {
                                        Quality = 70 // Adjust quality as per your requirement
                                    };

                                    // Define the percentage by which you want to resize the image
                                    double resizePercentage = 0.6; // Resize to 50% of the original size

                                    // Check if either width or height exceeds the threshold size
                                    if ((image.Width > 1500 && image.Width <= 3000) || (image.Height > 1500 && image.Height <= 3000))
                                    {
                                        // Calculate the new dimensions based on the percentage
                                        int newWidth = (int)(image.Width * resizePercentage);
                                        int newHeight = (int)(image.Height * resizePercentage);

                                        // Resize the image to the new dimensions
                                        image.Mutate(x => x.Resize(newWidth, newHeight));
                                    }
                                    else if ((image.Width >= 3001 && image.Width <= 5000) || (image.Height >= 3001 && image.Height <= 3000))
                                    {
                                        resizePercentage = 0.7;
                                        // Calculate the new dimensions based on the percentage
                                        int newWidth = (int)(image.Width * resizePercentage);
                                        int newHeight = (int)(image.Height * resizePercentage);

                                        // Resize the image to the new dimensions
                                        image.Mutate(x => x.Resize(newWidth, newHeight));
                                    }
                                    else if ((image.Width >= 5001 && image.Width <= 8000) || (image.Height >= 5001 && image.Height <= 8000))
                                    {
                                        resizePercentage = 0.8;
                                        // Calculate the new dimensions based on the percentage
                                        int newWidth = (int)(image.Width * resizePercentage);
                                        int newHeight = (int)(image.Height * resizePercentage);

                                        // Resize the image to the new dimensions
                                        image.Mutate(x => x.Resize(newWidth, newHeight));
                                    }
                                    else
                                    {
                                        resizePercentage = 0.9;
                                        // Calculate the new dimensions based on the percentage
                                        int newWidth = (int)(image.Width * resizePercentage);
                                        int newHeight = (int)(image.Height * resizePercentage);

                                        // Resize the image to the new dimensions
                                        image.Mutate(x => x.Resize(newWidth, newHeight));
                                    }

                                    // Convert the image to byte array
                                    using (var outputStream = new MemoryStream())
                                    {
                                        image.Save(outputStream, options);
                                        var photoArray = outputStream.ToArray();

                                        // Create a new QualityPhoto instance for each image
                                        var qualityPhoto = new Photo
                                        {
                                            Content = photoArray,
                                            MimeType = mimeType
                                        };

                                        // Add the photo to the QualityPhotos list
                                        qualityPhotos.Add(qualityPhoto);
                                    }
                                }
                            }
                        }
                    }
                }
                // Set the updated QualityPhotos list back to the module
                qualityPhotosProperty.SetValue(module, qualityPhotos);
            }
        }
                // Helper method to retrieve the next NCR
        private NCR GetNCR(int id)
        {
            NCR ncrModel = _context.NCRs
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Part)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Operations)
                    .Include(p => p.Operations.QualityPhotos)
                    .Include(p => p.Operations.VideoLinks)
                .Include(n => n.Procurement)
                    .Include(pr => pr.Procurement.QualityPhotos)
                    .Include(pr => pr.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(r => r.Reinspection.QualityPhotos)
                    .Include(r => r.Reinspection.VideoLinks)
                //Draft models
                .Include(dqr => dqr.DraftQualityRepresentative)
                    .Include(dqr => dqr.DraftQualityRepresentative.QualityPhotos)
                    .Include(dqr => dqr.DraftQualityRepresentative.VideoLinks)
                .Include(de => de.DraftEngineering)
                    .Include(de => de.DraftEngineering.QualityPhotos)
                    .Include(de => de.DraftEngineering.VideoLinks)
                .Include(dop => dop.DraftOperations)
                    .Include(dop => dop.DraftOperations.QualityPhotos)
                    .Include(dop => dop.DraftOperations.VideoLinks)
                .Include(dp => dp.DraftProcurement)
                    .Include(dp => dp.DraftProcurement.QualityPhotos)
                    .Include(dp => dp.DraftProcurement.VideoLinks)
                .Include(dr => dr.DraftReinspection)
                    .Include(dr => dr.DraftReinspection.QualityPhotos)
                    .Include(dr => dr.DraftReinspection.VideoLinks)
                .FirstOrDefault(n => n.ID == id);

            if (ncrModel != null)
            {
                return ncrModel;
            }

            return null;
        }

        public async Task<FileContentResult> DownloadPDFAsync(int id)
        {
            var ncrDetails = await _context.NCRs
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.EngReview)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Operations)
                    .Include(o => o.Operations.QualityPhotos)
                    .Include(o => o.Operations.VideoLinks)
                    .Include(o => o.Operations.PrelDecision)
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.ProcessApplicable)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                    .Include(qr => qr.QualityRepresentative.Part)
                .Include(n => n.Procurement)
                    .Include(pr => pr.Procurement.QualityPhotos)
                    .Include(pr => pr.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(re => re.Reinspection.QualityPhotos)
                    .Include(re => re.Reinspection.VideoLinks)
                .FirstOrDefaultAsync(m => m.ID == id);
            
            MemoryStream memoryStream = new MemoryStream();

            var document = new NCRPdfReport(ncrDetails);

            document.GeneratePdf(memoryStream);

            var pdfData = memoryStream.ToArray();

            return File(pdfData, "application/pdf", "NCRDetails.pdf");

        }

        private bool NCRExists(int id)
        {
            return _context.NCRs.Any(e => e.ID == id);
        }
        private bool QualRepExists(int id)
        {
            return _context.QualityRepresentatives.Any(e => e.ID == id);
        }
        private bool EngineeringExists(int id)
        {
            return _context.Engineerings.Any(e => e.ID == id);
        }
        private bool OperationsExists(int id)
        {
            return _context.OperationsS.Any(e => e.ID == id);
        }
        private bool ProcurementExists(int id)
        {
            return _context.Procurements.Any(e => e.ID == id);
        }
        private bool ReinspectionExists(int id)
        {
            return _context.Reinspections.Any(e => e.ID == id);
        }

    }
}