using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Haver.Data;
using Haver.Models;
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

namespace Haver.Controllers
{
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
            , string actionButton, string active, string sortDirection = "desc", string sortField = "NCR NO.")
        {
            PopulateList();

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Purchasing)
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
                        .OrderBy(p => p.CreatedOn);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn);
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
        public async Task<IActionResult> Archived(string SearchString, int? SupplierID, int? page, int? pageSizeID, string SelectedOption
            , string actionButton, string active, string sortDirection = "desc", string sortField = "NCR NO.", DateOnly? StartDate = null, DateOnly? EndDate = null)
        {
            PopulateList();

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Purchasing)
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
                        .OrderBy(p => p.CreatedOn);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn);
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

        // GET: NCR/Archiving
        public async Task<IActionResult> Archiving(string SearchString, int? SupplierID, int? page, int? pageSizeID, string SelectedOption
            ,string actionButton, string active, string sortDirection = "desc", string sortField = "NCR NO.", DateOnly? StartDate = null, DateOnly? EndDate = null)
        {
            PopulateList();

            string[] sortOptions = new[] { "NCR NO.", "CREATED ON" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Purchasing)
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
                        .OrderBy(p => p.CreatedOn);
                }
                else
                {
                    haverContext = haverContext
                        .OrderByDescending(p => p.CreatedOn);
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
                .Include(n => n.Purchasing)
                    .Include(p => p.Purchasing.QualityPhotos)
                    .Include(p => p.Purchasing.PrelDecision)
                    .Include(p => p.Purchasing.VideoLinks)
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
                .FirstOrDefaultAsync(m => m.ID == id);

            if (nCR == null)
            {
                return NotFound();
            }

            return View(nCR);
        }

        // GET: NCR/Analytics
        public async Task<IActionResult> Analytics(int? page, string actionButton, string sortDirection = "desc", string sortField = "NCRNumber")
        {
            PopulateList();

            string[] sortOptions = new[] { "NCRNumber" };

            var haverContext = _context.NCRs.Include(n => n.Engineering)
                                            .Include(n => n.Purchasing)
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

        public List<object> GenChart()
        {
            List<object> data = new List<object>();

            var partUsed = _context.Parts.FirstOrDefault(p => p.ID == 1);
            List<int> total = new List<int>();
            List<string> months = new List<string>();

            var NCRsLinkedToPart = _context.QualityRepresentatives.Where(n => n.PartID == partUsed.ID);

            foreach (var ncr in NCRsLinkedToPart)
            {
                total.Add(ncr.QuantDefective);
                months.Add(ncr.QualityRepDate.Month.ToString());
            }

            data.Add(months);
            data.Add(total);
            return data;

        } 

        // GET: NCR/CreateQualityRepresentative
        public IActionResult CreateQualityRepresentative(int? id)
        {
            var allOptions = _context.Employees
                .Where(d => d.Email != null)
                .OrderBy(d => d.LastName)
                .ThenBy(d => d.FirstName);

            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);
            PopulateList();

            if (id != null)
            {
                NCR quality = MergeNCRData((int)id);

                return View("Create", quality);
            }

            NCR genNCR = new NCR
            {
                QualityRepresentative = new QualityRepresentative
                {
                    NonConforming = true
                },
            };

            NCRNumber createNCRNumber = new NCRNumber();

            PopulateList();

            NewNCRNumber(genNCR, createNCRNumber);
            return View("Create", genNCR);

        }

        // POST: NCR/CreateQualityRepresentative
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQualityRepresentative(NCR quality,NCR genNCR, NCRNumber createNCRNumber, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links, string draft)
        {
            //Temporarily bypass the required attributes in order to save a draft
            if (draft != null)
            {
                ModelState.Clear();
            }

            if (ModelState.IsValid)
            {
                PopulateUserEmailData(selectedOptions);

                //Save draft for the first time
                if (draft != null && quality.IsNCRDraft == false)
                {
                    NCR ncrModelDraft = new NCR
                    {
                        PrevNCRID = genNCR.PrevNCRID,
                        IsNCRDraft = true,
                        Status = "Active",
                        Phase = "Quality Representative",
                        NCRNum = genNCR.NCRNum,
                        QualityRepresentative = genNCR.QualityRepresentative
                    };

                    List<string> requiredProperties = typeof(QualityRepresentative)
                        .GetProperties()
                        .Where(p => p.GetCustomAttribute<RequiredAttribute>() != null)
                        .Select(p => p.Name)
                        .ToList();

                    AssignDefaultValues(ncrModelDraft.QualityRepresentative, "...", requiredProperties);

                    await AddQualityPhotos(ncrModelDraft.QualityRepresentative, Pictures);
                    await AddVideoLinks(ncrModelDraft.QualityRepresentative, Links);
                    NewNCRNumber(ncrModelDraft, createNCRNumber);

                    _context.NCRNumbers.Add(createNCRNumber);
                    _context.NCRs.Add(ncrModelDraft);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                //Save draft for the second time and beyond
                else if (draft != null && quality.IsNCRDraft == true)
                {
                    NCR ncrToUpdate = GetNCR(quality.ID);
                    if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.QualityRepresentative))
                    {
                        await AddQualityPhotos(ncrToUpdate.QualityRepresentative, Pictures);
                        await AddVideoLinks(ncrToUpdate.QualityRepresentative, Links);
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                //Finally submit draft
                else if (draft == null && quality.IsNCRDraft == true)
                {
                    NCR ncrToUpdate = GetNCR(quality.ID);
                    if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.QualityRepresentative))
                    {
                        ncrToUpdate.IsNCRDraft = false;
                        ncrToUpdate.Phase = "Engineering";

                        await AddQualityPhotos(ncrToUpdate.QualityRepresentative, Pictures);
                        await AddVideoLinks(ncrToUpdate.QualityRepresentative, Links);
                    }

                    SendNotificationEmail(selectedOptions, Subject, emailContent);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }

                SendNotificationEmail(selectedOptions, Subject, emailContent);
                
                if(quality.Phase == "Quality Representative")
                {
                    NCR ncrModelFill = GetNCR(quality.ID);
                    ncrModelFill.Phase = "Engineering";
                    ncrModelFill.QualityRepresentative = quality.QualityRepresentative;

                    await AddQualityPhotos(ncrModelFill.QualityRepresentative, Pictures);
                    await AddVideoLinks(ncrModelFill.QualityRepresentative, Links);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                
                NCR ncrModel = new NCR
                {
                    PrevNCRID = genNCR.PrevNCRID,
                    Status = "Active",
                    Phase = "Engineering",
                    CreatedOn = DateOnly.FromDateTime(DateTime.Now),
                    NCRNum = genNCR.NCRNum,
                    QualityRepresentative = genNCR.QualityRepresentative
                };
                NCR ncrModelNoEng = new NCR
                {
                    PrevNCRID = genNCR.PrevNCRID,
                    Status = "Active",
                    Phase = "Operations",
                    CreatedOn = DateOnly.FromDateTime(DateTime.Now),
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


                return RedirectToAction("Index");
            }
            PopulateList();
            return View("Create", genNCR);
        }

        // GET: NCR/CreateEngineering
        public IActionResult CreateEngineering(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR eng = MergeNCRData(id);

            PopulateList();

            return View("Create", eng);
        }

        // POST: NCR/CreateEngineering
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEngineering(NCR eng, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links)
        {
            if (ModelState.IsValid)
            {
                PopulateUserEmailData(selectedOptions);
                SendNotificationEmail(selectedOptions, Subject, emailContent);

                NCR ncrModel = GetNCR(eng.ID);
                ncrModel.Phase = "Operations";
                ncrModel.Engineering = eng.Engineering;

                await AddQualityPhotos(ncrModel.Engineering, Pictures);
                await AddVideoLinks(ncrModel.Engineering, Links);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            PopulateList();
            return View("Create", eng);
        }

        // GET: NCR/CreatePurchasing
        public IActionResult CreatePurchasing(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR purch = MergeNCRData(id);

            PopulateList();

            return View("Create", purch);
        }

        // POST: NCR/CreatePurchasing
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePurchasing(NCR purch, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links)
        {
            if (ModelState.IsValid)
            {
                PopulateUserEmailData(selectedOptions);
                SendNotificationEmail(selectedOptions, Subject, emailContent);

                NCR ncrModel = GetNCR(purch.ID);
                ncrModel.Phase = "Procurement";
                ncrModel.Purchasing = purch.Purchasing;

                await AddQualityPhotos(ncrModel.Purchasing, Pictures);
                await AddVideoLinks(ncrModel.Purchasing, Links);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            PopulateList();

            return View("Create", purch);
        }

        // GET: NCR/CreateProcurement
        public IActionResult CreateProcurement(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR proc = MergeNCRData(id);

            PopulateList();

            return View("Create", proc);
        }

        // POST: NCR/CreateProcurement
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProcurement(NCR proc, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links)
        {
            if (ModelState.IsValid)
            {
                PopulateUserEmailData(selectedOptions);
                SendNotificationEmail(selectedOptions, Subject, emailContent);

                NCR ncrModel = GetNCR(proc.ID);
                ncrModel.Phase = "Reinspection";
                ncrModel.Procurement = proc.Procurement;

                await AddQualityPhotos(ncrModel.Procurement, Pictures);
                await AddVideoLinks(ncrModel.Procurement, Links);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            PopulateList();

            return View("Create", proc);
        }

        // GET: NCR/CreateReinspection
        public IActionResult CreateReinspection(int id)
        {
            string[] selectedOptions = Array.Empty<string>();

            PopulateUserEmailData(selectedOptions);

            NCR reinspec = MergeNCRData(id);

            PopulateList();

            return View("Create", reinspec);
        }

        // POST: NCR/CreateReinspection
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReinspection(NCR reinspec, List<IFormFile> Pictures, string[] selectedOptions, string Subject, string emailContent, string[] Links)
        {
            if (ModelState.IsValid)
            {
                PopulateUserEmailData(selectedOptions);
                SendNotificationEmail(selectedOptions, Subject, emailContent);

                NCR ncrModel = GetNCR(reinspec.ID);

                ncrModel.Phase = "Complete";

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

                ncrModel.Reinspection = reinspec.Reinspection;

                await AddQualityPhotos(ncrModel.Reinspection, Pictures);
                await AddVideoLinks(ncrModel.Reinspection, Links);
                await _context.SaveChangesAsync();

                //In case reinpection failed repeat NCR process
                if (reinspec.Reinspection.ReinspecAccepted == false && ncrModel.PrevNCRID == null)
                {

                    NCR repeatNCR = new NCR
                    {
                        PrevNCRID = ncrModel.ID,
                        Status = "Active",
                        Phase = "Quality Representative",
                        CreatedOn = DateOnly.FromDateTime(DateTime.Now)
                    };

                    NCRNumber createNCRNumber = new NCRNumber();
                    NewNCRNumber(repeatNCR, createNCRNumber);

                    _context.NCRNumbers.Add(createNCRNumber);
                    _context.NCRs.Add(repeatNCR);
                    await _context.SaveChangesAsync();

                    //Get the prev NCR
                    var ncrToUpdate = await _context.NCRs
                        .FirstOrDefaultAsync(p => p.ID == ncrModel.ID);

                    if(await TryUpdateModelAsync<NCR>(ncrToUpdate, ""))
                    {
                        ncrToUpdate.NewNCRID = repeatNCR.ID;

                        await _context.SaveChangesAsync();
                    }


                    return RedirectToAction("CreateQualityRepresentative", new { id = repeatNCR.ID });
                }

                return RedirectToAction("Index");
            }

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
                .Include(n => n.Purchasing)
                    .Include(p => p.Purchasing.QualityPhotos)
                    .Include(p => p.Purchasing.PrelDecision)
                    .Include(p => p.Purchasing.VideoLinks)
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
                .Include(n => n.Purchasing)
                    .Include(p => p.Purchasing.QualityPhotos)
                    .Include(p => p.Purchasing.PrelDecision)
                    .Include(p => p.Purchasing.VideoLinks)
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
            if (await TryUpdateModelAsync<NCR>(ncrToUpdate, "", ncr => ncr.QualityRepresentative, ncr => ncr.Engineering, ncr => ncr.Purchasing, ncr => ncr.Procurement, ncr => ncr.Reinspection))
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
                        case "Purchasing":
                            await AddQualityPhotos(ncrToUpdate.Purchasing, Pictures);
                            await AddVideoLinks(ncrToUpdate.Purchasing, Links);
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

                    //Send on to Functions
                    return RedirectToAction("Details", new { id = ncrToUpdate.ID });
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

        private void PopulateList()
        {
            QualityRepresentative qualityRepresentative = null;
            Engineering engineering = null;
            Purchasing purchasing = null;

            ViewData["SupplierID"] = SupplierList(qualityRepresentative?.SupplierID);
            ViewData["ProblemID"] = ProblemList(qualityRepresentative?.ProblemID);
            ViewData["ProcessApplicableID"] = ProcessApplicableList(qualityRepresentative?.ProcessApplicableID);
            ViewData["PartID"] = PartList(qualityRepresentative?.PartID);
            ViewData["EngReviewID"] = EngReviewList(engineering?.EngReviewID);
            ViewData["PrelDecisionID"] = PrelDecisionList(purchasing?.PrelDecisionID);
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

        public static void AssignDefaultValues(object obj, string defaultValue, List<string> requiredProperties)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType == typeof(string) && requiredProperties.Contains(property.Name))
                {
                    string value = (string)property.GetValue(obj);
                    if (string.IsNullOrEmpty(value))
                    {
                        property.SetValue(obj, defaultValue);
                    }
                }
            }
        }

        //Question to Prof, is this good for optmization?
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

        public async Task<IActionResult> ArchiveNCR(int id)
        {
            var ncrToArchive = _context.NCRs.FirstOrDefault(ncr => ncr.ID == id);

            if (await TryUpdateModelAsync<NCR>(ncrToArchive, ""))
            { 
                ncrToArchive.IsNCRArchived = true;

                await _context.SaveChangesAsync();
            }


            return RedirectToAction("Index");
        }

        public async Task<IActionResult> VoidNCR(int id)
        {
            var ncrToVoid = _context.NCRs.FirstOrDefault(ncr => ncr.ID == id);

            if (ncrToVoid.Status == "Voided")
            {
                if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
                {
                    ncrToVoid.Status = "Active";

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
            }


            if (await TryUpdateModelAsync<NCR>(ncrToVoid, ""))
            {
                ncrToVoid.Status = "Voided";

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
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


        //Helper method to retrieve data from previous section/phase and merge it with current section's phase
        private NCR MergeNCRData(int id)
        {
            NCR existingData = GetNCR(id);

            // Convert the view model to the database model
            NCR ncrModel = new NCR
            {
                Status = existingData.Status,
                IsNCRDraft = existingData.IsNCRDraft,
                Phase = existingData.Phase,
                CreatedOn = existingData.CreatedOn,
                UserRole = existingData.UserRole,
                NCRNum = existingData.NCRNum,
                QualityRepresentative = existingData.QualityRepresentative,
                Engineering = existingData.Engineering,
                Purchasing = existingData.Purchasing,
                Procurement = existingData.Procurement,
                Reinspection = existingData.Reinspection
            };

            return ncrModel;
        }

        // Helper method to retrieve the next NCR
        private NCR GetNCR(int id)
        {
            // Retrieve the next NCR with the specified phase from the database

            NCR ncrModel = _context.NCRs
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.VideoLinks)
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.VideoLinks)
                .Include(n => n.Purchasing)
                    .Include(p => p.Purchasing.QualityPhotos)
                    .Include(p => p.Purchasing.VideoLinks)
                .Include(n => n.Procurement)
                    .Include(pr => pr.Procurement.QualityPhotos)
                    .Include(pr => pr.Procurement.VideoLinks)
                .Include(n => n.Reinspection)
                    .Include(r => r.Reinspection.QualityPhotos)
                    .Include(r => r.Reinspection.VideoLinks)
                .FirstOrDefault(n => n.ID == id);

            if (ncrModel != null)
            {
                return ncrModel;
            }

            return null;
        }

        public async Task<IActionResult> DownloadPDFAsync(int id)
        {
            var ncrDetails = await _context.NCRs
                .Include(n => n.Engineering)
                    .Include(e => e.Engineering.QualityPhotos)
                    .Include(e => e.Engineering.EngReview)
                .Include(n => n.Purchasing)
                    .Include(p => p.Purchasing.QualityPhotos)
                    .Include(p => p.Purchasing.PrelDecision)
                .Include(n => n.QualityRepresentative)
                    .Include(qr => qr.QualityRepresentative.ProcessApplicable)
                    .Include(qr => qr.QualityRepresentative.Problem)
                    .Include(qr => qr.QualityRepresentative.Supplier)
                    .Include(qr => qr.QualityRepresentative.QualityPhotos)
                    .Include(qr => qr.QualityRepresentative.Part)
                .Include(n => n.Procurement).ThenInclude(e => e.QualityPhotos)
                .Include(n => n.Reinspection).ThenInclude(e => e.QualityPhotos)
                .FirstOrDefaultAsync(m => m.ID == id);

            if (ncrDetails == null)
            {
                return NotFound();
            }

            var document = new Document();
            using (var memoryStream = new MemoryStream())
            {
                PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                Font headingFont = FontFactory.GetFont(FontFactory.HELVETICA, 24, Font.BOLD);
                Paragraph heading;
                Font fieldFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                heading = new Paragraph("Quality Representative", headingFont);
                heading.Alignment = Element.ALIGN_CENTER;
                document.Add(heading);

                document.Add(Chunk.NEWLINE);
                document.Add(Chunk.NEWLINE);

                document.Add(new Paragraph($"Identify Process Applicable: {ncrDetails.QualityRepresentative.ProcessApplicable.ProcessName}", fieldFont));

                document.Add(new Paragraph($"Supplier: {ncrDetails.QualityRepresentative.Supplier.SupplierName}", fieldFont));

                document.Add(new Paragraph($"NCR Number: {ncrDetails.NCRNum}", fieldFont));

                document.Add(new Paragraph($"Part: {ncrDetails.QualityRepresentative.Part.PartSummary}", fieldFont));

                document.Add(new Paragraph($"Quantity Received: {ncrDetails.QualityRepresentative.QuantReceived}", fieldFont));

                document.Add(new Paragraph($"PO or Prod. No.: {ncrDetails.QualityRepresentative.PoNo}", fieldFont));

                document.Add(new Paragraph($"Problem: {ncrDetails.QualityRepresentative.Problem.ProblemDescription}", fieldFont));

                document.Add(new Paragraph($"Quantity Defective: {ncrDetails.QualityRepresentative.QuantDefective}", fieldFont));

                document.Add(new Paragraph($"Sales Order No.: {ncrDetails.QualityRepresentative.SalesOrd}", fieldFont));

                document.Add(new Paragraph($"Quality Representative's Name: {ncrDetails.QualityRepresentative.QualityRepresentativeSign}", fieldFont));

                document.Add(new Paragraph($"Date: {ncrDetails.QualityRepresentative.QualityRepDate}", fieldFont));

                document.Add(new Paragraph($"Description of Defect: {ncrDetails.QualityRepresentative.DescDefect}", fieldFont));


                if (ncrDetails.QualityRepresentative.QualityPhotos != null && ncrDetails.QualityRepresentative.QualityPhotos.Count > 0)
                {
                    foreach (var image in ncrDetails.QualityRepresentative.QualityPhotos)
                    {
                        var imageBytes = image.Content;
                        var img = iTextSharp.text.Image.GetInstance(imageBytes);
                        img.ScaleToFit(300f, 300f);
                        document.Add(img);
                    }
                }

                document.NewPage();


                if (ncrDetails.Engineering != null)
                {
                    heading = new Paragraph("Engineering", headingFont);
                    heading.Alignment = Element.ALIGN_CENTER;
                    document.Add(heading);

                    document.Add(new Paragraph($"Engineering Review: {ncrDetails.Engineering.EngReview.Review}", fieldFont));

                    document.Add(new Paragraph($"Custom Notification Necessary: {ncrDetails.Engineering.IsCustNotificationNecessary}", fieldFont));

                    document.Add(new Paragraph($"Drawing Requirement Updating: {ncrDetails.Engineering.DrawReqUpdating}", fieldFont));

                    document.Add(new Paragraph($"Engineer's Signature: {ncrDetails.Engineering.EngineerSign}", fieldFont));

                    document.Add(new Paragraph($"Engineering Date: {ncrDetails.Engineering.EngineeringDate}", fieldFont));

                    document.Add(new Paragraph("Drawing", headingFont));


                    document.Add(new Paragraph($"Original Revision Number: {ncrDetails.Engineering.OrgRevisionNum}", fieldFont));

                    document.Add(new Paragraph($"Updated Revision Number: {ncrDetails.Engineering.UpdatedRevisionNum}", fieldFont));

                    document.Add(new Paragraph($"Revised By: {ncrDetails.Engineering.RevisionedBy}", fieldFont));

                    document.Add(new Paragraph($"Revision Date: {ncrDetails.Engineering.RevisionDate}", fieldFont));

                    document.Add(new Paragraph("Report Notes", headingFont));

                    document.Add(new Paragraph($"Disposition: {ncrDetails.Engineering.Disposition}", fieldFont));

                    document.Add(new Paragraph($"Customer Issue Message: {ncrDetails.Engineering.CustIssueMsg}", fieldFont));

                    if (ncrDetails.Engineering.QualityPhotos != null && ncrDetails.Engineering.QualityPhotos.Count > 0)
                    {
                        foreach (var image in ncrDetails.Engineering.QualityPhotos)
                        {
                            var imageBytes = image.Content;
                            var img = iTextSharp.text.Image.GetInstance(imageBytes);
                            img.ScaleToFit(300f, 300f);
                            document.Add(img);
                        }
                    }

                    document.NewPage();
                }


                if (ncrDetails.Purchasing != null)
                {
                    heading = new Paragraph("Purchasing", headingFont);
                    heading.Alignment = Element.ALIGN_CENTER;
                    document.Add(heading);

                    document.Add(new Paragraph($"Preliminary Decision ID: {ncrDetails.Purchasing.PrelDecisionID}", fieldFont));

                    document.Add(new Paragraph($"CAR Raised: {ncrDetails.Purchasing.CarRaised}", fieldFont));

                    document.Add(new Paragraph($"Follow-Up Required: {ncrDetails.Purchasing.IsFollowUpReq}", fieldFont));

                    document.Add(new Paragraph($"CAR Number: {ncrDetails.Purchasing.CarNum}", fieldFont));

                    document.Add(new Paragraph($"Expected Date: {ncrDetails.Purchasing.ExpecDate}", fieldFont));

                    document.Add(new Paragraph($"Follow-Up Type: {ncrDetails.Purchasing.FollowUpType}", fieldFont));

                    document.Add(new Paragraph($"Operation Manager's Signature: {ncrDetails.Purchasing.OpManagerSign}", fieldFont));

                    document.Add(new Paragraph($"Purchasing Date: {ncrDetails.Purchasing.PurchasingDate}", fieldFont));

                    document.Add(new Paragraph("Report Notes", headingFont));

                    document.Add(new Paragraph($"Purchasing Message: {ncrDetails.Purchasing.Message}", fieldFont));

                    if (ncrDetails.Purchasing.QualityPhotos != null && ncrDetails.Purchasing.QualityPhotos.Count > 0)
                    {
                        foreach (var image in ncrDetails.Purchasing.QualityPhotos)
                        {
                            var imageBytes = image.Content;
                            var img = iTextSharp.text.Image.GetInstance(imageBytes);
                            img.ScaleToFit(300f, 300f);
                            document.Add(img);
                        }
                    }

                    document.NewPage();
                }

                if (ncrDetails.Procurement != null)
                {
                    document.Add(Chunk.NEWLINE);
                    document.Add(Chunk.NEWLINE);
                    heading = new Paragraph("Procurement", headingFont);
                    heading.Alignment = Element.ALIGN_CENTER;
                    document.Add(heading);

                    document.Add(new Paragraph($"Supplier Items Returned: {ncrDetails.Procurement.SuppItemsBack}", fieldFont));

                    document.Add(new Paragraph($"Supplier Return Completed: {ncrDetails.Procurement.SuppReturnCompleted}", fieldFont));

                    document.Add(new Paragraph($"Is Credit Expected: {ncrDetails.Procurement.IsCreditExpec}", fieldFont));

                    document.Add(new Paragraph($"Charge Supplier: {ncrDetails.Procurement.ChargeSupplier}", fieldFont));

                    document.Add(new Paragraph($"RMA Number: {ncrDetails.Procurement.RMANo}", fieldFont));

                    document.Add(new Paragraph($"Expected Date of Return: {ncrDetails.Procurement.ExpecDateOfReturn}", fieldFont));

                    document.Add(new Paragraph($"Procurement Sign: {ncrDetails.Procurement.ProcurementSign}", fieldFont));

                    document.Add(new Paragraph($"Procurement Date: {ncrDetails.Procurement.ProcurementDate}", fieldFont));

                    document.Add(new Paragraph("Report Notes", headingFont));

                    document.Add(new Paragraph($"Carrier Information: {ncrDetails.Procurement.CarrierInfo}", fieldFont));


                    if (ncrDetails.Procurement.QualityPhotos != null && ncrDetails.Procurement.QualityPhotos.Count > 0)
                    {
                        foreach (var image in ncrDetails.Procurement.QualityPhotos)
                        {
                            var imageBytes = image.Content;
                            var img = iTextSharp.text.Image.GetInstance(imageBytes);
                            img.ScaleToFit(300f, 300f);
                            document.Add(img);
                        }
                    }

                    document.NewPage();
                }




                document.Close();


                var pdfData = memoryStream.ToArray();
                return File(pdfData, "application/pdf", "NCRDetails.pdf");
            }

        }
        private bool NCRExists(int id)
        {
            return _context.NCRs.Any(e => e.ID == id);
        }
    }
}