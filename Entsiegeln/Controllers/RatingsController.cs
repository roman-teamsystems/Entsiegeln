using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Data;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Authorization;

namespace Berlin2bgreen.Controllers
{
    [Authorize(Roles = "Admin,Manager,User")]
    [Route("[controller]")]
    [ApiController]
    public class RatingsController : ControllerBase
    {
        private readonly EntsiegelnContext _context;

        public RatingsController(EntsiegelnContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rating>>> GetRatings()
        {
            return await _context.Ratings.ToListAsync();
        }

        [Route("[Action]/{projectId}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<int[]>> CountByProject(int projectId)
        {
            int[] ratingsCount = new int[2];
            ratingsCount[0] = await _context.Ratings.Where(r => (r.ProjectId == projectId) && (r.Like == true)).CountAsync();
            ratingsCount[1] = await _context.Ratings.Where(r => (r.ProjectId == projectId) && (r.Favorite == true)).CountAsync();
            return ratingsCount;
        }

        [Route("[Action]/{projectId}")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Rating>>> GetProjectRatings(int projectId)
        {
            return await _context.Ratings.Where(r => (r.ProjectId == projectId)).ToListAsync();
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Rating>>> GetUserRatings(string userId)
        {
            var rating = await _context.Ratings.Where(r => r.UserId == userId).ToListAsync();

            if (rating == null)
            {
                return NotFound();
            }

            return rating;
        }

        [HttpGet]
        [Route("[Action]")]
        [AllowAnonymous]
        public async Task<Dictionary<int, int>> GetFavoriteMap()
        {
            Dictionary<int, int> favoriteMap = new Dictionary<int, int>();
            List<Rating> ratings = await _context.Ratings.ToListAsync();
            foreach (var rating in ratings)
            {
                if (rating.Favorite == true)
                {
                    if (favoriteMap.ContainsKey(rating.ProjectId))
                    {
                        favoriteMap[rating.ProjectId]++;
                    }
                    else
                    {
                        favoriteMap.Add(rating.ProjectId, 1);
                    }
                }
            }
            return favoriteMap;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutRating(int id, Rating rating)
        {
            if (id != rating.Id)
            {
                return BadRequest();
            }

            _context.Entry(rating).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RatingExists(id))
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

        [AllowAnonymous]
        [Route("[Action]/{id}")]
        public async Task<IActionResult> UnjoinFavorite(int id)
        {
            var rating = await _context.Ratings.Where(r => r.Id == id).FirstOrDefaultAsync();
            rating.Favorite = false;
            _context.Entry(rating).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RedirectToAction("UserPage", "Users", new { id = rating.UserId });
        }

        [HttpPost]
        public async Task<ActionResult<Rating>> PostRating(Rating rating)
        {
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRating", new { id = rating.Id }, rating);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRating(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);
            if (rating == null)
            {
                return NotFound();
            }

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RatingExists(int id)
        {
            return _context.Ratings.Any(e => e.Id == id);
        }
    }
}
