using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Social.Domain.Entities;
using Social.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Infrastructure.Persistence
{
    public class SocialDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options)
        { 
            
        }

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");

                entity.HasKey(post => post.Id);

                entity.Property(post => post.Id)
                        .ValueGeneratedNever();

                entity.Property(post => post.Content)
                        .IsRequired();
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.ToTable("comments");

                entity.HasKey(comment => comment.Id);

                entity.Property(comment => comment.Id)
                        .ValueGeneratedNever();

                entity.Property(comment => comment.Content)
                        .HasMaxLength(Comment.MaxContentLength)
                        .IsRequired();

                entity.HasOne<Post>()
                        .WithMany()
                        .HasForeignKey(comment => comment.PostId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PostLike>(entity =>
            {
                entity.ToTable("post_likes");

                entity.HasKey(like => new
                {
                    like.PostId,
                    like.UserId
                });

                entity.Property(like => like.PostId)
                        .HasColumnName("PostId")
                        .ValueGeneratedNever();

                entity.Property(like => like.UserId)
                        .HasColumnName("UserId")
                        .ValueGeneratedNever();

                entity.Property(like => like.CreatedAt)
                        .HasColumnName("CreatedAt");

                entity.HasOne<Post>()
                        .WithMany()
                        .HasForeignKey(like => like.PostId)
                        .HasConstraintName("fk_post_likes_posts_post_id")
                        .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
