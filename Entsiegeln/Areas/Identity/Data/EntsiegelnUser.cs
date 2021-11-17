using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Entsiegeln.Models;
using Microsoft.AspNetCore.Identity;

namespace Entsiegeln.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the EntsiegelnUser class
    public class EntsiegelnUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        [Display(Name = "Vorname")]
        public string FirstName { get; set; }
        [PersonalData]
        [StringLength(100)]
        [Display(Name = "Nachname")]
        public string LastName { get; set; }
        [PersonalData]
        [StringLength(100)]
        [Display(Name = "Straße")]
        public string Street { get; set; }
        [PersonalData]
        [StringLength(5)]
        [Display(Name = "PLZ")]
        public string Zip { get; set; }
        [PersonalData]
        [StringLength(100)]
        [Display(Name = "Ort")]
        public string City { get; set; }
        public int? MaxNumberOfFavorites { get; set; } = 10;
        [ForeignKey("UserId")]
        public List<Project> Projects { get; } = new List<Project>();
        [ForeignKey("UserId")]
        public List<Rating> Ratings { get; } = new List<Rating>();
    }
}
