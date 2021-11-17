using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Data;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Azure.Storage.Blobs;

namespace Entsiegeln.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BilderController : Controller
    {
        private readonly EntsiegelnContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly BlobContainerClient _blobContainerClient;

        public BilderController(EntsiegelnContext context, IWebHostEnvironment environment, BlobContainerClient blobContainerClient)
        {
            _context = context;
            _environment = environment;
            _blobContainerClient = blobContainerClient;
        }

        // GET: api/Bilder
        [HttpGet]
        [Route("[Action]")]
        public async Task<ActionResult<IEnumerable<Bild>>> Index()
        {
            var bilder = await _context.Bilder.ToListAsync();
            ViewBag.Count = bilder.Count;
            return View(bilder);
        }

        // GET: api/Bilder/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bild>> GetBild(int id)
        {
            var bild = await _context.Bilder.FindAsync(id);

            if (bild == null)
            {
                return NotFound();
            }

            return bild;
        }

        // PUT: api/Bilder/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBild(int id, Bild bild)
        {
            if (id != bild.Id)
            {
                return BadRequest();
            }

            _context.Entry(bild).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BildExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Bilder
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bild>> PostBild(Bild bild)
        {
            _context.Bilder.Add(bild);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBild", new { id = bild.Id }, bild);
        }

        // DELETE: api/Bilder/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBild(int id)
        {
            var bild = await _context.Bilder.FindAsync(id);
            if (bild == null)
            {
                return NotFound();
            }
            var guid = bild.Name;
            _context.Bilder.Remove(bild);
            await _context.SaveChangesAsync();
            DeleteAzureImage(guid.ToString());
            return NoContent();
        }

        private void DeleteAzureImage(string guid)
        {
            var blobClient = _blobContainerClient.GetBlobClient(guid + ".jpeg");
            if (blobClient.Exists())
            {
                blobClient.Delete();
            }
        }


        private bool BildExists(int id)
        {
            return _context.Bilder.Any(e => e.Id == id);
        }
    }
}
