using Haver.CustomControllers;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Haver.Data;
using Haver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities.Encoders;


namespace Haver.Controllers
{
    [Authorize(Roles = "Admin,Quality Inspector")]
    public class ProblemController : LookupsController
    {
        private readonly HaverContext _context;

        public ProblemController(HaverContext context)
        {
            _context = context;
        }

        // GET: Problem
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Problem/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Problem/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ProblemDescription")] Problem Problem)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(Problem);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Problem name alredy exists.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(Problem);
        }

        // GET: Problem/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Problems == null)
            {
                return NotFound();
            }

            var Problem = await _context.Problems.FindAsync(id);
            if (Problem == null)
            {
                return NotFound();
            }
            return View(Problem);
        }

        // POST: Problem/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var ProblemToUpdate = await _context.Problems
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (ProblemToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Problem>(ProblemToUpdate, "",
                d => d.ProblemDescription))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProblemExists(ProblemToUpdate.ID))
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
            return View(ProblemToUpdate);
        }

        // GET: Problem/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Problems == null)
            {
                return NotFound();
            }

            var Problem = await _context.Problems
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Problem == null)
            {
                return NotFound();
            }

            return View(Problem);
        }

        // POST: Problem/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Problems == null)
            {
                return NotFound();
            }
            var Problem = await _context.Problems
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (Problem != null)
                {
                    _context.Problems.Remove(Problem);
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
            return View(Problem);

        }

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            string feedBack = string.Empty;
            string feedBackGreen = string.Empty;
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
                        if (workSheet.Cells[1, 1].Text == "Problem Description")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                Problem p = new Problem();
                                try
                                {
                                    // Row by row...
                                    p.ProblemDescription = workSheet.Cells[row, 1].Text;
                                    _context.Problems.Add(p);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += "Error: Record " + p.ProblemDescription + " was rejected as a duplicate."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + p.ProblemDescription + " caused an error."
                                                + "<br />";
                                    }
                                    //Here is the trick to using SaveChanges in a loop.  You must remove the 
                                    //offending object from the cue or it will keep raising the same error.
                                    _context.Remove(p);
                                }
                                catch (Exception ex)
                                {
                                    errorCount++;
                                    if (ex.GetBaseException().Message.Contains("correct format"))
                                    {
                                        feedBack += "Error: Record " + p.ProblemDescription + " was rejected because the Problem Description was not in the correct format."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + p.ProblemDescription + " caused and error."
                                                + "<br />";
                                    }
                                }
                            }
                            feedBackGreen += "Finished Importing " + (successCount + errorCount).ToString() +
                                " Records with " + successCount.ToString() + " inserted and " +
                                errorCount.ToString() + " rejected";
                        }
                        else
                        {
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> Remember, you must have the heading 'Problem Description' in the first cell of the first row.";
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

            TempData["FeedbackGreen"] = feedBackGreen + "<br />";
            TempData["Feedback"] = feedBack + "<br />";

            //Note that we are assuming that you are using the Preferred Approach to Lookup Values
            //And the custom LookupsController
            return Redirect(ViewData["returnURL"].ToString());
        }

        private bool ProblemExists(int id)
        {
            return _context.Problems.Any(e => e.ID == id);
        }
    }
}