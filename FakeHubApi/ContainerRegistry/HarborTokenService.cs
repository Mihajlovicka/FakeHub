using System.Text.Json.Serialization;
using FakeHubApi.Model.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

public interface IHarborTokenService
{
    Task<string> GenerateAndStoreHarborToken(string userId, string email, string password);
    Task<HarborCredentials> GetHarborToken(string userId);
    Task<bool> ValidateHarborToken(string token);
    Task HandleInvalidToken(string userId);
}

public class HarborTokenService : IHarborTokenService
{
    private readonly IMemoryCache _cache;
    private readonly IDataProtector _protector;
    private readonly HarborSettings _settings;

    public HarborTokenService(IMemoryCache cache, IDataProtectionProvider provider, IOptions<HarborSettings> settings)
    {
        _cache = cache;
        _protector = provider.CreateProtector("Harbor.Credentials");
        _settings = settings.Value;
    }

    public async Task<string> GenerateAndStoreHarborToken(string userId, string email, string password)
    {
        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddMinutes(_settings.Duration);

        // Encrypt the password for storage
        var encryptedPassword = _protector.Protect(password);

        var harborCredentials = new HarborCredentials
        {
            Token = token,
            Email = email,
            EncryptedPassword = encryptedPassword,
            Expiry = expiry,
            UserId = userId
        };

        // Store in cache with expiration
        _cache.Set(token, harborCredentials, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiry
        });

        // Also store by user ID for easy retrieval
        _cache.Set($"harbor_user_{userId}", token, new MemoryCacheEntryOptions
        {
            AbsoluteExpiration = expiry
        });

        return token;
    }

    public async Task<HarborCredentials> GetHarborToken(string userId)
    {
        if (_cache.TryGetValue($"harbor_user_{userId}", out string tokenId))
        {
            if (_cache.TryGetValue(tokenId, out HarborCredentials credentials))
            {
                // Decrypt the password when retrieving
                credentials.DecryptedPassword = _protector.Unprotect(credentials.EncryptedPassword);
                return credentials;
            }
        }
        return null;
    }

    public async Task<bool> ValidateHarborToken(string token)
    {
        return _cache.TryGetValue(token, out HarborCredentials _);
    }

    public async Task HandleInvalidToken(string userId)
    {
        _cache.Remove($"harbor_user_{userId}");
    }
}



public class HarborCredentials
{
    public string Token { get; set; }
    public string Email { get; set; }
    public string EncryptedPassword { get; set; }
    [JsonIgnore]
    public string DecryptedPassword { get; set; }
    public DateTime Expiry { get; set; }
    public string UserId { get; set; }
}

public class LoginResponseDto
{
    public string Token { get; set; }
    public string HarborToken { get; set; }
    public DateTime HarborTokenExpiry { get; set; }
}