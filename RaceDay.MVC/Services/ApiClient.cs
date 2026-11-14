
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;

    public ApiClient(
        HttpClient http,
        IHttpContextAccessor httpContext)
    {
        _http = http;
        _httpContext = httpContext;
    }

    private ISession Session =>
        _httpContext.HttpContext!.Session;

    private void AttachCookie(HttpRequestMessage request)
    {
        var cookie = Session.GetString("ApiCookie");

        if (!string.IsNullOrWhiteSpace(cookie))
        {
            request.Headers.Add("Cookie", cookie);
        }
    }

    private void SaveCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues(
                "Set-Cookie", out var cookies))
        {
            return;
        }

        var sessionCookie = string.Join(
            "; ",
            cookies.Select(c => c.Split(';')[0]));

        Session.SetString("ApiCookie", sessionCookie);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        using var request =
            new HttpRequestMessage(HttpMethod.Get, url);

        AttachCookie(request);

        var response = await _http.SendAsync(request);

        SaveCookie(response);

        return response;
    }

    public async Task<T?> GetJsonAsync<T>(string url)
    {
        using var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string url,
        object? body = null)
    {
        using var request = new HttpRequestMessage(method, url);

        AttachCookie(request);

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        var response = await _http.SendAsync(request);

        SaveCookie(response);

        return response;
    }

    public Task<HttpResponseMessage> PostJsonAsync(
        string url,
        object body)
    {
        return SendAsync(HttpMethod.Post, url, body);
    }

    public Task<HttpResponseMessage> PutJsonAsync(
        string url,
        object body)
    {
        return SendAsync(HttpMethod.Put, url, body);
    }

    public Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return SendAsync(HttpMethod.Delete, url);
    }

    public async Task<HttpResponseMessage> UploadAsync(
        string url,
        IFormFile file,
        string fieldName = "file")
    {
        using var request =
            new HttpRequestMessage(HttpMethod.Post, url);

        using var content = new MultipartFormDataContent();

        await using var fileStream = file.OpenReadStream();

        using var streamContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(file.ContentType);
        }

        content.Add(
            streamContent,
            fieldName,
            Path.GetFileName(file.FileName));

        request.Content = content;

        AttachCookie(request);

        var response = await _http.SendAsync(request);

        SaveCookie(response);

        return response;
    }
}