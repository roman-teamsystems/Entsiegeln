using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Entsiegeln.Models
{
    public class Preferences
    {
        public int Id { get; set; }
        [Display(Name ="Bäume pflanzen")]
        public string Icon1 { get; set; }
        [Display(Name = "Baumscheibenvergrößerung")]
        public string Icon2 { get; set; }
        [Display(Name = "Versickerungsbeet")]
        public string Icon3 { get; set; }
        [Display(Name = "Bauliche Entsiegelung")]
        public string Icon4 { get; set; }
        [Display(Name = "Grünflächen schaffen")]
        public string Icon5 { get; set; }
        [Display(Name = "Fassadenbegrünung")]
        public string Icon6 { get; set; }
    }
}
