using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Data;
using Entsiegeln.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Entsiegeln.Areas.Identity.Data;

namespace Berlin2bgreen.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly EntsiegelnContext _context;
        private readonly IConfiguration config;
        private readonly UserManager<EntsiegelnUser> userManager;

        public UsersController(EntsiegelnContext context, IConfiguration configuration, UserManager<EntsiegelnUser> userManager)
        {
            _context = context;
            config = configuration;
            this.userManager = userManager;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // GET: Users/Details/5
        //public async Task<IActionResult> Details(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(user);
        //}

        [AllowAnonymous]
        public async Task<IActionResult> UserPage(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _context.Users.Include(u => u.Projects).Include(u => u.Ratings.Where(r => r.Favorite == true)).ThenInclude(r => r.Project).FirstOrDefaultAsync(m => m.Id == id);
                if (user == null)
                {
                    return NotFound();
                }
                if (User.Identity.Name == user.UserName)
                {

                    return View(user);
                }
            }
            return NotFound();
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            //var user = await _context.Users.FindAsync(id);
            var user = await _context.Users.Include(u => u.Ratings).SingleOrDefaultAsync(u => u.Id == id);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<int> GetMaxNumberOfFavorites(string id)
        {
            if (id == null)
            {
                return 0;
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return 0;
            }
            return (user.MaxNumberOfFavorites == null) ? 0 : (int)user.MaxNumberOfFavorites;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CheckPermission()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Manager") || User.IsInRole("Admin"))
                {
                    return Ok();
                }
            }
            return Unauthorized();
        }

        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await userManager.AddToRoleAsync(user, "Admin");
            return Ok();
        }

        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await userManager.RemoveFromRoleAsync(user, "Admin");
            return Ok();
        }

        public async Task<IActionResult> MakeManager(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await userManager.AddToRoleAsync(user, "Manager");
            return Ok();
        }

        public async Task<IActionResult> RemoveManager(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            await userManager.RemoveFromRoleAsync(user, "Manager");
            return Ok();
        }

    }
}
