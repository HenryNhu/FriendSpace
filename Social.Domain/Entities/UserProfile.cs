using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Domain.Entities
{
    public class UserProfile
    {
        public const int MaxDisplayNameLength = 80;
        public const int MaxBioLength = 500;

        public Guid UserId { get; private set; }

        public string DisplayName { get; private set; } = string.Empty;

        public string Bio { get; private set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; private set; }

        public DateTimeOffset? UpdatedAt { get; private set; }

        private UserProfile()
        {
        }

        public UserProfile(Guid userId, string displayName, string? bio)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Người dùng không hợp lệ.", nameof(userId));
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException("Tên hiển thị không được để trống.", nameof(displayName));
            }

            var normalizedName = displayName.Trim();
            var normalizedBio = bio?.Trim() ?? string.Empty;

            if (normalizedName.Length < 2 || normalizedName.Length > MaxDisplayNameLength)
            {
                throw new ArgumentException($"Tên hiển thị phải từ 2 đến {MaxDisplayNameLength} ký tự.", nameof(displayName));
            }

            if (normalizedBio.Length > MaxBioLength)
            {
                throw new ArgumentException($"Giới thiệu không được vượt quá {MaxBioLength} ký tự.", nameof(bio));
            }

            UserId = userId;
            DisplayName = normalizedName;
            Bio = normalizedBio;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
