using Microsoft.EntityFrameworkCore;
using Social.Domain.Entities;
using Social.Infrastructure.Identity;

namespace Social.Infrastructure.Persistence;

public static class MediaModelConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.ToTable("media_assets");
            entity.HasKey(asset => asset.Id);
            entity.Property(asset => asset.Id).ValueGeneratedNever();
            entity.Property(asset => asset.BlobName).HasMaxLength(180).IsRequired();
            entity.Property(asset => asset.ContentType).HasMaxLength(40).IsRequired();
            entity.HasIndex(asset => asset.BlobName).IsUnique();
            entity.HasIndex(asset => asset.OwnerId).IsUnique().HasFilter("\"IsAvatar\" = TRUE");
            entity.HasIndex(asset => asset.CreatedAt).HasFilter("\"PostId\" IS NULL AND \"IsAvatar\" = FALSE");
            entity.HasOne<Post>().WithMany().HasForeignKey(asset => asset.PostId).OnDelete(DeleteBehavior.SetNull);
            // Account deletion must explicitly arrange blob cleanup first.
            entity.HasOne<ApplicationUser>().WithMany().HasForeignKey(asset => asset.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
