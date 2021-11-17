using Entsiegeln.Areas.Identity.Data;
using Entsiegeln.Models;
using Entsiegeln.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Entsiegeln.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<EntsiegelnUser> userManager;

        public HomeController(ILogger<HomeController> logger, UserManager<EntsiegelnUser> userManager)
        {
            _logger = logger;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Map(Bezirke bezirk)
        {
            //ViewData["Hostname"] = Program.Hostname;
            ViewData["Bezirk"] = (int)bezirk;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Impressum()
        {
            return View();
        }

        public IActionResult Help()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            EntsiegelnUser user = await userManager.FindByNameAsync(User.Identity.Name);
            ContactViewModel contactViewModel = new ContactViewModel();
            contactViewModel.Name = user.FirstName + ' ' + user.LastName;
            contactViewModel.eMail = user.Email;

            return View(contactViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage([Bind] ContactViewModel contactViewModel, EmailSender emailSender)
        {
            await emailSender.SendEmailAsync("admin@berlin-entsiegeln.de", contactViewModel.Reason, contactViewModel.eMail + "<br/>" + contactViewModel.Name + "<br/>" + contactViewModel.Message);

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
