using Entsiegeln.Data;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Berlin2bgreen.Controllers
{
    public class ErrorController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;
        private readonly EntsiegelnContext _context;

        public ErrorController(EntsiegelnContext context, IWebHostEnvironment environment, ILogger<ErrorController> logger)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Errors.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult> PostError([FromQuery] int? user, [FromQuery] int code, [FromBody] string text)
        {
            _logger.LogError("Errorcode:{error} User: {id}", code, user);
            Error error = new Error();
            error.Date = DateTime.UtcNow;
            error.ErrorCode = code;
            error.Text = text;
            if (User.Identity.IsAuthenticated)
            {
                error.Text += $" - User: {User.FindFirstValue(ClaimTypes.Name)}";
            }
            error.User = user;
            _context.Errors.Add(error);
            await _context.SaveChangesAsync();
            return Ok();
        }

        public async Task<IActionResult> DeleteErrors()
        {
            var errors = await _context.Errors.ToListAsync();

            _context.Errors.RemoveRange(errors);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Error");
        }
    }
}
