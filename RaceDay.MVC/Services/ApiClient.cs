using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.MVC.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContext;

    public ApiClient(HttpClient http, IHttpContextAccessor httpContext)
    {
        _http = http;
        _httpContext = httpContext;
    }

    private ISession Session => _httpContext.HttpContext!.Session;

    private void AttachCookie(HttpRequestMessage request)
    {
        var cookie = Session.GetString("ApiCookie");

        if (!string.IsNullOrWhiteSpace(cookie))
            request.Headers.Add("Cookie", cookie);
    }

    private void SaveCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return;

        var sessionCookie = string.Join("; ", cookies.Select(c => c.Split(';')[0]));
        Session.SetString("ApiCookie", sessionCookie);
    }

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        AttachCookie(request);

        var response = await _http.SendAsync(request);
        SaveCookie(response);
        return response;
    }

    public async Task<T?> GetJsonAsync<T>(string url)
    {
        var response = await GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return default;

        return await response.Content.ReadFromJsonAsync<T>();
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        AttachCookie(request);

        if (body != null)
            request.Content = JsonContent.Create(body);

        var response = await _http.SendAsync(request);
        SaveCookie(response);
        return response;
    }

    public Task<HttpResponseMessage> PostJsonAsync(string url, object body)
        => SendAsync(HttpMethod.Post, url, body);

    public Task<HttpResponseMessage> PutJsonAsync(string url, object body)
        => SendAsync(HttpMethod.Put, url, body);

    public Task<HttpResponseMessage> DeleteAsync(string url)
        => SendAsync(HttpMethod.Delete, url);

    public async Task<HttpResponseMessage> UploadAsync(
        string url, IFormFile file, string fieldName = "file")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        AttachCookie(request);

        var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(file.OpenReadStream());

        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(file.ContentType);

        content.Add(streamContent, fieldName, file.FileName);
        request.Content = content;

        var response = await _http.SendAsync(request);
        SaveCookie(response);
        return response;
    }
}
