using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Eventus.Samples.Core.ReadModel;
using Eventus.Samples.Web.Data;

namespace Eventus.Samples.Web
{
    public class BankAccountSummariesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BankAccountSummariesController(ApplicationDbContext context)
        {
            _context = context;    
        }

        // GET: BankAccountSummaries
        public async Task<IActionResult> Index()
        {
            return View(await _context.BankAccountSummary.ToListAsync());
        }

        // GET: BankAccountSummaries/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccountSummary = await _context.BankAccountSummary
                .SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccountSummary == null)
            {
                return NotFound();
            }

            return View(bankAccountSummary);
        }

        // GET: BankAccountSummaries/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BankAccountSummaries/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Balance")] BankAccountSummary bankAccountSummary)
        {
            if (ModelState.IsValid)
            {
                bankAccountSummary.Id = Guid.NewGuid();
                _context.Add(bankAccountSummary);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(bankAccountSummary);
        }

        // GET: BankAccountSummaries/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccountSummary = await _context.BankAccountSummary.SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccountSummary == null)
            {
                return NotFound();
            }
            return View(bankAccountSummary);
        }

        // POST: BankAccountSummaries/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,Balance")] BankAccountSummary bankAccountSummary)
        {
            if (id != bankAccountSummary.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bankAccountSummary);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BankAccountSummaryExists(bankAccountSummary.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(bankAccountSummary);
        }

        // GET: BankAccountSummaries/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bankAccountSummary = await _context.BankAccountSummary
                .SingleOrDefaultAsync(m => m.Id == id);
            if (bankAccountSummary == null)
            {
                return NotFound();
            }

            return View(bankAccountSummary);
        }

        // POST: BankAccountSummaries/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var bankAccountSummary = await _context.BankAccountSummary.SingleOrDefaultAsync(m => m.Id == id);
            _context.BankAccountSummary.Remove(bankAccountSummary);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool BankAccountSummaryExists(Guid id)
        {
            return _context.BankAccountSummary.Any(e => e.Id == id);
        }
    }
}
