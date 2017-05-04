using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IO;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class DatabaseConfigContext : DbContext
    {
        public virtual DbSet<Account> Account { get; set; }

        public DatabaseConfigContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            var dbFile = Path.Combine(profileConfigPath, "config.db");

            optionsBuilder.UseSqlite($"data source={dbFile}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}