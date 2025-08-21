using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EORequests.Infrastructure.Data
{
    // Factory used by EF Core tools at design-time

    public class EoDbContextFactory : IDesignTimeDbContextFactory<EoDbContext>
    {
        public EoDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<EoDbContext>();

            // Use the same connection string as runtime
            optionsBuilder.UseSqlServer(
                "Server=ece-int-ms-17-sql.unog.un.org;Database=EoRequests;User Id=PortalUser;Password=AF2014999-;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new EoDbContext(optionsBuilder.Options);
        }
    }
}
