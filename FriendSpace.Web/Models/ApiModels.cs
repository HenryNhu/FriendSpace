namespace FriendSpace.Web.Models
{
    public class ApiModels
    {
        public class PostItem
        {
            public Guid Id { get; set; }
            public Guid AuthorId { get; set; }
            public string Content { get; set; } = string.Empty;
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? UpdatedAt { get; set; }
        }

        public class UserInfo
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
        }

        public class LoginResult
        {
            public string AccessToken { get; set; } = string.Empty;
        }

        public class ApiProblem
        {
            public string? Message { get; set; }
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }
    }
}
