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
        public DbSet<Friendship> Friendships => Set<Friendship>();
        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<PostLike> PostLikes => Set<PostLike>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            MediaModelConfiguration.Configure(modelBuilder);
            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.ToTable("friendships", table =>
                {
                    table.HasCheckConstraint("ck_friendships_pair", "\"UserLowId\" < \"UserHighId\"");
                    table.HasCheckConstraint("ck_friendships_requester", "\"RequestedById\" IN (\"UserLowId\", \"UserHighId\")");
                });
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Id).ValueGeneratedNever();
                entity.HasIndex(f => new { f.UserLowId, f.UserHighId }).IsUnique();
                entity.HasIndex(f => new { f.UserHighId, f.Status });
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(f => f.UserLowId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(f => f.UserHighId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Post>(entity =>
            {
                entity.ToTable("posts");
                entity.Property(p => p.Visibility).HasDefaultValue(PostVisibility.Public);
                entity.HasIndex(p => new { p.AuthorId, p.CreatedAt, p.Id });
                entity.HasIndex(p => new { p.CreatedAt, p.Id });

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

            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.ToTable("user_profiles");

                entity.HasKey(profile => profile.UserId);

                entity.Property(profile => profile.UserId)
                    .ValueGeneratedNever();

                entity.Property(profile => profile.DisplayName)
                    .HasMaxLength(UserProfile.MaxDisplayNameLength)
                    .IsRequired();

                entity.Property(profile => profile.Bio)
                    .HasMaxLength(UserProfile.MaxBioLength)
                    .IsRequired();

                entity.HasOne<ApplicationUser>()
                    .WithOne()
                    .HasForeignKey<UserProfile>(profile => profile.UserId)
                    .HasConstraintName("fk_user_profiles_users_user_id")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
