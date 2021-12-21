using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Data;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Entsiegeln.Controllers
{
    public class ExportProject
    {
        public int Bezirk { get; set; }
        public DateTime Datum { get; set; }
        public string Beitragender { get; set; }
        public string WKTKoordinaten { get; set; }
        public string Strasse { get; set; }
        public string Plz { get; set; }
        public string Bezeichnung { get; set; }
        public string Details { get; set; }
        public string[] Bilder { get; set; }
        public bool BSV { get; set; }
        public bool Kub { get; set; }
        public bool Bpf { get; set; }
        public bool PzuB { get; set; }
        public bool PentsV { get; set; }
        public bool VzuG { get; set; }
        public bool Div { get; set; }
        public bool Vbeet { get; set; }
        public bool PP { get; set; }
        public bool UG { get; set; }
        public bool AzuX { get; set; }
        public bool GwPI { get; set; }
        public bool RuF { get; set; }
        public byte Prio { get; set; }
        public byte Status { get; set; }
    }

    public enum Sortierung
    {
        Keine, Datum, Strasse, Likes
    }

    public enum Status
    {
        Eingereicht, Angenommen
    }
    public enum Filter
    {
        Kein, Bpf, Bsv, Vbeet, Bau, Gruen, Fassade
    }

    [Authorize(Roles = "Manager,Admin")]
    public class ProjectsController : Controller
    {
        private readonly EntsiegelnContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly BlobContainerClient _blobContainerClient;


        public ProjectsController(EntsiegelnContext context, IWebHostEnvironment environment, BlobContainerClient blobContainerClient)
        {
            _context = context;
            _environment = environment;
            _blobContainerClient = blobContainerClient;
        }

        private string GetMapImage(Project project)
        {
            const string credentials = "AtjRMMexXb8SbkvunWD_M8mXGoHUPgvmoI3PJ7Hcy4n7-I5ZNJ_FJNoPPP3EqKP_";
            string mapUrl = "https://dev.virtualearth.net/REST/v1/Imagery/Map/Road/{centerPoint}/{zoomLevel}?mapSize={mapSize}&pushpin={pushpin}&mapLayer={mapLayer}&key={BingMapsAPIKey}";
            mapUrl = mapUrl.Replace("{BingMapsAPIKey}", credentials);
            mapUrl = mapUrl.Replace("{mapSize}", "533,300");
            mapUrl = mapUrl.Replace("{mapLayer}", "Basemap,Buildings");
            mapUrl = mapUrl.Replace("{centerPoint}", project.Koordinaten.Coordinates[0].Y.ToString(new CultureInfo("en-US")) + "," + project.Koordinaten.Coordinates[0].X.ToString(new CultureInfo("en-US")));
            mapUrl = mapUrl.Replace("{pushpin}", project.Koordinaten.Coordinates[0].Y.ToString(new CultureInfo("en-US")) + "," + project.Koordinaten.Coordinates[0].X.ToString(new CultureInfo("en-US")));
            mapUrl = mapUrl.Replace("{zoomLevel}", "17");
            return mapUrl;
        }

        private static int CompareLikes(Project x, Project y)
        {
            if (x.Pro < y.Pro)
            {
                return 1;
            }
            else if (x.Pro > y.Pro)
            {
                return -1;
            }
            return 0;
        }

        // GET: Projects
        public async Task<IActionResult> Index(int bezirk, bool newProjects = false, int sortierung = 0, int filter = 0)
        {
            if (newProjects)
            {
                ViewBag.Sort = 0;
                ViewBag.NewProjects = true;
                return View(await _context.Projekte.Where(p => (int)p.Bezirk == bezirk && p.Status == 0).Include(p => p.Ratings).ToListAsync());
            }
            else
            {
                ViewBag.NewProjects = false;
                ViewBag.Sort = sortierung;
                ViewBag.Filter = filter;
                IQueryable<Project> query;
                switch ((Filter)filter)
                {
                    case Filter.Bpf:
                        {
                            query = _context.Projekte.Include(p => p.Ratings)
                     .Where(p => (int)p.Bezirk == bezirk && (p.Bpf == true) || (p.PzuB == true)); break;
                        }
                    case Filter.Bau:
                        {
                            query = _context.Projekte.Include(p => p.Ratings)
                     .Where(p => (int)p.Bezirk == bezirk && ((p.Kub == true) || (p.PentsV == true) || (p.AzuX == true) || (p.GwPI == true))
                                                                                                                && (p.Bpf == false) && (p.PzuB == false)); break;
                        }
                    case Filter.Vbeet:
                        {
                            query = _context.Projekte.Include(p => p.Ratings)
                   .Where(p => (int)p.Bezirk == bezirk && ((p.PP == true) || (p.UG == true) || (p.Vbeet == true))
                                                                                                              && (p.Bpf == false) && (p.PzuB == false)
                                                                                                              && (p.Kub == false) && (p.PentsV == false) && (p.AzuX == false) && (p.GwPI == false)); break;
                        }
                    case Filter.Gruen:
                        {
                            query = _context.Projekte.Include(p => p.Ratings)
                   .Where(p => (int)p.Bezirk == bezirk && ((p.VzuG == true))
                                                                                                              && (p.Bpf == false) && (p.PzuB == false)
                                                                                                              && (p.Kub == false) && (p.PentsV == false) && (p.AzuX == false) && (p.GwPI == false)
                                                                                                              && (p.PP == false) && (p.UG == false) && (p.Vbeet == false)); break;
                        }
                    case Filter.Bsv:
                        {
                            query = _context.Projekte.Include(p => p.Ratings)
                     .Where(p => (int)p.Bezirk == bezirk && (p.BSV == true)
                                                                                                                && (p.Bpf == false) && (p.PzuB == false)
                                                                                                                && (p.Kub == false) && (p.PentsV == false) && (p.AzuX == false) && (p.GwPI == false)
                                                                                                                && (p.PP == false) && (p.UG == false) && (p.Vbeet == false)
                                                                                                                && (p.VzuG == false)); break;
                        }
                    default: { query = _context.Projekte.Include(p => p.Ratings).Where(p => (int)p.Bezirk == bezirk); break; }
                }
                if ((Sortierung)sortierung == Sortierung.Datum)
                {
                    return View(await query.OrderByDescending(p => p.Datum).ToListAsync());
                }
                else if ((Sortierung)sortierung == Sortierung.Strasse)
                {
                    return View(await query.OrderBy(p => p.Strasse).ToListAsync());
                }
                else if ((Sortierung)sortierung == Sortierung.Likes)
                {
                    var projects = await query.ToListAsync();
                    projects.Sort(CompareLikes);
                    return View(projects);
                }
                return View(await query.ToListAsync());
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ExportProject>>> ExportProjects()
        {
            var query = from p in _context.Projekte
                        where p.Status == 1
                        select new ExportProject
                        {
                            Bezirk = (int)p.Bezirk,
                            Datum = p.Datum,
                            Beitragender = p.Beitragender,
                            WKTKoordinaten = p.WKTKoordinaten,
                            Strasse = p.Strasse,
                            Plz = p.Plz,
                            Bezeichnung = p.Bezeichnung,
                            Details = p.Details,
                            BSV = p.BSV,
                            Kub = p.Kub,
                            Bpf = p.Bpf,
                            PzuB = p.PzuB,
                            PentsV = p.PentsV,
                            VzuG = p.VzuG,
                            Div = p.Div,
                            Vbeet = p.Vbeet,
                            PP = p.PP,
                            UG = p.UG,
                            AzuX = p.AzuX,
                            GwPI = p.GwPI,
                            RuF = p.RuF,
                            Prio = p.Prio,
                            Status = p.Status,
                            Bilder = (from b in _context.Bilder
                                      where b.ProjectId == p.Id
                                      select b.Name.ToString()).ToArray<string>()
                        };
            return await query.ToListAsync();
        }

        [HttpGet]
        public ActionResult ImportProjects()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ImportProjects(IFormCollection form)
        {
            string jsonContent = form["jsonContent"];
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            ExportProject[] exportProjects;
            exportProjects = JsonSerializer.Deserialize<ExportProject[]>(jsonContent, options);
            Project[] projects = new Project[exportProjects.Length];
            for (int i = 0; i < projects.Length; i++)
            {
                projects[i] = new Project();
                projects[i].Bezirk = (Bezirke)exportProjects[i].Bezirk;
                projects[i].Datum = exportProjects[i].Datum;
                projects[i].Beitragender = exportProjects[i].Beitragender;
                projects[i].WKTKoordinaten = exportProjects[i].WKTKoordinaten;
                projects[i].Strasse = exportProjects[i].Strasse;
                projects[i].Plz = exportProjects[i].Plz;
                projects[i].Bezeichnung = exportProjects[i].Bezeichnung;
                projects[i].Details = exportProjects[i].Details;
                projects[i].BSV = exportProjects[i].BSV;
                projects[i].Kub = exportProjects[i].Kub;
                projects[i].Bpf = exportProjects[i].Bpf;
                projects[i].PzuB = exportProjects[i].PzuB;
                projects[i].PentsV = exportProjects[i].PentsV;
                projects[i].VzuG = exportProjects[i].VzuG;
                projects[i].Div = exportProjects[i].Div;
                projects[i].Vbeet = exportProjects[i].Vbeet;
                projects[i].PP = exportProjects[i].PP;
                projects[i].UG = exportProjects[i].UG;
                projects[i].AzuX = exportProjects[i].AzuX;
                projects[i].GwPI = exportProjects[i].GwPI;
                projects[i].RuF = exportProjects[i].RuF;
                projects[i].Prio = exportProjects[i].Prio;
                projects[i].Status = exportProjects[i].Status;

                _context.Add(projects[i]);
                await _context.SaveChangesAsync();

                Bild[] projectImages = new Bild[exportProjects[i].Bilder.Length];
                for(int j = 0; j < projectImages.Length; j++)
                {
                    projectImages[j] = new Bild();
                    projectImages[j].Name = Guid.Parse(exportProjects[i].Bilder[j]);
                    projectImages[j].ProjectId = projects[i].Id;
                }
                _context.AddRange(projectImages);
                await _context.SaveChangesAsync();


            }

            return View();
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projekte.Include(p => p.Bilder).FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            ViewBag.MapImage = GetMapImage(project);
            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Bezeichnung,Datum,WKTKoordinaten,Strasse,Plz,Details,BSV,Kub,Bpf,PzuB,PentsV,VzuG,Div,Vbeet,PP,UG,AzuX,GwPI,RuF,Status")] Project project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projekte.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Bezeichnung,Datum,Beitragender,Strasse,Plz,Details,BSV,Kub,Bpf,PzuB,PentsV,VzuG,Div,Vbeet,PP,UG,AzuX,GwPI,RuF,Status,Bezirk")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }
            //var originalProject = await _context.Projekte.FindAsync(id);
            //if (originalProject == null)
            //{
            //    return NotFound();
            //}
            //project.Koordinaten = originalProject.Koordinaten;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    _context.Entry(project).Property(p => p.Koordinaten).IsModified = false;
                    _context.Entry(project).Property(p => p.UserId).IsModified = false;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projekte
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [AllowAnonymous]
        public async Task<IActionResult> DeleteMyProject(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var project = await _context.Projekte.Where(p => p.Id == id).Include(p => p.Bilder).FirstAsync();
                if (project.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
                {
                    string[] fileNames = new string[project.Bilder.Count];
                    for (int i = 0; i < project.Bilder.Count; i++)
                    {
                        fileNames[i] = project.Bilder[i].Name.ToString();
                    }
                    _context.Projekte.Remove(project);
                    await _context.SaveChangesAsync();
                    DeleteFiles(fileNames);
                    return RedirectToAction("UserPage", "Users", new { id = User.FindFirstValue(ClaimTypes.NameIdentifier) });
                }
            }
            return NotFound();
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projekte.Where(p => p.Id == id).Include(p => p.Bilder).FirstAsync();
            string[] fileNames = new string[project.Bilder.Count];
            for (int i = 0; i < project.Bilder.Count; i++)
            {
                fileNames[i] = project.Bilder[i].Name.ToString();
            }
            _context.Projekte.Remove(project);
            await _context.SaveChangesAsync();
            DeleteFiles(fileNames);
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projekte.Any(e => e.Id == id);
        }

        private void DeleteFiles(string[] fileNames)
        {
            foreach (string guid in fileNames)
            {
                var blobClient = _blobContainerClient.GetBlobClient(guid + ".jpeg");
                if (blobClient.Exists())
                {
                    blobClient.Delete();
                }
            }
        }

        public static void DeleteFiles(IWebHostEnvironment env, Project project, BlobContainerClient blobContainerClient)
        {
            string[] fileNames = new string[project.Bilder.Count];
            for (int i = 0; i < project.Bilder.Count; i++)
            {
                fileNames[i] = project.Bilder[i].Name.ToString();
            }
            foreach (string guid in fileNames)
            {
                var blobClient = blobContainerClient.GetBlobClient(guid + ".jpeg");
                if (blobClient.Exists())
                {
                    blobClient.Delete();
                }
            }
        }
    }
}
