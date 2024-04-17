using Haver.CustomControllers;
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using Haver.Data;
using Haver.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities.Encoders;


namespace Haver.Controllers
{
    [Authorize(Roles = "Admin,Quality Inspector")]
    public class PartController : LookupsController
    {
        private readonly HaverContext _context;

        public PartController(HaverContext context)
        {
            _context = context;
        }

        // GET: Part
        public IActionResult Index()
        {

            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Part/Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Part/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,PartDesc, PartNumber")] Part Part)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(Part);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Part number alredy exists.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(Part);
        }

        // GET: Part/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parts == null)
            {
                return NotFound();
            }

            var Part = await _context.Parts.FindAsync(id);
            if (Part == null)
            {
                return NotFound();
            }
            return View(Part);
        }

        // POST: Part/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var PartToUpdate = await _context.Parts
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (PartToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Part>(PartToUpdate, "", d => d.PartNumber,
                d => d.PartDesc))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartExists(PartToUpdate.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                        + "was modified by another user. Please go back and refresh.");
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }
            return View(PartToUpdate);
        }

        // GET: Part/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Parts == null)
            {
                return NotFound();
            }

            var Part = await _context.Parts
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Part == null)
            {
                return NotFound();
            }

            return View(Part);
        }

        // POST: Part/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Parts == null)
            {
                return Problem("Entity set 'HaverContext.Parts'  is null.");
            }
            var Part = await _context.Parts
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (Part != null)
                {
                    _context.Parts.Remove(Part);
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
            return View(Part);

        }

        [HttpPost]
        public async Task<IActionResult> InsertFromExcel(IFormFile theExcel)
        {
            string feedBackGreen = string.Empty;
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
                        List<string> partsWithSupplierNull = new List<string>();
                        if (workSheet.Cells[1, 1].Text == "Part Description" &&
                            workSheet.Cells[1, 2].Text == "Part Number" &&
                            workSheet.Cells[1, 3].Text == "Supplier Name")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                Part p = new Part();
                                try
                                {
                                    // Row by row...
                                    p.PartDesc = workSheet.Cells[row, 1].Text;
                                    p.PartNumber = workSheet.Cells[row, 2].GetValue<int>();

                                    _context.Parts.Add(p);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += "Error: Part " + p.PartNumber + " was rejected as a duplicate."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + p.PartNumber + " caused an error."
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
                                        feedBack += "Error: Record " + p.PartNumber + " was rejected because the number was not in the correct format."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + p.PartNumber + " caused and error."
                                                + "<br />";
                                    }
                                }
                            }
                            if(partsWithSupplierNull.Count > 1)
                            {
                                string commaSeparatedParts = string.Join(", ", partsWithSupplierNull);
                                string partOrParts = partsWithSupplierNull.Count > 1 ? "parts" : "part";

                                feedBack += "The following " + (partOrParts) + " were inserted with null or invalid supplier names " + (commaSeparatedParts)
                                + "<br />";
                            }
                            feedBackGreen += "Finished Importing " + (successCount + errorCount).ToString() +
                                " Records with " + successCount.ToString() + " inserted and " +
                                errorCount.ToString() + " rejected";
                        }
                        else
                        {
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> Remember, you must have the headings 'Part Description', 'Part Number' and 'Supplier Name' in the first three cells of the first row.";
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

        private bool PartExists(int id)
        {
            return _context.Parts.Any(e => e.ID == id);
        }


        private SelectList SupplierList(int? selectedId)
        {
            return new SelectList(_context
                .Suppliers
                .OrderBy(m => m.SupplierName), "ID", "SupplierName", selectedId);
        }

    }
}