using System.Net;
using System.Text.Json;
using RibbitReels.Data.DTOs;
using RibbitReels.Services.Shared;

namespace RibbitReels.Services.Implementations;

public class UdemyRepository
{
    private readonly HttpClient _http;

    public UdemyRepository(HttpClient http)
    {
        _http = http;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<OperationResult<UdemyCourseListResponse>> SearchCoursesAsync(string q, int page = 1, int pageSize = 6)
    {
        if (string.IsNullOrWhiteSpace(q))
            return OperationResult<UdemyCourseListResponse>.Fail("Query cannot be empty.", HttpStatusCode.BadRequest);
        
        var url = $"courses/?search={Uri.EscapeDataString(q)}&page={page}&page_size={pageSize}";

        var res = await _http.GetAsync(url);
        if (!res.IsSuccessStatusCode)
            return OperationResult<UdemyCourseListResponse>.Fail($"Udemy API error: {res.StatusCode}", res.StatusCode);

        var body = await res.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<UdemyCourseListResponse>(body, _jsonOptions)
                   ?? new UdemyCourseListResponse();

        return OperationResult<UdemyCourseListResponse>.Success(data, HttpStatusCode.OK);
    }

    public async Task<OperationResult<UdemyCourse>> GetCourseAsync(int courseId)
    {
        var res = await _http.GetAsync($"courses/{courseId}/");
        if (!res.IsSuccessStatusCode)
            return OperationResult<UdemyCourse>.Fail($"Udemy API error: {res.StatusCode}", res.StatusCode);

        var body = await res.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<UdemyCourse>(body, _jsonOptions);
        if (data == null) return OperationResult<UdemyCourse>.Fail("Course not found.", HttpStatusCode.NotFound);

        return OperationResult<UdemyCourse>.Success(data, HttpStatusCode.OK);
    }
}