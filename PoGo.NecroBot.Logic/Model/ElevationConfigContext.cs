using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class ElevationConfigContext : DbContext
    {
        public virtual DbSet<ElevationLocation> ElevationLocation { get; set; }

        public ElevationConfigContext()
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "Cache");
            if (!Directory.Exists(profileConfigPath))
                Directory.CreateDirectory(profileConfigPath);
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "Cache");
            var dbFile = Path.Combine(profileConfigPath, "elevations.db");

            optionsBuilder.UseSqlite($"data source={dbFile}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
