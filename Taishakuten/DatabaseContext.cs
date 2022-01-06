using Microsoft.EntityFrameworkCore;
using Taishakuten.Entities;

namespace Taishakuten
{
    class DatabaseContext : DbContext
    {
        public DbSet<Reminder> Reminders { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }
    }
}
