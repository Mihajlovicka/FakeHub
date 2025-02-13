using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Mysqlx.Crud;
using Newtonsoft.Json;

namespace FakeHubApi.Tests.Repositories.Tests
{
    public class RepositoryControllerIntegrationTests
    {
        private HttpClient _client;
        private CustomWebApplicationFactory _factory;
        private string _regularUserToken;
        private string _regularUser1Token;
        private string _adminToken;
        private string _superAdminToken;

        [OneTimeSetUp]
        public async Task Setup()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            await SetupDbData();
            await InitializeTokens();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test, Order(1)]
        public async Task Register_WithoutBearerToken_ReturnsUnauthorized()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "Test Repo",
                Description = "Test repository description.",
                IsPrivate = false,
                OwnedBy = RepositoryOwnedBy.User,
                OwnerId = 1
            };

            _client.DefaultRequestHeaders.Authorization = null;

            var response = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test, Order(2)]
        public async Task Register_WithAdminUserToken_ReturnsOk()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "Admin Repository",
                Description = "Admin repository description.",
                IsPrivate = true
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var response = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            var responseRepositoryString = responseObj?.Result?.ToString() ?? string.Empty;

            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseRepositoryString, Is.Not.Empty);

            var responseRepositoryObject = JsonConvert.DeserializeObject<Model.Entity.Repository>(responseRepositoryString);

            Assert.That(responseRepositoryObject?.Badge, Is.EqualTo(Badge.DockerOfficialImage));
            Assert.That(responseRepositoryObject?.OwnedBy, Is.EqualTo(RepositoryOwnedBy.Admin));
            Assert.That(responseRepositoryObject?.OwnerId, Is.GreaterThanOrEqualTo(0));
        }

        [Test, Order(3)]
        public async Task Register_WithSuperAdminUserToken_ReturnsOk()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "SuperAdmin Repository",
                Description = "SuperAdmin repository description.",
                IsPrivate = false
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);
            var response = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            var responseRepositoryString = responseObj?.Result?.ToString() ?? string.Empty;

            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseRepositoryString, Is.Not.Empty);

            var responseRepositoryObject = JsonConvert.DeserializeObject<Model.Entity.Repository>(responseRepositoryString);

            Assert.That(responseRepositoryObject?.Badge, Is.EqualTo(Badge.DockerOfficialImage));
            Assert.That(responseRepositoryObject?.OwnedBy, Is.EqualTo(RepositoryOwnedBy.SuperAdmin));
            Assert.That(responseRepositoryObject?.OwnerId, Is.GreaterThanOrEqualTo(0));
        }

        [Test, Order(4)]
        public async Task GetRepositories_WithoutToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.GetAsync("/api/repositories/all");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test, Order(5)]
        public async Task GetRepositories_AsUser_ReturnsEmptyList()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUser1Token);
            var response = await _client.GetAsync("/api/repositories/all");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.EqualTo(0));
        }

        [Test, Order(6)]
        public async Task GetRepositories_AsAdmin_ReturnsAllRepositories()
        {
            using var scope = _factory.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var repositoriesCount = db.Repositories.Count();

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var response = await _client.GetAsync("/api/repositories/all");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.EqualTo(repositoriesCount));
        }

        [Test, Order(7)]
        public async Task GetVisibleRepositories_AsUser_ReturnsPublicAdminRepositories()
        {
            var username = "superadmin@fakehub.com";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
            var response = await _client.GetAsync($"/api/repositories/all/{username}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.GreaterThanOrEqualTo(0));
            Assert.That(repositories, Is.All.Matches<RepositoryDto>(r => r.IsPrivate == false));
        }

        [Test, Order(8)]
        public async Task GetVisibleRepositories_AsCurrentUser_ReturnsAllUserRepositories()
        {
            var username = "user@fakehub.com";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
            var response = await _client.GetAsync($"/api/repositories/all/{username}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.GreaterThanOrEqualTo(0));
            Assert.That(repositories, Has.Some.Matches<RepositoryDto>(r => r.IsPrivate == false));
            Assert.That(repositories, Has.Some.Matches<RepositoryDto>(r => r.IsPrivate == true));
        }

        [Test, Order(9)]
        public async Task GetVisibleRepositories_AsAdmin_ReturnsAllUserRepositories()
        {
            var username = "user@fakehub.com";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var response = await _client.GetAsync($"/api/repositories/all/{username}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.GreaterThanOrEqualTo(0));
            Assert.That(repositories, Has.Some.Matches<RepositoryDto>(repo => repo.IsPrivate));
        }

        [Test, Order(10)]
        public async Task Register_WithValidUserToken_AndSuccessfulSave_ReturnsOk()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "Valid Repo",
                Description = "A valid repository description.",
                IsPrivate = true,
                OwnedBy = RepositoryOwnedBy.User,
                OwnerId = 1
            };

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IRepositoryService));
                    services.AddSingleton<IRepositoryService>(new FakeRepositoryService
                    {
                        SaveFunc = dto => Task.FromResult(new ResponseBase
                        {
                            Success = true,
                            Result = new { Message = "Repository saved successfully" }
                        })
                    });
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);

            var response = await client.PostAsJsonAsync("/api/repositories", repositoryDto);
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
        }

        [Test, Order(11)]
        public async Task Register_WithValidUserToken_AndUnsuccessfulSave_ReturnsBadRequest()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "Invalid Repo",
                Description = "This repository fails to save.",
                IsPrivate = false,
                OwnedBy = RepositoryOwnedBy.User,
                OwnerId = 1
            };

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IRepositoryService));
                    services.AddSingleton<IRepositoryService>(new FakeRepositoryService
                    {
                        SaveFunc = dto => Task.FromResult(new ResponseBase
                        {
                            Success = false,
                            ErrorMessage = "Repository could not be saved"
                        })
                    });
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);

            var response = await client.PostAsJsonAsync("/api/repositories", repositoryDto);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.Multiple(() =>
            {
                Assert.That(responseObj, Is.Not.Null);
                Assert.That(responseObj!.Success, Is.False);
                Assert.That(responseObj.ErrorMessage, Is.EqualTo("Repository could not be saved"));
            });
        }

        private async Task SetupDbData()
        {
            using var scope = _factory.Services.CreateScope();
            await using var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

            if (!await roleManager.RoleExistsAsync("USER"))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = "USER" });
            }
            if (!await roleManager.RoleExistsAsync("ADMIN"))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = "ADMIN" });
            }
            if (!await roleManager.RoleExistsAsync("SUPERADMIN"))
            {
                await roleManager.CreateAsync(new IdentityRole<int> { Name = "SUPERADMIN" });
            }

            var regularUser = await userManager.FindByEmailAsync("user@fakehub.com");
            if (regularUser == null)
            {
                regularUser = new User
                {
                    Email = "user@fakehub.com",
                    UserName = "user@fakehub.com"
                };
                var result = await userManager.CreateAsync(regularUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser, "USER");
                }
            }

            var adminUser = await userManager.FindByEmailAsync("admin@fakehub.com");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    Email = "admin@fakehub.com",
                    UserName = "admin@fakehub.com"
                };
                var result = await userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "ADMIN");
                }
            }

            var superAdminUser = await userManager.FindByEmailAsync("superadmin@fakehub.com");
            if (superAdminUser == null)
            {
                superAdminUser = new User
                {
                    Email = "superadmin@fakehub.com",
                    UserName = "superadmin@fakehub.com"
                };
                var result = await userManager.CreateAsync(superAdminUser, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "SUPERADMIN");
                }
            }

            var regularUser1 = await userManager.FindByEmailAsync("user1@fakehub.com");
            if (regularUser1 == null)
            {
                regularUser1 = new User
                {
                    Email = "user1@fakehub.com",
                    UserName = "user1@fakehub.com"
                };
                var result = await userManager.CreateAsync(regularUser1, "Password123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(regularUser1, "USER");
                }
            }

            var repository1 = new Model.Entity.Repository
            {
                Id = 1,
                Name = "Repository1",
                Description = "Repository1 description",
                IsPrivate = true,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.User,
            };

            var repository2 = new Model.Entity.Repository
            {
                Id = 2,
                Name = "Repository2",
                Description = "Repository2 description",
                IsPrivate = false,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.User,
            };

            await db.Repositories.AddAsync(repository1);
            await db.Repositories.AddAsync(repository2);
            await db.SaveChangesAsync();
        }
        
        private async Task InitializeTokens()
        {
            var regularUserLogin = new LoginRequestDto
            {
                Email = "user@fakehub.com",
                Password = "Password123!"
            };
            _regularUserToken = await GetTokenFromSuccessfulUserLogin(regularUserLogin);

            var regularUser1Login = new LoginRequestDto
            {
                Email = "user1@fakehub.com",
                Password = "Password123!"
            };
            _regularUser1Token = await GetTokenFromSuccessfulUserLogin(regularUser1Login);

            var adminUserLogin = new LoginRequestDto
            {
                Email = "admin@fakehub.com",
                Password = "Password123!"
            };
            _adminToken = await GetTokenFromSuccessfulUserLogin(adminUserLogin);

            var superAdminUserLogin = new LoginRequestDto
            {
                Email = "superadmin@fakehub.com",
                Password = "Password123!"
            };
            _superAdminToken = await GetTokenFromSuccessfulUserLogin(superAdminUserLogin);
        }
        
        private async Task<string> GetTokenFromSuccessfulUserLogin(LoginRequestDto loginRequestDto)
        {
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequestDto);
            loginResponse.EnsureSuccessStatusCode();

            var responseObj = await loginResponse.Content.ReadFromJsonAsync<ResponseBase>();
            var loginResponseDtoString = responseObj?.Result?.ToString() ?? string.Empty;

            var loginResponseDtoObject = JsonConvert.DeserializeObject<LoginResponseDto>(loginResponseDtoString);
            return loginResponseDtoObject?.Token ?? "";
        }
        
        private class FakeRepositoryService : IRepositoryService
        {
            public Func<RepositoryDto, Task<ResponseBase>> SaveFunc { get; set; } =
                dto => Task.FromResult(new ResponseBase { Success = true });

            public Func<Task<ResponseBase>> GetAllRepositoriesForCurrentUserFunc { get; set; } =
                () => Task.FromResult(new ResponseBase { Success = true, Result = new List<RepositoryDto>() });

            public Func<Task<ResponseBase>> GetAllVisibleRepositoriesForUserFunc { get; set; } =
                () => Task.FromResult(new ResponseBase { Success = true, Result = new List<RepositoryDto>() });

            public Task<ResponseBase> Save(RepositoryDto model)
            {
                return SaveFunc(model);
            }

            public Task<ResponseBase> GetAllRepositoriesForCurrentUser()
            {
                return GetAllRepositoriesForCurrentUserFunc();
            }

            public Task<ResponseBase> GetAllVisibleRepositoriesForUser(string username)
            {
                return GetAllVisibleRepositoriesForUserFunc();
            }
        }

    }
}
