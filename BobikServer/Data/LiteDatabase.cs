using BobikServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BobikServer.Data
{
    public class LiteDatabase : IdentityDbContext<IdentityUser>
    {

        public LiteDatabase(DbContextOptions<LiteDatabase> options) : base(options)
        {

        }

        public DbSet<AccountProfile> Accounts { get; set; }
        public DbSet<GameProfile> GameProfiles { get; set; }
        public DbSet<LoginToken> LoginTokens { get; set; }
        public DbSet<ConnectGameserverToken> ConnectGameserverTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AccountProfile>()
                .HasMany(a => a.GameProfiles)
                .WithOne(g => g.AccountProfile)
                .HasForeignKey(g => g.AccountProfileId);

            modelBuilder.Entity<AccountProfile>()
                .HasMany(a => a.LoginTokens)
                .WithOne(g => g.AccountProfile)
                .HasForeignKey(g => g.AccountId);

            modelBuilder.Entity<AccountProfile>()
                .HasMany(a => a.ConnectGameserverTokens)
                .WithOne(g => g.AccountProfile)
                .HasForeignKey(g => g.AccountId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
