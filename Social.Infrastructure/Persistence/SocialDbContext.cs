using Microsoft.EntityFrameworkCore;
using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence
{
    public class SocialDbContext : DbContext
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { 
            
        }

        public DbSet<Post> Posts => Set<Post>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");

                entity.HasKey(post => post.Id);

                entity.Property(post => post.Id).ValueGeneratedNever();

                entity.Property(post => post.Content).IsRequired();
            });
        }
    }
}
