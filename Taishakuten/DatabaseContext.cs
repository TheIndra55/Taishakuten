using Microsoft.EntityFrameworkCore;
using System;
using Taishakuten.Entities;

namespace Taishakuten
{
    class DatabaseContext : DbContext
    {
        public DbSet<Reminder> Reminders { get; set; }

        private string _connectionString;

        // empty constructor for EF Core Tools
        public DatabaseContext() { _connectionString = "server=localhost;database=taishakuten;user=indra"; }

        public DatabaseContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //if (_connectionString.IndexOf("Data Source=") == 0)
            //{
            //    optionsBuilder.UseSqlite(_connectionString);
            //    return;
            //}
            if (_connectionString.IndexOf("server=") == 0)
            {
                optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
                return;
            }

            throw new InvalidOperationException("No database provider for connection string");
        }
    }
}
