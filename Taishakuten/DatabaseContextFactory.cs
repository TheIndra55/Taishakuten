using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Taishakuten
{
    // used for EFCore tools
    class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var connectionString = args[0];
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseMySql(connectionString, serverVersion)
                .Options;

            return new DatabaseContext(options);
        }
    }
}
