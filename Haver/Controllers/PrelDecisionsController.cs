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


namespace Haver.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PrelDecisionController : LookupsController
    {
        private readonly HaverContext _context;

        public PrelDecisionController(HaverContext context)
        {
            _context = context;
        }

        // GET: PrelDecision
        public IActionResult Index()
        {
            return Redirect(ViewData["returnURL"].ToString());
        }

        // GET: PrelDecision/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PrelDecision/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Decision")] PrelDecision PrelDecision)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(PrelDecision);
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(PrelDecision);
        }

        // GET: PrelDecision/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PrelDecisions == null)
            {
                return NotFound();
            }

            var PrelDecision = await _context.PrelDecisions.FindAsync(id);
            if (PrelDecision == null)
            {
                return NotFound();
            }
            return View(PrelDecision);
        }

        // POST: PrelDecision/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var PrelDecisionToUpdate = await _context.PrelDecisions
                .FirstOrDefaultAsync(m => m.ID == id);

            //Check that you got it or exit with a not found error
            if (PrelDecisionToUpdate == null)
            {
                return NotFound();
            }

            //Try updating it with the values posted
            if (await TryUpdateModelAsync<PrelDecision>(PrelDecisionToUpdate, "",
                d => d.Decision))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return Redirect(ViewData["returnURL"].ToString());
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrelDecisionExists(PrelDecisionToUpdate.ID))
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
            return View(PrelDecisionToUpdate);
        }

        // GET: PrelDecision/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PrelDecisions == null)
            {
                return NotFound();
            }

            var PrelDecision = await _context.PrelDecisions
                .FirstOrDefaultAsync(m => m.ID == id);
            if (PrelDecision == null)
            {
                return NotFound();
            }

            return View(PrelDecision);
        }

        // POST: PrelDecision/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PrelDecisions == null)
            {
                return Problem("Entity set 'HaverContext.PrelDecisions'  is null.");
            }
            var PrelDecision = await _context.PrelDecisions
               .FirstOrDefaultAsync(m => m.ID == id);
            try
            {
                if (PrelDecision != null)
                {
                    _context.PrelDecisions.Remove(PrelDecision);
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
            return View(PrelDecision);

        }

        private bool PrelDecisionExists(int id)
        {
            return _context.PrelDecisions.Any(e => e.ID == id);
        }
    }
}
