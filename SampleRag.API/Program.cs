using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.IdentityModel.Tokens;
using SampleRag.API.Endpoints;
using SampleRag.API.Middleware;
using SampleRag.Di;
using SampleRag.Di.Mapping;
using SampleRag.Domain.Models.Configs;

var builder = WebApplication.CreateSlimBuilder(args);

// builder.AddServiceDefaults();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 4;  // Максимум 4 запроса
        opt.Window = TimeSpan.FromSeconds(12);  // За 12 секунд
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;  // Очередь на 2 запроса
    });

    // Обработка превышения лимита
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Too many requests", token);
    };
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (jwtSettings.Enabled)
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = jwtSettings.Authority;
            options.Audience = jwtSettings.Audience;
            options.RequireHttpsMetadata = jwtSettings.RequireHttpsMetadata;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(jwtSettings.Issuer),
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = !string.IsNullOrEmpty(jwtSettings.Audience),
                ValidAudience = jwtSettings.Audience,
            };
        });
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdministrator", policy =>
            policy.RequireRole("Administrator"));
}
else
{
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("RequireAdministrator", policy =>
            policy.RequireRole("Administrator"));
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Dev";
        options.DefaultChallengeScheme = "Dev";
    }).AddScheme<AuthenticationSchemeOptions, DevAuthHandler>("Dev", _ => { });
}

/*builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});*/

builder.Services.ConfigureMapster();
builder.Services.ConfigureAiDependencies(builder.Configuration);
builder.Services.ConfigureDependencies(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "no-sniff";
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapChatsEndpoints();
app.MapMessagesEndpoints();
app.MapDocumentsEndpoints();
app.MapFilesEndpoints();
app.MapKnowledgeScopesEndpoints();
app.MapFeedbacksEndpoints();

app.Run();
