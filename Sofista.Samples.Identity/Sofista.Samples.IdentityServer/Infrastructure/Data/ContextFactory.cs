using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sofista.Samples.IdentityServer.Infrastructure.Data
{
    public class ContextFactory : IDesignTimeDbContextFactory<SofistaDbContext>
    {
        public SofistaDbContext CreateDbContext()
        {
            return CreateDbContext(null);
        }

        public SofistaDbContext CreateDbContext(string[] args)
        {
            var builderConfiguration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json");
            var configuration = builderConfiguration.Build();
            var connectionString = configuration.GetConnectionString("IdentityServer");

            var builder = new DbContextOptionsBuilder<SofistaDbContext>();
            builder.UseSqlServer(connectionString);

            return new SofistaDbContext(builder.Options);
        }
    }
}
