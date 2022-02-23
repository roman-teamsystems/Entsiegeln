using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Azure.Storage.Blobs;
using Entsiegeln.Data;

namespace Entsiegeln.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager,User")]
    public class ApiController : ControllerBase
    {
        private readonly EntsiegelnContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;
        private readonly BlobContainerClient _blobContainerClient;


        public ApiController(EntsiegelnContext context, IWebHostEnvironment environment, ILogger<ApiController> logger, BlobContainerClient blobContainerClient)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _blobContainerClient = blobContainerClient;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjets(int bezirk, string userId = null, bool onlynew = false)
        {
            if ((User.Identity.IsAuthenticated) && (userId != null))
            {
                if (User.IsInRole("Admin") || User.IsInRole("Manager"))
                {
                    if (onlynew)
                    {
                        return await _context.Projekte.Where(p => (int)p.Bezirk == bezirk && p.Status == 0).Include(p => p.Bilder).ToListAsync();
                    }
                    else
                    {
                        return await _context.Projekte.Where(p => (int)p.Bezirk == bezirk).Include(p => p.Bilder).ToListAsync();
                    }
                }
                return await _context.Projekte.Where(p => (int)p.Bezirk == bezirk && ((p.UserId == userId) || (p.Status > 0))).Include(p => p.Bilder).ToListAsync();
            }
            return await _context.Projekte.Where(p => (int)p.Bezirk == bezirk && p.Status > 0).Include(p => p.Bilder).ToListAsync();
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projekte.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }
            await _context.Entry(project).Collection(p => p.Bilder).LoadAsync();
            return project;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, Project project)
        {
            if (id != project.Id)
            {
                return BadRequest();
            }

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id))
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

        [HttpPut("{id}/{status}")]
        public async Task<IActionResult> PutChangedStatus(int id, byte status)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Manager"))
            {
                var project = await _context.Projekte.FindAsync(id);
                if (project == null)
                {
                    return NotFound();
                }
                project.Status = status;

                _context.Entry(project).State = EntityState.Modified;
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ProjectExists(id))
                    {
                        _logger.LogError(ex, "Project not found in PutChangedStatus({p1}, {p2})", id, status);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Unknown error in PutChangedStatus()");
                        throw;
                    }
                }
                return NoContent();
            }
            return Unauthorized();
        }

        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            _context.Projekte.Add(project);
            await _context.SaveChangesAsync();
            _logger.LogError("Project wurde von {username} angelegt.", User.Identity.Name);
            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projekte.Where(p => p.Id == id).Include(p => p.Bilder).FirstAsync();
            if (project == null)
            {
                return NotFound();
            }
            Entsiegeln.Controllers.ProjectsController.DeleteFiles(_environment, project, _blobContainerClient);
            _context.Projekte.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projekte.Any(e => e.Id == id);
        }
    }
}
