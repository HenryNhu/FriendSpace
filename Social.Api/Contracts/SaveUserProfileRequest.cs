namespace Social.Api.Contracts
{
    public class SaveUserProfileRequest
    {
        public string DisplayName { get; set; } = string.Empty;

        public string? Bio { get; set; }
    }
}
