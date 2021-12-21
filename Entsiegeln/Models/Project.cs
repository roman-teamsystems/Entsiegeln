using Entsiegeln.Areas.Identity.Data;
using Entsiegeln.Data;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entsiegeln.Models
{
    public enum Bezirke
    {
        Mitte=1,
        FriedrichshainKreuzberg=2,
        Pankow=3,
        CharlottenburgWilmersdorf=4,
        Spandau=5,
        SteglitzZehlendorf=6,
        TempelhofSchöneberg=7,
        Neukölln=8,
        TreptowKöpenick=9,
        MarzahnHellersdorf=10,
        Lichtenberg=11,
        Reinickendorf=12
    }

    [Index(nameof(Bezirk))]
    public class Project
    {
        public int Id { get; set; }
        public Bezirke Bezirk { get; set; }
        [MaxLength(100)]
        [Display(Name = "Ortsangabe")]
        public string Bezeichnung { get; set; }

        [Required, DataType(DataType.Date)]
        [Display(Name = "Hinzugefügt am")]
        public DateTime Datum { get; set; }

        [MaxLength(200)]
        public string Beitragender { get; set; }

        [Column(TypeName = "geometry")]
        [JsonIgnore]
        public Geometry Koordinaten { get; set; }

        [NotMapped]
        public string WKTKoordinaten
        {
            get { return (this.Koordinaten != null ? EntsiegelnContext.wktwriter.Write(this.Koordinaten) : ""); }
            set { this.Koordinaten = EntsiegelnContext.wktreader.Read(value); }
        }
        [MaxLength(100)]
        [Display(Name = "Straße")]
        public string Strasse { get; set; }

        [Column(TypeName = "VARCHAR"), StringLength(5)]
        [Display(Name = "PLZ")]
        public string Plz { get; set; }

        [MaxLength(4000)]
        public string Details { get; set; }

        public List<Bild> Bilder { get; } = new List<Bild>();

        [NotMapped]
        [Display(Name ="Maßnahme")]
        public string Massnahme
        {
            get
            {
                if (Bpf || PzuB)
                {
                    return "Bäume pflanzen";
                }
                else if (Kub || PentsV || AzuX || GwPI)
                {
                    return "Bauliche Entsiegelung";
                }
                else if (PP || UG || Vbeet)
                {
                    return "Versickerungsbeet";
                }
                else if (VzuG)
                {
                    return "Grünflächen schaffen";
                }
                else if (BSV)
                {
                    return "Baumscheibenvergrößerung";
                }
                else if (Div)
                {
                    return "Fassadenbegrünung";
                }
                return "sonstiges";
            }
            set { }
        }
        public bool BSV { get; set; } = false;
        public bool Kub { get; set; } = false;
        public bool Bpf { get; set; } = false;
        public bool PzuB { get; set; } = false;
        public bool PentsV { get; set; } = false;
        public bool VzuG { get; set; } = false;
        public bool Div { get; set; } = false;
        public bool Vbeet { get; set; } = false;
        public bool PP { get; set; } = false;
        public bool UG { get; set; } = false;
        public bool AzuX { get; set; } = false;
        public bool GwPI { get; set; } = false;
        public bool RuF { get; set; } = false;
        public byte Prio { get; set; } = 0;
        public byte Status { get; set; } = 0;
        public string UserId { get; set; }
        public EntsiegelnUser User { get; set; }
        [NotMapped]
        [JsonIgnore]
        [Display(Name = "Likes")]
        public int Pro
        {
            get
            {
                if (Ratings == null)
                {
                    return 0;
                }
                else
                {
                    return Ratings.Where(r => (r.Like == true)).Count();
                }
            }
        }
        [NotMapped]
        [JsonIgnore]
        [Display(Name = "Dislikes")]
        public int Contra
        {
            get
            {
                if (Ratings == null)
                {
                    return 0;
                }
                else
                {
                    return Ratings.Where(r => (r.Like == false)).Count();
                }
            }
        }

        public List<Rating> Ratings { get; } = new List<Rating>();
    }

    public class Bild
    {
        public int Id { get; set; }
        [Required]
        public Guid Name { get; set; }
        public int ProjectId { get; set; }
        //public Project Project { get; set; }
    }
}
