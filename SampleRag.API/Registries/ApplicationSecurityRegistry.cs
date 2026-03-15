using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.API.Registries;

public static class ApplicationSecurityRegistry
{
    public static void ConfigureCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5274")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
            });
        });
    }

    public static void ConfigureRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.AddFixedWindowLimiter("send-message", opt =>
            {
                opt.PermitLimit = 1;
                opt.Window = TimeSpan.FromMinutes(2);
                opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                opt.QueueLimit = 2;
            });

            // Обработка превышения лимита
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                await context.HttpContext.Response.WriteAsync("Too many requests", token);
            };
        });
    }

    public static void ConfigureJwtAuth(this IServiceCollection services, JwtSettings jwtSettings)
    {
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                /*options.Authority = jwtSettings.Authority;
                options.Audience = jwtSettings.Audience;
                options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;*/
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    RoleClaimType = "roles",

                    ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Issuer),
                    ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = !string.IsNullOrEmpty(jwtSettings.SigningKey),
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", policy => policy.RequireClaim("roles", "Admin", "SuperAdmin"));
    }

    public static void ConfigureCookieAuth(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };

                options.Cookie.HttpOnly = true;
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
                options.Cookie.SameSite = SameSiteMode.None;
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    }
}
