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


namespace Haver.Controllers
{
    //[Authorize]
    public class NCRController : Controller
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
        public async Task<IActionResult> Index(string SearchString, int? SupplierID, int? page, int? pageSizeID, string SelectedOption
            , string actionButton, string active, string sortDirection = "desc", string sortField = "CREATED ON")
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

            if (SupplierID.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == SupplierID);
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
        public async Task<IActionResult> Archived(string SearchString, int? SupplierID, int? page, int? pageSizeID, string SelectedOption
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

            if (SupplierID.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == SupplierID);
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
        public async Task<IActionResult> Archiving(string SearchString, int? SupplierID, int? page, int? pageSizeID, string SelectedOption
            ,string actionButton, string active, string sortDirection = "desc", string sortField = "CREATED ON", DateTime? StartDate = null, DateTime? EndDate = null)
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

            if (SupplierID.HasValue)
            {
                haverContext = haverContext.Where(p => p.QualityRepresentative.SupplierID == SupplierID);
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
                partsDefectiveVM.EnoughData = qualRepsLinkedToSupplier.Count() <= 3 ? false : true;

                (double periodChange, int total, int previousTotal) periodChange = PoPChange(supplierNumber, timeSpan, "supplier");

                //Assign values to ViewModel
                partsDefectiveVM.StartDate = formattedStartDate;
                partsDefectiveVM.EndDate = formattedEndDate;
                partsDefectiveVM.PartsDefectiveAmount = periodChange.total;
                partsDefectiveVM.PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : periodChange.periodChange;
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

        public IActionResult GetNCRTimeList(string actionButton, string sortDirection = "desc", string sortField = "SINCE CREATED", int page = 1)
        {
            try
            {
                string[] sortOptions = new[] { "SINCE CREATED", "LAST FILLED" };


                //Get parts
                var NCRs = _context.NCRs
                    .Where(ncr => ncr.Status == "Active")
                    .Include(ncr => ncr.QualityRepresentative.Problem)
                    .ToList();
                var ncrTimeList = new List<NCRTimeListVM>();

                //Retrieve a list containing the parts Linked to each Quality Representative section
                foreach (var NCR in NCRs)
                {
                    //Get the number of days since last filled
                    TimeSpan differenceLastFilled;
                    int lastFilled = 0;

                    if (NCR.Phase == "Quality Representative")
                    {
                        differenceLastFilled = DateTime.Now - Convert.ToDateTime(NCR.QualityRepresentative?.QualityRepDate);
                        lastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Egineering")
                    {
                        differenceLastFilled = DateTime.Now - Convert.ToDateTime(NCR.Engineering?.EngineeringDate);
                        lastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Procurement")
                    {
                        differenceLastFilled = DateTime.Now - Convert.ToDateTime(NCR.Procurement?.ProcurementDate);
                        lastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Operations")
                    {
                        differenceLastFilled = DateTime.Now - Convert.ToDateTime(NCR.Operations?.OperationsDate);
                        lastFilled = differenceLastFilled.Days;
                    }
                    else if (NCR.Phase == "Reinspection")
                    {
                        differenceLastFilled = DateTime.Now - Convert.ToDateTime(NCR.Reinspection?.ReinspectionDate);
                        lastFilled = differenceLastFilled.Days;
                    }

                    //Get the number of days since created
                    TimeSpan differenceCreated;
                    int sinceCreated = 0;

                    differenceCreated = DateTime.Now - Convert.ToDateTime(NCR.CreatedOn);
                    sinceCreated = differenceCreated.Days;

                    NCRTimeListVM ncrRecord = new NCRTimeListVM
                    {
                        NCRNo = NCR.NCRNum,
                        Problem = NCR.QualityRepresentative?.Problem.ProblemDescription,
                        Phase = NCR.Phase,
                        LastFilled = lastFilled,
                        SinceCreated = sinceCreated
                    };

                    // Add to the list
                    ncrTimeList.Add(ncrRecord);
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
                if (sortField == "SINCE CREATED")
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
                        PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : periodChange.periodChange,
                        IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true
                    };

                    // Add to the list
                    defectivePartsList.Add(partDefective);
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
            catch(Exception)
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
                partsDefectiveVM.EnoughData = qualRepsLinkedToPart.Count() <= 3 ? false : true;

                (double periodChange, int total, int previousTotal) periodChange = PoPChange(partNumber, timeSpan, "part");

                //Assign values to ViewModel
                partsDefectiveVM.StartDate = formattedStartDate;
                partsDefectiveVM.EndDate = formattedEndDate;
                partsDefectiveVM.PartsDefectiveAmount = periodChange.total;
                partsDefectiveVM.PeriodChange = double.IsNaN(periodChange.periodChange) || double.IsInfinity(periodChange.periodChange) ? null : periodChange.periodChange;
                partsDefectiveVM.IsChangePositive = periodChange.previousTotal >= periodChange.total ? false : true;

                return Ok(partsDefectiveVM);
            }
            catch(Exception)
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

        public List<QualityRepresentative> GetQualRepsByPart(string partNumber, List<DateOnly>timeSpan, bool previousTimeSpan)
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
            //Initialize time variables
            DateOnly startDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Now);
            DateOnly previousDate = DateOnly.FromDateTime(DateTime.Now);

            //Define period chosen
            if (period == "Monthly")
            {
                startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-1));
                previousDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-2));
            }
            else if (period == "6 Months")
            {
                startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-6));
                previousDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-12));
            }
            else if (period == "Yearly")
            {
                startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-12));
                previousDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-24));
            }
            else if (period == "3 Years")
            {
                startDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-36));
                previousDate = DateOnly.FromDateTime(DateTime.Now.AddMonths(-72));
            }

            List<DateOnly> dates = new List<DateOnly> { startDate, endDate, previousDate};
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
        public IActionResult CreateQualityRepresentative(int? id)
        {
            string[] selectedOptions = Array.Empty<string>();

            NCRNumber displayNCRNumber = new NCRNumber();

            PopulateUserEmailData(selectedOptions);

            if (id != null)
            {
                
                NCR quality = GetNCR((int)id);

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
                }

                if(quality.PrevNCRID == null)
                {
                NewNCRNumber(quality, displayNCRNumber);

                }
                PopulateList();

                return View("Create", quality);
            }

            NCR genNCR = new NCR();

            PopulateList();

            NewNCRNumber(genNCR, displayNCRNumber);

            //ViewData["data-val"] = "disabled";

            return View("Create", genNCR);

        }

        // POST: NCR/CreateQualityRepresentative
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Quality Inspector")]
        public async Task<IActionResult> CreateQualityRepresentative(NCR quality,NCR genNCR, int[] imagesToRemove, int[] linksToRemove, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft)
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
                    NCRNumber createNCRNumber = new NCRNumber();

                    PopulateUserEmailData(selectedOptions);

                    //Save draft for the first time
                    if (draft != null && quality.IsNCRDraft == false)
                    {
                        NCR ncrToUpdate = GetNCR(quality.ID);
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
                            QualityPhotos = genNCR.QualityRepresentative.QualityPhotos,
                            VideoLinks = genNCR.QualityRepresentative.VideoLinks,
                        };

                        if(ncrToUpdate != null && ncrToUpdate.PrevNCRID != null)
                        {
                            //Get ncr to update
                            
                            ncrToUpdate.IsNCRDraft = true;
                            ncrToUpdate.DraftQualityRepresentative = draftQual;

                            //Add photos and pictures
                            await AddQualityPhotos(ncrToUpdate.DraftQualityRepresentative, Pictures);
                            await AddVideoLinks(ncrToUpdate.DraftQualityRepresentative, Links);

                            await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.DraftQualityRepresentative);
                        }
                        else
                        {
                            //Set up NCR model for draft
                            NCR ncrModelDraft = new NCR
                            {
                                IsNCRDraft = true,
                                CreatedOnDO = DateOnly.FromDateTime(DateTime.Now),
                                CreatedOn = DateTime.Now,
                                Status = "Active",
                                Phase = "Quality Representative",
                                NCRNum = "0000-000",
                                DraftQualityRepresentative = draftQual
                            };

                            //Add photos and pictures
                            await AddQualityPhotos(ncrModelDraft.DraftQualityRepresentative, Pictures);
                            await AddVideoLinks(ncrModelDraft.DraftQualityRepresentative, Links);
                            _context.NCRs.Add(ncrModelDraft);
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && quality.IsNCRDraft == true)
                    {
                        //Get ncr to update
                        NCR ncrToUpdate = GetNCR(quality.ID);
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
                        }

                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    //Finally submit draft
                    else if (draft == null && quality.IsNCRDraft == true)
                    {
                        //Get NCR to Update
                        NCR ncrToUpdate = GetNCR(quality.ID);

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
                        
                        ncrToUpdate.CreatedOn = DateTime.Now;
                        ncrToUpdate.CreatedOnDO = DateOnly.FromDateTime(DateTime.Now);

                        NewNCRNumber(ncrToUpdate, createNCRNumber);
                        _context.NCRNumbers.Add(createNCRNumber);

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
                        }

                        await _context.SaveChangesAsync();

                        SendNotificationEmail(selectedOptions, Subject, emailContent);

                        return RedirectToAction("Index");
                    }

                    //If in Quality Representative Phase, fill NCR
                    if (quality.Phase == "Quality Representative")
                    {
                        NCR ncrModelFill = GetNCR(quality.ID);
                        ncrModelFill.Phase = "Engineering";
                        ncrModelFill.QualityRepresentative = quality.QualityRepresentative;

                        await AddQualityPhotos(ncrModelFill.QualityRepresentative, Pictures);
                        await AddVideoLinks(ncrModelFill.QualityRepresentative, Links);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }

                    //If engineering not required, skip it
                    NCR ncrModel = new NCR
                    {
                        PrevNCRID = genNCR.PrevNCRID,
                        Status = "Active",
                        Phase = "Engineering",
                        CreatedOnDO = DateOnly.FromDateTime(DateTime.Now),
                        CreatedOn = DateTime.Now,
                        NCRNum = genNCR.NCRNum,
                        QualityRepresentative = genNCR.QualityRepresentative
                    };
                    NCR ncrModelNoEng = new NCR
                    {
                        PrevNCRID = genNCR.PrevNCRID,
                        Status = "Active",
                        Phase = "Operations",
                        CreatedOnDO = DateOnly.FromDateTime(DateTime.Now),
                        CreatedOn = DateTime.Now,
                        NCRNum = genNCR.NCRNum,
                        QualityRepresentative = genNCR.QualityRepresentative
                    };

                    await AddQualityPhotos(genNCR.QualityRepresentative, Pictures);
                    await AddVideoLinks(genNCR.QualityRepresentative, Links);
                    NewNCRNumber(genNCR, createNCRNumber);
                    _context.NCRNumbers.Add(createNCRNumber);

                    //Decide if engineering should be included
                    if (genNCR.QualityRepresentative.ConfirmingEng == true)
                    {
                        _context.NCRs.Add(ncrModelNoEng);
                    }
                    else
                    {
                        _context.NCRs.Add(ncrModel);
                    }
                    await _context.SaveChangesAsync();

                    SendNotificationEmail(selectedOptions, Subject, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                //if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.CustomerCode"))
                //{
                //    ModelState.AddModelError("CustomerCode", "Unable to save changes. Remember, you cannot have duplicate Customer Codes.");
                //}
                //else
                //{
                //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //}
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            
            //If NCR is a draft
            if (quality.IsNCRDraft)
            {
                NCR ncrModel = GetNCR(quality.ID);

                quality.QualityRepresentative.QualityPhotos = ncrModel.DraftQualityRepresentative.QualityPhotos;
                quality.QualityRepresentative.VideoLinks = ncrModel.DraftQualityRepresentative.VideoLinks;

                PopulateUserEmailData(selectedOptions);
                PopulateList();
                return View("Create", quality);
            }
            genNCR.Phase = "Quality Representative";

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", genNCR);
        }

        // GET: NCR/CreateEngineering
        [Authorize(Roles = "Admin,Engineer")]
        public IActionResult CreateEngineering(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR eng = GetNCR(id);

            if(eng.IsNCRDraft == true)
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
        public async Task<IActionResult> CreateEngineering(NCR eng, List<IFormFile> Pictures, NCRNumber createNCRNumber, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove)
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
                            QualityPhotos = eng.Engineering.QualityPhotos,
                            VideoLinks = eng.Engineering.VideoLinks
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftEngineering = draftEng;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftEngineering, Pictures);
                        await AddVideoLinks(ncrModel.DraftEngineering, Links);

                        await _context.SaveChangesAsync();
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
                        ncrModel.DraftEngineering.QualityPhotos = eng.Engineering.QualityPhotos;
                        ncrModel.DraftEngineering.VideoLinks = eng.Engineering.VideoLinks;

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
                        }

                        await _context.SaveChangesAsync();
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
                        }

                        await _context.SaveChangesAsync();

                        SendNotificationEmail(selectedOptions, Subject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Operations";
                    ncrModel.Engineering = eng.Engineering;

                    await AddQualityPhotos(ncrModel.Engineering, Pictures);
                    await AddVideoLinks(ncrModel.Engineering, Links);
                    await _context.SaveChangesAsync();

                    SendNotificationEmail(selectedOptions, Subject, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                //if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.CustomerCode"))
                //{
                //    ModelState.AddModelError("CustomerCode", "Unable to save changes. Remember, you cannot have duplicate Customer Codes.");
                //}
                //else
                //{
                //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //}
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            eng.Phase = "Engineering";

            //If NCR is a draft
            if (eng.IsNCRDraft)
            {
                NCR ncrModel = GetNCR(eng.ID);

                eng.Engineering.QualityPhotos = ncrModel.DraftEngineering.QualityPhotos;
                eng.Engineering.VideoLinks = ncrModel.DraftEngineering.VideoLinks;

                PopulateUserEmailData(selectedOptions);
                PopulateList();
                return View("Create", eng);
            }

            PopulateUserEmailData(selectedOptions);
            PopulateList();
            return View("Create", eng);
        }

        // GET: NCR/CreateOperations
        [Authorize(Roles = "Admin,Operations Manager")]
        public IActionResult CreateOperations(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR oper = GetNCR(id);

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
        public async Task<IActionResult> CreateOperations(NCR oper, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove)
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
                            QualityPhotos = oper.Operations.QualityPhotos,
                            VideoLinks = oper.Operations.VideoLinks
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftOperations = draftOpr;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftOperations, Pictures);
                        await AddVideoLinks(ncrModel.DraftOperations, Links);

                        await _context.SaveChangesAsync();
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
                        ncrModel.DraftOperations.QualityPhotos = oper.Operations.QualityPhotos;
                        ncrModel.DraftOperations.VideoLinks = oper.Operations.VideoLinks;

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
                        }

                        await _context.SaveChangesAsync();
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
                        }

                        await _context.SaveChangesAsync();

                        SendNotificationEmail(selectedOptions, Subject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Procurement";
                    ncrModel.Operations = oper.Operations;

                    await AddQualityPhotos(ncrModel.Operations, Pictures);
                    await AddVideoLinks(ncrModel.Operations, Links);
                    await _context.SaveChangesAsync();

                    SendNotificationEmail(selectedOptions, Subject, emailContent);

                    return RedirectToAction("Index");
                }
            }
            catch (DbUpdateException)
            {
                //if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.CustomerCode"))
                //{
                //    ModelState.AddModelError("CustomerCode", "Unable to save changes. Remember, you cannot have duplicate Customer Codes.");
                //}
                //else
                //{
                //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //}
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            //If NCR is a draft
            if (oper.IsNCRDraft)
            {
                NCR ncrModel = GetNCR(oper.ID);

                oper.Operations.QualityPhotos = ncrModel.DraftOperations.QualityPhotos;
                oper.Operations.VideoLinks = ncrModel.DraftOperations.VideoLinks;

                PopulateUserEmailData(selectedOptions);
                PopulateList();
                return View("Create", oper);
            }
            oper.Phase = "Operations";

            PopulateUserEmailData(selectedOptions);
            PopulateList();

            return View("Create", oper);
        }

        // GET: NCR/CreateProcurement
        [Authorize(Roles = "Admin,Procurement")]
        public IActionResult CreateProcurement(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR proc = GetNCR(id);

            if (proc.IsNCRDraft == true)
            {
                proc.Procurement = new Procurement();
                proc.Procurement.SuppItemsBack = proc.DraftProcurement.SuppItemsBack;
                proc.Procurement.RMANo = proc.DraftProcurement.RMANo;
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
        public async Task<IActionResult> CreateProcurement(NCR proc, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove)
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
                    NCR ncrModel = GetNCR(proc.ID);

                    //Save draft for the first time
                    if (draft != null && proc.IsNCRDraft == false)
                    {
                        //Create draft model and pass input data to it
                        DraftProcurement draftProc = new DraftProcurement
                        {
                            SuppItemsBack = proc.Procurement.SuppItemsBack,
                            RMANo = proc.Procurement.RMANo,
                            CarrierInfo = proc.Procurement.CarrierInfo,
                            ExpecDateOfReturn = proc.Procurement.ExpecDateOfReturn,
                            SuppReturnCompleted = proc.Procurement.SuppReturnCompleted,
                            IsCreditExpec = proc.Procurement.IsCreditExpec,
                            ChargeSupplier = proc.Procurement.ChargeSupplier,
                            ProcurementDate = proc.Procurement.ProcurementDate,
                            ProcurementSign = proc.Procurement.ProcurementSign,
                            QualityPhotos = proc.Procurement.QualityPhotos,
                            VideoLinks = proc.Procurement.VideoLinks,

                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftProcurement = draftProc;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftProcurement, Pictures);
                        await AddVideoLinks(ncrModel.DraftProcurement, Links);

                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    //Save draft for the second time and beyond
                    else if (draft != null && proc.IsNCRDraft == true)
                    {
                        //Pass data to the draft properties
                        ncrModel.DraftProcurement.SuppItemsBack = proc.Procurement.SuppItemsBack;
                        ncrModel.DraftProcurement.RMANo = proc.Procurement.RMANo;
                        ncrModel.DraftProcurement.CarrierInfo = proc.Procurement.CarrierInfo;
                        ncrModel.DraftProcurement.ExpecDateOfReturn = proc.Procurement.ExpecDateOfReturn;
                        ncrModel.DraftProcurement.SuppReturnCompleted = proc.Procurement.SuppReturnCompleted;
                        ncrModel.DraftProcurement.IsCreditExpec = proc.Procurement.IsCreditExpec;
                        ncrModel.DraftProcurement.ChargeSupplier = proc.Procurement.ChargeSupplier;
                        ncrModel.DraftProcurement.ProcurementDate = proc.Procurement.ProcurementDate;
                        ncrModel.DraftProcurement.ProcurementSign = proc.Procurement.ProcurementSign;
                        ncrModel.DraftProcurement.QualityPhotos = proc.Procurement.QualityPhotos;
                        ncrModel.DraftProcurement.VideoLinks = proc.Procurement.VideoLinks;

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
                        }

                        await _context.SaveChangesAsync();
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
                        }

                        await _context.SaveChangesAsync();

                        SendNotificationEmail(selectedOptions, Subject, emailContent);

                        return RedirectToAction("Index");
                    }

                    ncrModel.Phase = "Reinspection";
                    ncrModel.Procurement = proc.Procurement;

                    await AddQualityPhotos(ncrModel.Procurement, Pictures);
                    await AddVideoLinks(ncrModel.Procurement, Links);
                    await _context.SaveChangesAsync();

                    SendNotificationEmail(selectedOptions, Subject, emailContent);
                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            //If NCR is a draft
            if (proc.IsNCRDraft)
            {
                NCR ncrModel = GetNCR(proc.ID);

                proc.Procurement.QualityPhotos = ncrModel.DraftProcurement.QualityPhotos;
                proc.Procurement.VideoLinks = ncrModel.DraftProcurement.VideoLinks;

                PopulateUserEmailData(selectedOptions);
                PopulateList();
                return View("Create", proc);
            }
            proc.Phase = "Procurement";

            PopulateUserEmailData(selectedOptions);
            PopulateList();

            return View("Create", proc);
        }

        // GET: NCR/CreateReinspection
        [Authorize(Roles = "Admin,Quality Inspector")]
        public IActionResult CreateReinspection(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR reinspec = GetNCR(id);

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
        public async Task<IActionResult> CreateReinspection(NCR reinspec, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft, int[] imagesToRemove, int[] linksToRemove)
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

                    NCR ncrModel = GetNCR(reinspec.ID);

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
                            QualityPhotos = reinspec.Reinspection.QualityPhotos,
                            VideoLinks = reinspec.Reinspection.VideoLinks,
                        };

                        //Set up NCR model for draft
                        ncrModel.IsNCRDraft = true;
                        ncrModel.DraftReinspection = draftReinspec;

                        //Add photos and pictures
                        await AddQualityPhotos(ncrModel.DraftReinspection, Pictures);
                        await AddVideoLinks(ncrModel.DraftReinspection, Links);

                        await _context.SaveChangesAsync();
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
                        ncrModel.DraftReinspection.QualityPhotos = reinspec.Reinspection.QualityPhotos;
                        ncrModel.DraftReinspection.VideoLinks = reinspec.Reinspection.VideoLinks;

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
                        }

                        await _context.SaveChangesAsync();
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

                        await _context.SaveChangesAsync();
                        SendNotificationEmail(selectedOptions, Subject, emailContent);

                        //In case reinspection failed repeat NCR process
                        if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                        {

                            NCR repeatNCR = new NCR
                            {
                                PrevNCRID = ncrModel.ID,
                                Status = "Active",
                                Phase = "Quality Representative",
                                CreatedOnDO = DateOnly.FromDateTime(DateTime.Now),
                                CreatedOn = DateTime.Now
                            };

                            NCRNumber createNCRNumber = new NCRNumber();
                            NewNCRNumber(repeatNCR, createNCRNumber);
                            _context.NCRNumbers.Add(createNCRNumber);
                            _context.NCRs.Add(repeatNCR);
                            await _context.SaveChangesAsync();

                            if (await TryUpdateModelAsync<NCR>(ncrModel, ""))
                            {
                                ncrModel.IsNCRDraft = false;
                                ncrModel.NewNCRID = repeatNCR.ID;
                                await _context.SaveChangesAsync();
                            }

                            return RedirectToAction("CreateQualityRepresentative", new { id = repeatNCR.ID });
                        }

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
                    await _context.SaveChangesAsync();

                    SendNotificationEmail(selectedOptions, Subject, emailContent);

                    //In case reinpection failed repeat NCR process
                    if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                    {
                        NCR repeatNCR = new NCR
                        {
                            PrevNCRID = ncrModel.ID,
                            Status = "Active",
                            Phase = "Quality Representative",
                            CreatedOnDO = DateOnly.FromDateTime(DateTime.Now),
                            CreatedOn = DateTime.Now
                        };

                        NCRNumber createNCRNumber = new NCRNumber();
                        NewNCRNumber(repeatNCR, createNCRNumber);
                        _context.NCRNumbers.Add(createNCRNumber);
                        _context.NCRs.Add(repeatNCR);
                        await _context.SaveChangesAsync();

                        if (await TryUpdateModelAsync<NCR>(ncrModel, ""))
                        {
                            ncrModel.NewNCRID = repeatNCR.ID;

                            await _context.SaveChangesAsync();
                        }

                        return RedirectToAction("CreateQualityRepresentative", new { id = repeatNCR.ID });
                    }

                    return RedirectToAction("Index");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
            //If NCR is a draft
            if (reinspec.IsNCRDraft)
            {
                NCR ncrModel = GetNCR(reinspec.ID);

                reinspec.Reinspection.QualityPhotos = ncrModel.DraftReinspection.QualityPhotos;
                reinspec.Reinspection.VideoLinks = ncrModel.DraftReinspection.VideoLinks;

                PopulateUserEmailData(selectedOptions);
                PopulateList();
                return View("Create", reinspec);
            }
            reinspec.Phase = "Reinspection";

            PopulateUserEmailData(selectedOptions);
            PopulateList();

            return View("Create", reinspec);
        }

        // GET: Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
                return NotFound();
            }

            PopulateList();

            return View(ncr);
        }

        // POST: Customers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<IFormFile> Pictures, string[] selectedOptions, int[] imagesToRemove, int[] linksToRemove, string Subject, string emailContent, string[] Links,string sectionEdited)
        {
            PopulateUserEmailData(selectedOptions);
            SendNotificationEmail(selectedOptions, Subject, emailContent);

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

            //Check that we got the customer or exit with a not found error
            if (ncrToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.QualityRepresentative, ncr => ncr.Engineering, ncr => ncr.Operations, ncr => ncr.Procurement, ncr => ncr.Reinspection))
            {
                try
                {
                    if (imagesToRemove != null)
                    {
                        foreach(var image in imagesToRemove)
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
                    await _context.SaveChangesAsync();

                    //Send on to details with the section edited open
                    ViewData["myData"] = "Engineering";
                    return RedirectToAction("Details", new { id = ncrToUpdate.ID});
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NCRExists(ncrToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException dex)
                {
                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed: Customers.CustomerCode"))
                    {
                        ModelState.AddModelError("CustomerCode", "Unable to save changes. Remember, you cannot have duplicate Customer Codes.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }
            }
            return View(ncrToUpdate);
        }

        private SelectList SupplierList(int? selectedId)
        {
            return new SelectList(_context
                .Suppliers
                .OrderBy(m => m.SupplierName), "ID", "SupplierName", selectedId);
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
        }

        //Helper method to create new NCR number
        private async void NewNCRNumber(NCR ncr, NCRNumber createNCRNumber)
        {
            //Get last NCR number
            NCRNumber lastNCRNum = await _context.NCRNumbers
                .OrderByDescending(n => n.ID)
                .FirstOrDefaultAsync();

            //Create new NCR number
            createNCRNumber.Year = DateTime.Now.Date.Year;

            if (lastNCRNum == null)
            {
                ncr.NCRNum = createNCRNumber.GenerateNCRNumber(false, 0);
            }
            else if (lastNCRNum != null)
            {
                if (lastNCRNum.Year != DateTime.Now.Date.Year)
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
                else
                {
                    ViewData["Message"] = "Sorry but you can only send an email to yourself in this demo and you were not selected.";
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
                    else if(c.Active == true)
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

            foreach (var ncr in NCRsToArchive)
            {
                if (await TryUpdateModelAsync<NCR>(ncr, ""))
                {
                    ncr.IsNCRArchived = true;

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Archiving");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ArchiveNCR(int id)
        {
            NCR ncrToArchive = _context.NCRs.FirstOrDefault(n => n.ID == id);

            if (await TryUpdateModelAsync<NCR>(ncrToArchive, ""))
            { 
                ncrToArchive.IsNCRArchived = true;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> CancelDraft(int id)
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

            try
            {
                if(ncrDraft.IsNCRDraft == true && ncrDraft != null)
                {
                    //If phase is Quality Representative, delete the NCR
                    if (ncrDraft.Phase == "Quality Representative")
                    {

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

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");

                }
            }
            catch (DbUpdateException)
            {
                //if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                //{
                //    ModelState.AddModelError("", "Unable to Delete Customer. Remember, you cannot delete a Customer that has a function in the system.");
                //}
                //else
                //{
                //    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                //}
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> VoidNCR(int id, string voidingReason)
        {
            var ncrToVoid = _context.NCRs.FirstOrDefault(ncr => ncr.ID == id);

            if (ncrToVoid.Status == "Voided")
            {
                if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
                {
                    ncrToVoid.VoidingReason = null;
                    ncrToVoid.Status = "Active";

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
            }


            if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
            {
                ncrToVoid.VoidingReason = voidingReason;
                ncrToVoid.Status = "Voided";

                await _context.SaveChangesAsync();
            }

            return Ok(ncrToVoid);
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

        //Method to add Photos
        public static async Task AddQualityPhotos<T>(T module, List<IFormFile> photos) where T : class
        {
            var qualityPhotosProperty = module.GetType().GetProperty("QualityPhotos");
            if (qualityPhotosProperty != null && qualityPhotosProperty.PropertyType == typeof(List<QualityPhoto>))
            {
                var qualityPhotos = qualityPhotosProperty.GetValue(module) as List<QualityPhoto>;
                if (qualityPhotos == null)
                {
                    qualityPhotos = new List<QualityPhoto>();
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
                                var photoArray = memoryStream.ToArray();

                                // Create a new QualityPhoto instance for each image
                                var qualityPhoto = new QualityPhoto
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

                // Set the updated QualityPhotos list back to the module
                qualityPhotosProperty.SetValue(module, qualityPhotos);
            }
        }

        // Helper method to retrieve the next NCR
        private NCR GetNCR(int id)
        {

            NCR ncrModel = _context.NCRs
                .Include(n => n.QualityRepresentative)
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

            //Process.Start("explorer.exe", "NCR.pdf");
            //Redirect("Index");
        }

        private bool NCRExists(int id)
        {
            return _context.NCRs.Any(e => e.ID == id);
        }
    }
}