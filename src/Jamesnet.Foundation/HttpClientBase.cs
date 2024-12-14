using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Jamesnet.Foundation
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RequireAuthenticationAttribute : Attribute { }

    public abstract class HttpClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly JwtTokenManager _jwtTokenManager;

        protected HttpClientBase(
            HttpClient httpClient,
            JsonSerializerOptions jsonOptions,
            JwtTokenManager jwtTokenManager)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _jsonOptions = jsonOptions ?? throw new ArgumentNullException(nameof(jsonOptions));
            _jwtTokenManager = jwtTokenManager ?? throw new ArgumentNullException(nameof(jwtTokenManager));
        }

        protected async Task<T> SendAsync<T>(HttpRequestMessage request, [CallerMemberName] string callerMethod = "")
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                await ApplyAuthenticationIfRequired(request, callerMethod);

                var response = await _httpClient.SendAsync(request);

                await HandleResponseAsync(response, callerMethod);

                var content = await response.Content.ReadAsStringAsync();

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)content;
                }

                if (string.IsNullOrEmpty(content))
                {
                    throw new InvalidOperationException("Response content is empty");
                }

                var result = JsonSerializer.Deserialize<T>(content, _jsonOptions);
                if (result == null)
                {
                    throw new InvalidOperationException("Deserialization resulted in null");
                }

                return result;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, callerMethod);
                throw;
            }
        }

        protected async Task<bool> SendAsync(HttpRequestMessage request, [CallerMemberName] string callerMethod = "")
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                await ApplyAuthenticationIfRequired(request, callerMethod);

                var response = await _httpClient.SendAsync(request);

                await HandleResponseAsync(response, callerMethod);

                return true;
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(ex, callerMethod);
                throw;
            }
        }

        protected async Task<HttpRequestMessage> CreateRequestAsync(HttpMethod method, string url, object? content = null)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));

            var request = new HttpRequestMessage(method, url);
            if (content != null)
            {
                string jsonString = JsonSerializer.Serialize(content, _jsonOptions);
                request.Content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
            }
            return request;
        }

        private async Task ApplyAuthenticationIfRequired(HttpRequestMessage request, string callerMethod)
        {
            if (IsAuthenticationRequired(callerMethod))
            {
                var token = _jwtTokenManager.GetTokenFromCookie();
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("JWT token is not set");
                }
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Trim());
            }
        }

        private async Task HandleResponseAsync(HttpResponseMessage response, string callerMethod)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (IsAuthenticationRequired(callerMethod))
                {
                    _jwtTokenManager.RemoveTokenFromCookie();
                }
                throw new UnauthorizedAccessException("Authentication failed. Token has been removed.");
            }

            response.EnsureSuccessStatusCode();
        }

        private async Task HandleExceptionAsync(Exception ex, string callerMethod)
        {
            if (ex is UnauthorizedAccessException && IsAuthenticationRequired(callerMethod))
            {
                _jwtTokenManager.RemoveTokenFromCookie();
            }
        }

        private bool IsAuthenticationRequired(string methodName)
        {
            // 현재 타입과 상속 계층 구조를 검사
            var type = GetType();
            while (type != null && type != typeof(object))
            {
                var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method != null)
                {
                    var attr = method.GetCustomAttribute<RequireAuthenticationAttribute>();
                    if (attr != null)
                    {
                        return true;
                    }
                }
                type = type.BaseType;
            }
            return false;
        }
    }
}