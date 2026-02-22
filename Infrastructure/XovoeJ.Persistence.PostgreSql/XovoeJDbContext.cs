using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using XovoeJ.Entities;

namespace XovoeJ.Persistence.PostgreSql
{
    public class XovoeJDbContext : IdentityDbContext<User, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        public XovoeJDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User 索引
            modelBuilder.Entity<User>(build =>
            {
                build.HasIndex(u => u.PhoneNumber);
            });


        }
    }
}
