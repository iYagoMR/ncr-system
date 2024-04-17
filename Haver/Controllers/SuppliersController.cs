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
using System.Numerics;
using Microsoft.AspNetCore.Authorization;
using OfficeOpenXml;
using Org.BouncyCastle.Utilities.Encoders;


namespace Haver.Controllers
{
    [Authorize(Roles = "Admin,Quality Inspector")]
    public class SupplierController : LookupsController
    {
        private readonly HaverContext _context;

        public SupplierController(HaverContext context)
        {
            _context = context;
        }

        // GET: Supplier
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: Supplier/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,SupplierName, SupplierCode")] Supplier Supplier)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var dosSupCodeExists = _context.Suppliers.FirstOrDefault(s => s.SupplierCode == Supplier.SupplierCode);

                    _context.Add(Supplier);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException dex)
            {
                if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Supplier code alredy exists.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

            return View(Supplier);
        }

        // GET: Supplier/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var Supplier = await _context.Suppliers.FindAsync(id);
            if (Supplier == null)
            {
                return NotFound();
            }
            return View(Supplier);
        }

        // POST: Supplier/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var SupplierToUpdate = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (SupplierToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<Supplier>(SupplierToUpdate, "",
                d => d.SupplierName, d => d.SupplierCode))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SupplierExists(SupplierToUpdate.ID))
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
            return View(SupplierToUpdate);
        }

        // GET: Supplier/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Suppliers == null)
            {
                return NotFound();
            }

            var Supplier = await _context.Suppliers
                .FirstOrDefaultAsync(m => m.ID == id);
            if (Supplier == null)
            {
                return NotFound();
            }

            return View(Supplier);
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Suppliers == null)
            {
                return Problem("Entity set 'HaverContext.Suppliers'  is null.");
            }
            var Supplier = await _context.Suppliers
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (Supplier != null)
                {
                    _context.Suppliers.Remove(Supplier);
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
            return View(Supplier);
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
                        if (workSheet.Cells[1, 1].Text == "Supplier Name" &&
                            workSheet.Cells[1, 2].Text == "Supplier Code")
                        {
                            for (int row = start.Row + 1; row <= end.Row; row++)
                            {
                                Supplier e = new Supplier();
                                try
                                {
                                    // Row by row...
                                    e.SupplierName = workSheet.Cells[row, 1].Text;
                                    e.SupplierCode = workSheet.Cells[row, 2].GetValue<int>();
                                    _context.Suppliers.Add(e);
                                    _context.SaveChanges();
                                    successCount++;
                                }
                                catch (DbUpdateException dex)
                                {
                                    errorCount++;
                                    if (dex.GetBaseException().Message.Contains("UNIQUE constraint failed"))
                                    {
                                        feedBack += "Error: Supplier " + e.SupplierName + " was rejected because its supplier code must be unique"
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + e.SupplierName + " caused an error."
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
                                        feedBack += "Error: Record " + e.SupplierName + " was rejected because the Supplier Name was not in the correct format."
                                                + "<br />";
                                    }
                                    else
                                    {
                                        feedBack += "Error: Record " + e.SupplierName + " caused and error."
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
                            feedBack = "Error: You may have selected the wrong file to upload.<br /> Remember, you must have the headings 'Supplier Name' and 'Supplier Code' in the first two cells of the first row.";
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

        private bool SupplierExists(int id)
        {
            return _context.Suppliers.Any(e => e.ID == id);
        }
    }
}
