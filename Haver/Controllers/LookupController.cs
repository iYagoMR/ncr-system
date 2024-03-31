using Haver.CustomControllers;
using Haver.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Haver.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LookupController : CognizantController
    {
        private readonly HaverContext _context;

        public LookupController(HaverContext context)
        {
            _context = context;
        }
        public IActionResult Index(string Tab = "Information-Tab")
        {
            //Note: select the tab you want to load by passing in
            ViewData["Tab"] = Tab;
            return View();
        }
        public PartialViewResult EngReview()
        {
            ViewData["EngReviewID"] = new
                SelectList(_context.EngReviews
                .OrderBy(a => a.Review), "ID", "Review");
            return PartialView("_EngReview");
        }
        public PartialViewResult Part()
        {
            ViewData["PartID"] = new
                SelectList(_context.Parts
                .OrderBy(a => a.PartDesc), "ID", "PartDesc");
            return PartialView("_Part");
        }
        public PartialViewResult Supplier()
        {
            ViewData["SupplierID"] = new
                SelectList(_context.Suppliers
                .OrderBy(a => a.SupplierName), "ID", "SupplierName");
            return PartialView("_Supplier");
        }
        public PartialViewResult Problem()
        {
            ViewData["ProblemID"] = new
                SelectList(_context.Problems
                .OrderBy(a => a.ProblemDescription), "ID", "ProblemDescription");
            return PartialView("_Problem");
        }
        public PartialViewResult PrelDecision()
        {
            ViewData["PrelDecisionID"] = new
                SelectList(_context.PrelDecisions
                .OrderBy(a => a.Decision), "ID", "Decision");
            return PartialView("_PrelDecision");
        }
    }
}
