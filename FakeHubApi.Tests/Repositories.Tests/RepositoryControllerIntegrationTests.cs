using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FakeHubApi.ContainerRegistry;
using FakeHubApi.Data;
using FakeHubApi.Model.Dto;
using FakeHubApi.Model.Entity;
using FakeHubApi.Model.ServiceResponse;
using FakeHubApi.Service.Contract;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                IsPrivate = true,
                OwnerId = 1
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
                IsPrivate = false,
                OwnerId = -1
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
        public async Task GetAllRepositoriesForOrganization_ValidOrgName_ReturnsOk()
        {
            var orgName = "Organization1";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
            var response = await _client.GetAsync($"/api/repositories/organization/{orgName}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories!.Count, Is.GreaterThan(0));
        }

        [Test, Order(11)]
        public async Task GetAllRepositoriesForOrganization_InvalidOrgName_ReturnsBadRequest()
        {
            var orgName = "NonExistentOrg";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
            var response = await _client.GetAsync($"/api/repositories/organization/{orgName}");

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(12)]
        public async Task GetAllRepositoriesForOrganization_NoRepositories_ReturnsOk()
        {
            var orgName = "Organization2";
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);
            var response = await _client.GetAsync($"/api/repositories/organization/{orgName}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Empty);
        }

        [Test, Order(13)]
        public async Task Register_RepositoryBadgeEqualsUserBadge_ReturnsOk()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);

            var userProfileResponse = await _client.GetAsync($"/api/users/user@fakehub.com");
            userProfileResponse.EnsureSuccessStatusCode();

            var userProfileObj = await userProfileResponse.Content.ReadFromJsonAsync<ResponseBase>();
            var userProfileString = userProfileObj?.Result?.ToString() ?? string.Empty;
            var userProfile = JsonConvert.DeserializeObject<UserDto>(userProfileString);

            Assert.That(userProfile, Is.Not.Null);
            var userBadge = userProfile!.Badge;

            var repoName = $"RepoBadgeEqualsUserBadge_{Guid.NewGuid().ToString().Substring(0, 8)}";
            var repositoryDto = new RepositoryDto
            {
                Name = repoName,
                Description = "Repo for badge equals user badge test.",
                IsPrivate = false,
                OwnedBy = RepositoryOwnedBy.User,
                OwnerId = -1
            };
            var response = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            var responseRepositoryString = responseObj?.Result?.ToString() ?? string.Empty;

            Assert.That(responseRepositoryString, Is.Not.Empty);
            var responseRepositoryObject = JsonConvert.DeserializeObject<Model.Entity.Repository>(responseRepositoryString);
            Assert.That(responseRepositoryObject, Is.Not.Null);
            Assert.That(responseRepositoryObject?.Badge, Is.EqualTo(userBadge));
        }

        [Test, Order(14)]
        public async Task DeleteRepository_AsAdminOwner_ReturnsOk()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "RepoToDelete",
                Description = "Repository for deletion test.",
                IsPrivate = false,
                OwnedBy = RepositoryOwnedBy.Admin,
                OwnerId = 2
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            var createResponse = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            createResponse.EnsureSuccessStatusCode();
            var createdRepoObj = await createResponse.Content.ReadFromJsonAsync<ResponseBase>();
            var createdRepoString = createdRepoObj?.Result?.ToString() ?? string.Empty;
            var createdRepo = JsonConvert.DeserializeObject<Model.Entity.Repository>(createdRepoString);

            Assert.That(createdRepo, Is.Not.Null);

            var deleteResponse = await _client.DeleteAsync($"/api/repositories/{createdRepo!.Id}");
            deleteResponse.EnsureSuccessStatusCode();
            var deleteResponseObj = await deleteResponse.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(deleteResponseObj, Is.Not.Null);
            Assert.That(deleteResponseObj!.Success, Is.True);
            Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test, Order(15)]
        public async Task DeleteRepository_RepoDoesNotExist_ReturnsBadRequest()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            var invalidRepoId = 9999;
            var response = await _client.DeleteAsync($"/api/repositories/{invalidRepoId}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.Multiple(() =>
            {
                Assert.That(responseObj, Is.Not.Null);
                Assert.That(responseObj!.Success, Is.False);
                Assert.That(responseObj.ErrorMessage, Is.EqualTo("Repository not found"));
            });
        }

        [Test, Order(16)]
        public async Task DeleteRepository_AsNonOwner_ReturnsBadRequest()
        {
            var repositoryDto = new RepositoryDto
            {
                Name = "RepoOtherOwner",
                Description = "Repository owned by another user.",
                IsPrivate = false,
                OwnedBy = RepositoryOwnedBy.Admin,
                OwnerId = 3
            };

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _superAdminToken);

            var createResponse = await _client.PostAsJsonAsync("/api/repositories", repositoryDto);
            createResponse.EnsureSuccessStatusCode();
            var createdRepoObj = await createResponse.Content.ReadFromJsonAsync<ResponseBase>();
            var createdRepoString = createdRepoObj?.Result?.ToString() ?? string.Empty;
            var createdRepo = JsonConvert.DeserializeObject<Model.Entity.Repository>(createdRepoString);

            Assert.That(createdRepo, Is.Not.Null);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            var deleteResponse = await _client.DeleteAsync($"/api/repositories/{createdRepo!.Id}");
            Assert.That(deleteResponse.IsSuccessStatusCode, Is.False);

            var deleteResponseObj = await deleteResponse.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.Multiple(() =>
            {
                Assert.That(deleteResponseObj, Is.Not.Null);
                Assert.That(deleteResponseObj!.Success, Is.False);
                Assert.That(deleteResponseObj.ErrorMessage, Is.EqualTo("You do not have permission to delete this repository"));
            });
        }

        [Test, Order(17)]
        public async Task GetAllPublicRepositories_WithRepositories_ReturnsOk()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);

            var response = await _client.GetAsync("/api/repositories/public-repositories");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repositories = JsonConvert.DeserializeObject<List<RepositoryDto>>(responseObj.Result.ToString()!);
            Assert.That(repositories, Is.Not.Null);
            Assert.That(repositories!.Count, Is.GreaterThanOrEqualTo(0));
            Assert.That(repositories, Has.All.Matches<RepositoryDto>(r => r.IsPrivate == false));
        }

        [Test, Order(18)]
        public async Task GetRepository_ValidId_ReturnsOk()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUser1Token);

            var repositoryId = 5;
            var response = await _client.GetAsync($"/api/repositories/{repositoryId}");
            response.EnsureSuccessStatusCode();

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.True);
            Assert.That(responseObj.Result, Is.Not.Null);

            var repository = JsonConvert.DeserializeObject<RepositoryDto>(responseObj.Result.ToString()!);
            Assert.That(repository, Is.Not.Null);
            Assert.That(repository!.Id, Is.EqualTo(repositoryId));
        }

        [Test, Order(19)]
        public async Task GetRepository_InvalidId_ReturnsBadRequest()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUserToken);

            var invalidRepositoryId = 99999;
            var response = await _client.GetAsync($"/api/repositories/{invalidRepositoryId}");

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.False);
            Assert.That(responseObj.ErrorMessage, Is.EqualTo($"Repository with id {invalidRepositoryId} does not exist."));
        }

        [Test, Order(20)]
        public async Task GetRepository_PrivateRepositoryWithoutAccess_ReturnsBadRequest()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _regularUser1Token);

            var privateRepositoryId = 4;
            var response = await _client.GetAsync($"/api/repositories/{privateRepositoryId}");

            Assert.That(response.IsSuccessStatusCode, Is.False);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(responseObj, Is.Not.Null);
            Assert.That(responseObj!.Success, Is.False);
            Assert.That(responseObj.ErrorMessage, Is.EqualTo($"Repository with id {privateRepositoryId} does not exist."));
        }

        [Test, Order(21)]
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

        [Test, Order(22)]
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
        
        [Test, Order(23)]
        public async Task EditRepository_AsAdmin_SuccessfulEdit_ReturnsOk()
        {
            var fakeService = new FakeRepositoryService
            {
                EditRepositoryFunc = dto => Task.FromResult(new ResponseBase
                {
                    Success = true,
                    Result = JsonConvert.SerializeObject(new Model.Entity.Repository
                    {
                        Id = dto.Id,
                        Description = dto.Description,
                        IsPrivate = dto.IsPrivate,
                        OwnedBy = RepositoryOwnedBy.Admin,
                        OwnerId = 2
                    })
                })
            };

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IRepositoryService));
                    services.AddSingleton<IRepositoryService>(fakeService);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);

            var editDto = new EditRepositoryDto(1, "Updated Description", false);
            var editResponse = await client.PutAsJsonAsync("/api/repositories", editDto);

            editResponse.EnsureSuccessStatusCode();

            var editResponseObj = await editResponse.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.That(editResponseObj, Is.Not.Null);
            Assert.That(editResponseObj!.Success, Is.True);

            var updatedRepoString = editResponseObj.Result?.ToString() ?? string.Empty;
            var updatedRepo = JsonConvert.DeserializeObject<Model.Entity.Repository>(updatedRepoString);

            Assert.That(updatedRepo, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(updatedRepo!.Description, Is.EqualTo("Updated Description"));
                Assert.That(updatedRepo.IsPrivate, Is.False);
            });
        }

        [Test, Order(24)]
        public async Task EditRepository_WithoutToken_ReturnsUnauthorized()
        {
            var editDto = new EditRepositoryDto(1, "", false);

            _client.DefaultRequestHeaders.Authorization = null;
            var response = await _client.PutAsJsonAsync("/api/repositories", editDto);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        }

        [Test, Order(25)]
        public async Task EditRepository_RepoDoesNotExist_ReturnsBadRequest()
        {
            var editDto = new EditRepositoryDto(9999, "Updated Description", true);

            var fakeService = new FakeRepositoryService
            {
                EditRepositoryFunc = dto =>
                {
                    if (dto.Id == 9999)
                    {
                        return Task.FromResult(new ResponseBase
                        {
                            Success = false,
                            ErrorMessage = "Repository not found."
                        });
                    }

                    return Task.FromResult(new ResponseBase
                    {
                        Success = true,
                        Result = JsonConvert.SerializeObject(new Model.Entity.Repository
                        {
                            Id = dto.Id,
                            Description = dto.Description,
                            IsPrivate = dto.IsPrivate
                        })
                    });
                }
            };

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IRepositoryService));
                    services.AddSingleton<IRepositoryService>(fakeService);
                });
            }).CreateClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _adminToken);
            var response = await client.PutAsJsonAsync("/api/repositories", editDto);

            Assert.Multiple(() =>
            {
                Assert.That(response.IsSuccessStatusCode, Is.False);
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            });

            var responseObj = await response.Content.ReadFromJsonAsync<ResponseBase>();
            Assert.Multiple(() =>
            {
                Assert.That(responseObj, Is.Not.Null);
                Assert.That(responseObj!.Success, Is.False);
                Assert.That(responseObj.ErrorMessage, Is.EqualTo("Repository not found."));
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

            var organization1 = new Model.Entity.Organization
            {
                Id = 1,
                Name = "Organization1",
                Description = "Organization1 description",
                IsActive = true,
                OwnerId = regularUser.Id,
                Owner = regularUser,
            };

            var organization2 = new Model.Entity.Organization
            {
                Id = 2,
                Name = "Organization2",
                Description = "Organization2 description",
                IsActive = true,
                OwnerId = regularUser.Id,
                Owner = regularUser,
            };

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

            var repository3 = new Model.Entity.Repository
            {
                Id = 3,
                Name = "Repository3",
                Description = "Repository3 description",
                IsPrivate = false,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.Organization,
            };

            var repository4 = new Model.Entity.Repository
            {
                Id = 4,
                Name = "PublicRepo4",
                Description = "A private repository",
                IsPrivate = true,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.User,
            };

            var repository5 = new Model.Entity.Repository
            {
                Id = 5,
                Name = "PublicRepo5",
                Description = "A public repository",
                IsPrivate = false,
                OwnerId = 1,
                OwnedBy = RepositoryOwnedBy.User,
            };

            await db.Organizations.AddAsync(organization1);
            await db.Organizations.AddAsync(organization2);
            await db.Repositories.AddAsync(repository1);
            await db.Repositories.AddAsync(repository2);
            await db.Repositories.AddAsync(repository3);
            await db.Repositories.AddAsync(repository4);
            await db.Repositories.AddAsync(repository5);
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

            private Func<Task<ResponseBase>> GetAllRepositoriesForCurrentUserFunc { get; set; } =
                () => Task.FromResult(new ResponseBase { Success = true, Result = new List<RepositoryDto>() });

            private Func<Task<ResponseBase>> GetAllVisibleRepositoriesForUserFunc { get; set; } =
                () => Task.FromResult(new ResponseBase { Success = true, Result = new List<RepositoryDto>() });

            private Func<Task<ResponseBase>> GetAllRepositoriesForOrganizationFunc { get; set; } =
                () => Task.FromResult(new ResponseBase { Success = true, Result = new List<RepositoryDto>() });

            private Func<int, Task<ResponseBase>> DeleteRepositoryFunc { get; set; } =
                repositoryId => Task.FromResult(new ResponseBase { Success = true });

            public Task<(string, string)> GetFullProjectRepositoryName(int repositoryId)
            {
                // Return dummy values for testing
                return Task.FromResult(("ProjectName", "RepositoryName"));
            }

            public List<ArtifactDto> MapHarborArtifactToArtifactDto(HarborArtifact source)
            {
                // Return an empty list for testing
                return new List<ArtifactDto>();
            }

            public Task<ResponseBase> DeleteRepositoriesOfOrganization(Model.Entity.Organization existingOrganization)
            {
                // Simulate async deletion
                return Task.FromResult(ResponseBase.SuccessResponse());
            }

            public Task<ResponseBase> Search(string? query)
            {
                throw null;
            }
            
            private Func<Task<ResponseBase>> GetAllPublicRepositoriesFunc { get; set; } =
                () => Task.FromResult(new ResponseBase
                {
                    Success = true,
                    Result = new List<RepositoryDto>
                    {
                        new RepositoryDto
                        {
                            Id = 1,
                            Name = "PublicRepo1",
                            Description = "A public repository",
                            IsPrivate = false,
                            OwnedBy = RepositoryOwnedBy.User,
                            OwnerId = 2,
                            FullName = "User-PublicRepo1/PublicRepo1",
                            OwnerUsername = "User"
                        }
                    }
                });

            public Func<EditRepositoryDto, Task<ResponseBase>> EditRepositoryFunc { get; set; } =
                dto => Task.FromResult(new ResponseBase
                {
                    Success = true,
                    Result = JsonConvert.SerializeObject(new Model.Entity.Repository
                    {
                        Id = dto.Id,
                        Description = dto.Description,
                        IsPrivate = dto.IsPrivate
                    })
                });

            private Func<int, Task<ResponseBase>> GetRepositoryFunc { get; set; } =
                repositoryId => Task.FromResult(new ResponseBase
                {
                    Success = true,
                    Result = JsonConvert.SerializeObject(new Model.Entity.Repository
                    {
                        Id = repositoryId,
                        Description = "Existing Description",
                        IsPrivate = false,
                        OwnedBy = RepositoryOwnedBy.Admin,
                        OwnerId = 2
                    })
                });

            private Func<int, Task<ResponseBase>> CanEditRepositoryFunc { get; set; } =
                repositoryId => Task.FromResult(new ResponseBase
                {
                    Success = true
                });

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

            public Task<ResponseBase> GetAllRepositoriesForOrganization(string name)
            {
                return GetAllRepositoriesForOrganizationFunc();
            }

            public Task<ResponseBase> GetRepository(int repositoryId)
            {
                return GetRepositoryFunc(repositoryId);
            }

            public Task<ResponseBase> DeleteRepository(int repositoryId)
            {
                return DeleteRepositoryFunc(repositoryId);
            }
            public Task<ResponseBase> CanEditRepository(int repositoryId)
            {
                return CanEditRepositoryFunc(repositoryId);
            }

            public Task<ResponseBase> EditRepository(EditRepositoryDto data)
            {
                return EditRepositoryFunc(data);
            }

            public Task<ResponseBase> GetAllPublicRepositories()
            {
                return GetAllPublicRepositoriesFunc();
            }
        }
    }
}
