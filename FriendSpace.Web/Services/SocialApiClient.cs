using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using static FriendSpace.Web.Models.ApiModels;

namespace FriendSpace.Web.Services
{
    public class SocialApiClient : IDisposable
    {
        private readonly HttpClient _http;
        private string? _accessToken;

        public UserInfo? CurrentUser { get; private set; }

        public bool IsLoggedIn => CurrentUser is not null;

        public SocialApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("SocialApi");
        }

        public async Task RegisterAsync(string email, string password)
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                "api/auth/register",
                new { email, password });
        }

        public async Task LoginAsync(string email, string password)
        {
            Logout();

            using var response = await SendAsync(
                HttpMethod.Post,
                "api/auth/login?useCookies=false",
                new { email, password });

            var result = await response.Content
                .ReadFromJsonAsync<LoginResult>();

            if (result is null ||
                string.IsNullOrWhiteSpace(result.AccessToken))
            {
                throw new InvalidOperationException(
                    "API chưa trả về thông tin đăng nhập hợp lệ.");
            }

            _accessToken = result.AccessToken;

            try
            {
                using var meResponse = await SendAsync(
                    HttpMethod.Get,
                    "api/auth/me",
                    authenticated: true);

                CurrentUser = await meResponse.Content
                    .ReadFromJsonAsync<UserInfo>()
                    ?? throw new InvalidOperationException(
                        "Không đọc được thông tin tài khoản.");
            }
            catch
            {
                Logout();
                throw;
            }
        }

        public void Logout()
        {
            _accessToken = null;
            CurrentUser = null;
        }

        public async Task<List<PostItem>> GetPostsAsync(
            int pageNumber,
            int pageSize)
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                $"api/posts?pageNumber={pageNumber}&pageSize={pageSize}");

            return await response.Content
                .ReadFromJsonAsync<List<PostItem>>()
                ?? new List<PostItem>();
        }

        public async Task CreatePostAsync(string content)
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                "api/posts",
                new { content },
                authenticated: true);
        }

        public async Task UpdatePostAsync(Guid id, string content)
        {
            using var response = await SendAsync(
                HttpMethod.Put,
                $"api/posts/{id}/content",
                new { content },
                authenticated: true);
        }

        public async Task DeletePostAsync(Guid id)
        {
            using var response = await SendAsync(
                HttpMethod.Delete,
                $"api/posts/{id}",
                authenticated: true);
        }

        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string path,
            object? body = null,
            bool authenticated = false)
        {
            using var request = new HttpRequestMessage(method, path);

            if (body is not null)
            {
                request.Content = JsonContent.Create(body);
            }

            if (authenticated)
            {
                if (string.IsNullOrWhiteSpace(_accessToken))
                {
                    throw new InvalidOperationException(
                        "Bạn cần đăng nhập để thực hiện thao tác này.");
                }

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            HttpResponseMessage response;

            try
            {
                response = await _http.SendAsync(request);
            }
            catch (HttpRequestException exception)
            {
                throw new InvalidOperationException(
                    "Không kết nối được API. Hãy kiểm tra Social.Api đang chạy.",
                    exception);
            }

            try
            {
                await EnsureSuccessAsync(response);
                return response;
            }
            catch
            {
                response.Dispose();
                throw;
            }
        }

        private async Task EnsureSuccessAsync(
            HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                Logout();

                throw new InvalidOperationException(
                    "Phiên đăng nhập đã hết hạn hoặc email/mật khẩu không đúng.");
            }

            var message =
                $"Không thực hiện được yêu cầu (HTTP {(int)response.StatusCode}).";

            try
            {
                var problem = await response.Content
                    .ReadFromJsonAsync<ApiProblem>();

                if (problem?.Errors is { Count: > 0 })
                {
                    message = string.Join(
                        " ",
                        problem.Errors.Values.SelectMany(values => values));
                }
                else
                {
                    message = problem?.Message
                        ?? problem?.Detail
                        ?? problem?.Title
                        ?? message;
                }
            }
            catch (JsonException)
            {
                // Giữ thông báo mặc định nếu phản hồi không phải JSON.
            }

            throw new InvalidOperationException(message);
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
