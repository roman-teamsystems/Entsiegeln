using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entsiegeln.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Entsiegeln.Data
{
    public partial class EntsiegelnContext : IdentityDbContext<EntsiegelnUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<EntsiegelnUser>().HasMany(e => e.Projects).WithOne(e => e.User).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Project>().HasMany(e => e.Ratings).WithOne(e => e.Project).OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Project> Projekte { get; set; }
        public DbSet<Bild> Bilder { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<Preferences> Preferences { get; set; }
        public DbSet<Error> Errors { get; set; }

        public static NetTopologySuite.IO.WKTWriter wktwriter = new NetTopologySuite.IO.WKTWriter();
        public static NetTopologySuite.IO.WKTReader wktreader = new NetTopologySuite.IO.WKTReader();
    }

    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<EntsiegelnContext>();
            context.Database.Migrate();
            //if (!context.Projekte.Any())
            //{
            //    Project projekt = new Project();
            //    projekt.Bezeichnung = "Test";
            //    projekt.Datum = DateTime.Now;
            //    projekt.Koordinaten = new NetTopologySuite.Geometries.Point(13.4053451832479, 52.498416259514734);
            //    context.Projekte.Add(projekt);
            //    context.SaveChanges();
            //}

            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            IdentityRole admin = new IdentityRole { Name = "Admin" };
            IdentityRole manager = new IdentityRole { Name = "Manager" };
            IdentityRole user = new IdentityRole { Name = "User" };

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(admin);
            }
            if (!await roleManager.RoleExistsAsync("Manager"))
            {
                await roleManager.CreateAsync(manager);
            }
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(user);
            }

            var userManager = serviceProvider.GetService<UserManager<EntsiegelnUser>>();
            var ich = await userManager.FindByEmailAsync("crispin.schroeder@hotmail.com");
            if (ich != null)
            {
                if (!await userManager.IsInRoleAsync(ich, "Admin"))
                {
                    await userManager.AddToRoleAsync(ich, "Admin");
                }
            }
        }
    }
}
