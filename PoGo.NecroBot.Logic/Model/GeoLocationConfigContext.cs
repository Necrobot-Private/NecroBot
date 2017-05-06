using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class GeoLocationConfigContext : DbContext
    {
        public virtual DbSet<GeoLocation> GeoLocation { get; set; }

        public GeoLocationConfigContext()
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
            var dbFile = Path.Combine(profileConfigPath, "geolocation.db");

            optionsBuilder.UseSqlite($"data source={dbFile}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}
