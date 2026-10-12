using System;
using System.Collections.Generic;
using System.Text;

namespace Social.Application.UserProfiles
{
    public class UserProfileResponse
    {
        public Guid UserId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Bio { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
