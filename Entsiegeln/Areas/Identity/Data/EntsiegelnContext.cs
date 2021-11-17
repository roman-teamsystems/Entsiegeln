using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entsiegeln.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Entsiegeln.Models;

namespace Entsiegeln.Data
{
    public partial class EntsiegelnContext : IdentityDbContext<EntsiegelnUser>
    {
        public EntsiegelnContext(DbContextOptions<EntsiegelnContext> options)
            : base(options)
        {
        }
    }
}
