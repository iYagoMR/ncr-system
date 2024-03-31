using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Haver.Data;
using Haver.Models;
using Haver.CustomControllers;
using OfficeOpenXml;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;

namespace Haver.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EngReviewController : LookupsController
    {
        private readonly HaverContext _context;

        public EngReviewController(HaverContext context)
        {
            _context = context;
        }

        // GET: EngReview
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: EngReview/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EngReview/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Review")] EngReview EngReview)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(EngReview);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(EngReview);
        }

        // GET: EngReview/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.EngReviews == null)
            {
                return NotFound();
            }

            var EngReview = await _context.EngReviews.FindAsync(id);
            if (EngReview == null)
            {
                return NotFound();
            }
            return View(EngReview);
        }

        // POST: EngReview/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var EngReviewToUpdate = await _context.EngReviews
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (EngReviewToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<EngReview>(EngReviewToUpdate, "",
                d => d.Review))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EngReviewExists(EngReviewToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(EngReviewToUpdate);
        }

        // GET: EngReview/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.EngReviews == null)
            {
                return NotFound();
            }

            var EngReview = await _context.EngReviews
                .FirstOrDefaultAsync(m => m.ID == id);
            if (EngReview == null)
            {
                return NotFound();
            }

            return View(EngReview);
        }

        // POST: EngReview/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.EngReviews == null)
            {
                return Problem("Entity set 'HaverContext.EngReviews'  is null.");
            }
            var EngReview = await _context.EngReviews
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (EngReview != null)
                {
                    _context.EngReviews.Remove(EngReview);
                }
                await _context.SaveChangesAsync();
                return Redirect(ViewData["returnURL"].ToString());
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to Delete " + ViewData["ControllerFriendlyName"] +
                        ". Remember, you cannot delete a " + ViewData["ControllerFriendlyName"] + " that has related records.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(EngReview);

        }

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            string feedBack = string.Empty;
            if (theExcel != null)
            {
                string mimeType = theExcel.ContentType;
                long fileLength = theExcel.Length;
                if (!(mimeType == "" || fileLength == 0))//Looks like we have a file!!!
                {
                    if (mimeType.Contains("excel") || mimeType.Contains("spreadsheet"))
                    {
                        ExcelPackage excel;
                        using (var memoryStream = new MemoryStream())
                        {
                            await theExcel.CopyToAsync(memoryStream);
                            excel = new ExcelPackage(memoryStream);
                        }
                        var workSheet = excel.Workbook.Worksheets[0];
                        var start = workSheet.Dimension.Start;
                        var end = workSheet.Dimension.End;
                        int successCount = 0;
                        int errorCount = 0;
                        if (workSheet.Cells[1, 1].Text == "Review")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                EngReview e = new EngReview();
                                try
                                {
                                    // Row by row...
                                    e.Review = workSheet.Cells[row, 1].Text;
                                    _context.EngReviews.Add(e);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += "Error: Record " + e.Review + " was rejected as a duplicate."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + e.Review + " caused an error."
                                                + "<br />";
                                    }
                                    //Here is the trick to using SaveChanges in a loop.  You must remove the 
                                    //offending object from the cue or it will keep raising the same error.
                                    _context.Remove(e);
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    if (ex.GetBaseException().Message.Contains("correct format"))
                                    {
                                        feedBack += "Error: Record " + e.Review + " was rejected becuase the standard charge was not in the correct format."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + e.Review + " caused and error."
                                                + "<br />";
                                    }
                                }
                            }
                            feedBack += "Finished Importing " + (successCount + errorCount).ToString() +
                                " Records with " + successCount.ToString() + " inserted and " +
                                errorCount.ToString() + " rejected";
                        }
                        else
                        {
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> Remember, you must have the headings 'Name' and 'Standard Charge' in the first two cells of the first row.";
                        }
                    }
                    else
                    {
                        feedBack = "Error: That file is not an Excel spreadsheet.";
                    }
                }
                else
                {
                    feedBack = "Error:  file appears to be empty";
                }
            }
            else
            {
                feedBack = "Error: No file uploaded";
            }

            TempData["Feedback"] = feedBack + "<br /><br />";

            //Note that we are assuming that you are using the Preferred Approach to Lookup Values
            //And the custom LookupsController
            return Redirect(ViewData["returnURL"].ToString());
        }

        private bool EngReviewExists(int id)
        {
            return _context.EngReviews.Any(e => e.ID == id);
        }
    }
}
