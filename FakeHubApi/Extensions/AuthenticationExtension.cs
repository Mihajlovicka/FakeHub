using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FakeHubApi.Model.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace FakeHubApi.Extensions;

public static class AuthenticationExtension
{
    public static WebApplicationBuilder AddAuthenticationAndAuthorization(
        this WebApplicationBuilder builder
    )
    {
        var jwtOptions = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

        if (jwtOptions == null)
            throw new Exception("Jwt options not found.");

        var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);

        builder
            .Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };

                x.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = async context =>
                    {
                        var harborTokenService = context.HttpContext.RequestServices
                            .GetRequiredService<IHarborTokenService>();

                        // izvuci raw token
                        var token = context.Request.Headers["Authorization"]
                            .FirstOrDefault()?.Split(" ").Last();

                        string userId = null;
                        if (!string.IsNullOrEmpty(token))
                        {
                            try
                            {
                                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                                userId = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                            }
                            catch
                            {
                                // ako token ne može da se parsira, userId ostaje null
                            }
                        }

                        await harborTokenService.HandleInvalidToken(userId);
                    }
                };
            });
        builder.Services.AddAuthorization();

        return builder;
    }
}
