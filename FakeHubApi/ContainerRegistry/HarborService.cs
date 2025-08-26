namespace FakeHubApi.ContainerRegistry;

using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using FakeHubApi.Model.Settings;
using Microsoft.Extensions.Options;

public interface IHarborService
{
    Task<bool> createUser(HarborUser user);
    Task<bool> updateUser(int id, HarborUserUpdate user);
    Task<bool> updatePassword(int id, HarborUserPassword user);
    Task<int?> getUserId(string username);
    Task<bool?> projectNameExists(string projectName);
    Task<bool> createUpdateProject(HarborProjectCreate project, string projectName = "", bool isUpdate = false);
    Task<bool> deleteProject(string projectName, string repositoryName);
    Task<bool> addMember(string projectName, HarborProjectMember member);
    Task<bool> removeMembersByRole(string projectName, int role);
    Task<bool> removeMembers(string projectName, IEnumerable<int> memberIds);
    Task<bool> removeMemberFromTeam(string projectName, string username);
    Task<List<HarborArtifact>> GetTags(string projectName, string repositoryName);

}

public class HarborService : IHarborService
{
    private readonly HttpClient _httpClient;
    private readonly HarborSettings _settings;
    private const string Users = "users/";
    private const string Projects = "projects/";
    private const string Repositories = "repositories/";
    private const string Members = "members/";
    private const string Artifacts = "artifacts";

    private readonly ILogger<HarborService> _logger;
    private string _csrfToken = string.Empty;

    public HarborService(HttpClient httpClient, IOptions<HarborSettings> settings, ILogger<HarborService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_settings.Url);

        var authToken = Encoding.ASCII.GetBytes($"{_settings.Username}:{_settings.Password}");
        _httpClient.DefaultRequestHeaders.Remove("Cookie");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authToken));
    }

    public async Task<(bool Success, TResponse? Response, HttpStatusCode? StatusCode, string? Error)> SendRequestAsync<TResponse>(
        HttpMethod method,
        string url,
        object? payload = null,
        bool expectResponse = true) where TResponse : class
    {

        _httpClient.DefaultRequestHeaders.Remove("Cookie");

        var request = new HttpRequestMessage(method, url);
        if (_csrfToken != null)
        {
            request.Headers.Add("X-Harbor-CSRF-Token", _csrfToken);
        }

        try
        {
            if (payload != null)
            {
                var jsonContent = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });
                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            if (response.Headers.TryGetValues("X-Harbor-CSRF-Token", out var tokenValues))
            {
                _csrfToken = tokenValues.FirstOrDefault() ?? string.Empty;
            }

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Harbor API call failed. Status: {response.StatusCode}. Response: {responseContent}");
                return (false, null, response.StatusCode, responseContent);
            }

            _logger.LogInformation($"Harbor API call succeeded. Status: {response.StatusCode}");

            if (!expectResponse || string.IsNullOrWhiteSpace(responseContent))
            {
                return (true, null, null, null);
            }

            try
            {
                var result = JsonSerializer.Deserialize<TResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return (true, result, null, null);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to deserialize response from {url}");
                return (false, null, null, "Invalid response format");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception during API call to {url}");
            return (false, null, null, ex.Message);
        }
    }

    public async Task<bool> createUser(HarborUser user)
    {
        (bool success, _, _, _) = await SendRequestAsync<object>(HttpMethod.Post, Users, user, false);
        return success;
    }

    public async Task<bool> updateUser(int id, HarborUserUpdate user)
    {
        (bool success, _, _, _) = await SendRequestAsync<object>(HttpMethod.Put, $"{Users}{id}", user);
        return success;
    }

    public async Task<bool> updatePassword(int id, HarborUserPassword user)
    {
        (bool success, _, _, _) = await SendRequestAsync<object>(HttpMethod.Put, $"{Users}{id}/password", user);
        return success;
    }

    public async Task<int?> getUserId(string username)
    {
        var (success, users, _, _) = await SendRequestAsync<List<HarborUser>>(HttpMethod.Get, $"{Users}search?page=1&page_size=10&username={Uri.EscapeDataString(username)}", null, false);
        if (success && users != null && users.Count > 0)
        {
            return users[0].UserId;
        }
        return null;
    }

    public async Task<bool?> projectNameExists(string projectName)
    {
        var (success, _, status, _) = await SendRequestAsync<object>(HttpMethod.Head, $"{Projects}?project_name={Uri.EscapeDataString(projectName)}");
        return success ? success : status == HttpStatusCode.NotFound ? false : null;
    }

    public async Task<bool> createUpdateProject(HarborProjectCreate project, string projectName = "", bool isUpdate = false)
    {
        (bool success, _, _, _) = await SendRequestAsync<object>(isUpdate ? HttpMethod.Put : HttpMethod.Post, isUpdate ? $"{Projects}{projectName}" : Projects, project, false);
        return success;
    }

    public async Task<bool> deleteProject(string projectName, string repositoryName)
    {
        var (listSuccess, repositories, _, _) = await SendRequestAsync<List<JsonElement>>(HttpMethod.Get, $"{Projects}{projectName}/repositories");
        if (!listSuccess || repositories == null) return false;

        if(repositories?.Count > 0)
        {
            var (deleteRepoSuccess, _, _, _) = await SendRequestAsync<object>(HttpMethod.Delete, $"{Projects}{projectName}/repositories/{repositoryName}");
            if (!deleteRepoSuccess)
            {
                _logger.LogWarning($"Failed to delete repository {repositoryName} in project {projectName}");
                return false; 

            }
        }

        var (deleteProjectSuccess, _, _, _) = await SendRequestAsync<object>(HttpMethod.Delete, $"{Projects}{projectName}");
        return deleteProjectSuccess;
    }

    public async Task<bool> addMember(string projectName, HarborProjectMember member)
    {
        (bool success, _, _, _) = await SendRequestAsync<object>(HttpMethod.Post, $"{Projects}{projectName}/{Members}", member);
        return success;
    }

    public async Task<bool> removeMembersByRole(string projectName, int role)
    {
        var (success, members, _, _) = await SendRequestAsync<List<HarborProjectMemberGet>>(HttpMethod.Get, $"{Projects}{projectName}/{Members}");
        if (!success || members == null || members.Count == 0) return false;

        var memberIds = members.Where(m => m.RoleId == role).Select(m => m.Id).ToList();
        return await removeMembers(projectName, memberIds);
    }

    public async Task<bool> removeMemberFromTeam(string projectName, string username)
    {
        var (success, members, _, _) = await SendRequestAsync<List<HarborProjectMemberGet>>(HttpMethod.Get, $"{Projects}{projectName}/{Members}");
        if (!success || members == null || members.Count == 0) return false;

        var userId = members.FirstOrDefault(m => m.EntityName.Equals(username, StringComparison.OrdinalIgnoreCase))?.Id;
        if (userId == null) return false;

        return await removeMembers(projectName, new List<int> { userId.Value });
    }

    public async Task<bool> removeMembers(string projectName, IEnumerable<int> memberIds)
    {
        foreach (var memberId in memberIds)
        {
            var (success, _, _, _) = await SendRequestAsync<object>(HttpMethod.Delete, $"{Projects}{projectName}/{Members}{memberId}");
            if (!success) return false;
        }
        return true;
    }

    public async Task<List<HarborArtifact>> GetTags(string projectName, string repositoryName)
    {
        var (success, artifacts, _, _) = await SendRequestAsync<List<HarborArtifact>>(HttpMethod.Get, $"{Projects}{projectName}/{Repositories}{repositoryName}/{Artifacts}?page=1&page_size=10&with_tag=true&with_label=false&with_scan_overview=false&with_sbom_overview=false&with_immutable_status=false&with_accessory=false");
        return success ? artifacts : new List<HarborArtifact>();
    }
}


public class HarborUserUpdate
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("realname")]
    public string Realname { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
}

public class HarborUserPassword
{
    [JsonPropertyName("old_password")]
    public string OldPassword { get; set; } = string.Empty;

    [JsonPropertyName("new_password")]
    public string NewPassword { get; set; } = string.Empty;
}

public class HarborUser
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; } = 0;

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("realname")]
    public string Realname { get; set; } = string.Empty;

    [JsonPropertyName("comment")]
    public string Comment { get; set; } = string.Empty;
}

public class HarborProjectCreate
{
    [JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("public")]
    public bool Public { get; set; } = false;
}


public enum HarborRoles
{
    Admin = 1,
    Developer,
    Guest
}

public class HarborProjectMemberGet
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("entity_id")]
    public int EntityId { get; set; }

    [JsonPropertyName("entity_name")]
    public string EntityName { get; set; } = string.Empty;

}
public class HarborProjectMember
{
    [JsonPropertyName("role_id")]
    public int RoleId { get; set; }

    [JsonPropertyName("member_user")]
    public HarborProjectMemberUser MemberUser { get; set; } = new HarborProjectMemberUser();
}

public class HarborProjectMemberUser
{
    [JsonPropertyName("user_id")]
    public int UserId { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
}

public class HarborTag
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("push_time")]
    public DateTime? PushTime { get; set; } = null;

    [JsonPropertyName("pull_time")]
    public DateTime? PullTime { get; set; } = null;
}

public class HarborArtifact
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("tags")]
    public List<HarborTag> Tags { get; set; } = new List<HarborTag>();

    [JsonPropertyName("repository_name")]
    public string RepositoryName { get; set; } = string.Empty;

    [JsonPropertyName("extra_attrs")]
    public ExtraAttrs ExtraAttrs { get; set; } = new ExtraAttrs();
}

public class ExtraAttrs
{
    [JsonPropertyName("os")]
    public string Os { get; set; } = string.Empty;
}