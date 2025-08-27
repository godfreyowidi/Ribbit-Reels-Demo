using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using RibbitReels.Data.DTOs;

namespace RibbitReels.IntegrationTests.Branches;

// switched to in-memory - no test now touches db/azure

public class BranchFlowTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public BranchFlowTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UserCanCompleteBranchFlow()
    {
        // A. login as admin
        var adminToken = await RegisterAndLoginTestAdminAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        // B. admin creates a branch
        var branch = await CreateBranch("Introduction to Sustainable Afforestation", "Learn how to increase forest cover");

        // C. admin adds leaves
        var createdLeaf1 = await CreateLeaf(branch.Id, "What is Afforestation");
        var createdLeaf2 = await CreateLeaf(branch.Id, "Methods");

        // D. register user & get userId + token
        var (userId, userToken) = await RegisterTestUserAndReturnIdAndTokenAsync();

        // E. admin assigns branch to the user
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var assignPayload = new
        {
            UserId = userId,
            BranchId = branch.Id,
            AssignedByManagerId = await GetUserIdFromTokenAsync(adminToken)
        };
        var assignResponse = await _client.PostAsJsonAsync("/api/UserBranchAssignment/assign", assignPayload);
        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);

        // F. switch to learner context
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", userToken);

        // G. learner marks leaf as complete
        await MarkLeafComplete(branch.Id, createdLeaf1.Id);
        await MarkLeafComplete(branch.Id, createdLeaf2.Id);

        // H. check learning progress
        var progress = await GetProgress(branch.Id);
        Assert.NotNull(progress);
        Assert.Equal(branch.Id, progress.BranchId);
        Assert.Equal(2, progress.CompletedLeafIds.Count);
        Assert.True(progress.PercentageCompleted >= 100);
        Assert.NotNull(progress.CompletedAt);
    }

    private async Task<BranchResponse> CreateBranch(string title, string description)
    {
        var response = await _client.PostAsJsonAsync("/api/branch", new { Title = title, Description = description });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var branch = await response.Content.ReadFromJsonAsync<BranchResponse>();
        Assert.NotNull(branch);
        return branch!;
    }

    private async Task<LeafResponse> CreateLeaf(Guid branchId, string title)
    {
        var formData = new MultipartFormDataContent
        {
            { new StringContent(title), "Title" },
            { new StringContent("1"), "Order" },
            { new ByteArrayContent([0]), "VideoFile", "video.mp4" }
        };

        var response = await _client.PostAsync($"/api/leaf/{branchId}/manual", formData);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var leaf = await response.Content.ReadFromJsonAsync<LeafResponse>();
        Assert.NotNull(leaf);
        return leaf!;
    }

    private async Task MarkLeafComplete(Guid branchId, Guid leafId)
    {
        var payload = new { BranchId = branchId, CompletedLeafIds = new List<Guid> { leafId } };

        var response = await _client.PutAsJsonAsync("/api/UserLearningProgress/progress", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<LearningProgressResponse> GetProgress(Guid branchId)
    {
        var response = await _client.GetAsync($"/api/UserLearningProgress/progress?branchId={branchId}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var progress = await response.Content.ReadFromJsonAsync<LearningProgressResponse>();
        Assert.NotNull(progress);
        return progress!;
    }

    private async Task<(Guid userId, string token)> RegisterTestUserAndReturnIdAndTokenAsync()
    {
        var testEmail = $"user-{Guid.NewGuid()}@test.com";
        var password = "StrongP@ssw0rd!";
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = testEmail,
            Password = password,
            ConfirmPassword = password,
            DisplayName = "Assigned User"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var registerData = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var userId = Guid.Parse(registerData.GetProperty("userId").GetString()!);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = testEmail, Password = password });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        return (userId, loginResult!.Token);
    }

    private async Task<string> RegisterAndLoginTestAdminAsync()
    {
        var testEmail = $"admin-{Guid.NewGuid()}@test.com";
        var password = "AdminTest123!";

        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register-admin", new
        {
            Email = testEmail,
            DisplayName = "CI Test Admin",
            Password = password,
            ConfirmPassword = password
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new { Email = testEmail, Password = password });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(loginResult?.Token);
        return loginResult!.Token!;
    }

    private async Task<Guid> GetUserIdFromTokenAsync(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<JsonElement>();
        return Guid.Parse(data.GetProperty("id").GetString()!);
    }
}
