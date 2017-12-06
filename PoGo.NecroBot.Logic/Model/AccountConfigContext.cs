using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.IO;

namespace PoGo.NecroBot.Logic.Model
{
    public partial class AccountConfigContext : DbContext
    {
        public virtual DbSet<Account> Account { get; set; }
        public virtual DbSet<PokemonTimestamp> PokemonTimestamp { get; set; }
        public virtual DbSet<PokestopTimestamp> PokestopTimestamp { get; set; }

        public AccountConfigContext()
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            if (!Directory.Exists(profileConfigPath))
                Directory.CreateDirectory(profileConfigPath);
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var profilePath = Path.Combine(Directory.GetCurrentDirectory());
            var profileConfigPath = Path.Combine(profilePath, "config");
            var dbFile = Path.Combine(profileConfigPath, "accounts.db");

            optionsBuilder.UseSqlite($"data source={dbFile}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PokemonTimestamp>()
                .HasOne(p => p.Account)
                .WithMany(b => b.PokemonTimestamp)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<PokestopTimestamp>()
                .HasOne(p => p.Account)
                .WithMany(b => b.PokestopTimestamp)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
