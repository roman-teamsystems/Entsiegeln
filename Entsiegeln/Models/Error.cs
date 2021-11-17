using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Entsiegeln.Models
{
    public class Error
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int? ErrorCode { get; set; }
        [MaxLength(200)]
        public string Text { get; set; }
        public int? User { get; set; }

    }
}
