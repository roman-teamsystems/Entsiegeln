using System;
using Entsiegeln.Areas.Identity.Data;
using Entsiegeln.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(Entsiegeln.Areas.Identity.IdentityHostingStartup))]
namespace Entsiegeln.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<EntsiegelnContext>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("EntsiegelnContextConnection"), x => x.UseNetTopologySuite()));

                services.AddDefaultIdentity<EntsiegelnUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<EntsiegelnContext>();
            });
        }
    }
}