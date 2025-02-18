using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fintrack_database.Entities
{
    public class FinTrackDatabaseContext : DbContext
    {
        public FinTrackDatabaseContext()
        {
        }

        public FinTrackDatabaseContext(DbContextOptions<FinTrackDatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation("utf8mb4_hungarian_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");

                entity.HasOne(e => e.User)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(e => e.UserId)
                    .HasConstraintName("FK_Category_User")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.ParentCategory)
                    .WithMany(e => e.ChildCategories)
                    .OnDelete(DeleteBehavior.ClientCascade)
                    .HasConstraintName("FK_Category_Category");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PRIMARY");
            });
        }
    }
}
