using Entsiegeln.Areas.Identity.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Entsiegeln.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public EntsiegelnUser User { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public bool? Like { get; set; }
        public bool? Favorite { get; set; }
    }
}
