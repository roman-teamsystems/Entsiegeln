using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Data;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace Berlin2bgreen.Controllers
{
    public class PreferencesController : Controller
    {
        private readonly EntsiegelnContext _context;

        public PreferencesController(EntsiegelnContext context)
        {
            _context = context;
        }

        // GET: Preferences
        public async Task<IActionResult> Index()
        {
            return View(await _context.Preferences.ToListAsync());
        }

        // GET: Preferences/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preferences = await _context.Preferences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (preferences == null)
            {
                return NotFound();
            }

            return View(preferences);
        }

        // GET: Preferences/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Preferences/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Icon1,Icon2,Icon3,Icon4,Icon5")] Preferences preferences)
        {
            if (ModelState.IsValid)
            {
                _context.Add(preferences);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(preferences);
        }

        // GET: Preferences/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preferences = await _context.Preferences.FindAsync(id);
            if (preferences == null)
            {
                return NotFound();
            }
            return View(preferences);
        }

        // POST: Preferences/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Icon1,Icon2,Icon3,Icon4,Icon5")] Preferences preferences)
        {
            if (id != preferences.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(preferences);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PreferencesExists(preferences.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(preferences);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int id, int fileId, IFormFile formFile)
        {
            if ((formFile == null) || (formFile.Length == 0))
            {
                return RedirectToAction(nameof(Index));
            }
            if (PreferencesExists(id) == false)
            {
                return NotFound();
            }
            var preferences = await _context.Preferences
                .FirstOrDefaultAsync(m => m.Id == id);
            Stream stream = formFile.OpenReadStream();
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                switch (fileId)
                {
                    case 1: { preferences.Icon1 = reader.ReadToEnd(); break; }
                    case 2: { preferences.Icon2 = reader.ReadToEnd(); break; }
                    case 3: { preferences.Icon3 = reader.ReadToEnd(); break; }
                    case 4: { preferences.Icon4 = reader.ReadToEnd(); break; }
                    case 5: { preferences.Icon5 = reader.ReadToEnd(); break; }
                }
                
            }
            stream.Dispose();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(preferences);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PreferencesExists(preferences.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            return Ok(await _context.Preferences.FirstOrDefaultAsync());
        }

        // GET: Preferences/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var preferences = await _context.Preferences
                .FirstOrDefaultAsync(m => m.Id == id);
            if (preferences == null)
            {
                return NotFound();
            }

            return View(preferences);
        }

        // POST: Preferences/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var preferences = await _context.Preferences.FindAsync(id);
            _context.Preferences.Remove(preferences);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PreferencesExists(int id)
        {
            return _context.Preferences.Any(e => e.Id == id);
        }
    }
}
