using Microsoft.AspNetCore.Routing.Constraints;
using SampleRag.API.Endpoints;
using SampleRag.API.Endpoints.Auth;
using SampleRag.API.Registries;
using SampleRag.Di.Registries;
using SampleRag.Domain.Models.Configs;
using Serilog;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger());

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwaggerApiDocs();

builder.Services.Configure<RouteOptions>(options =>
{
    options.SetParameterPolicy<RegexInlineRouteConstraint>("regex");
});

/*builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});*/

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new ();

builder.Services.ConfigureJwtAuth(jwtSettings);
builder.Services.ConfigureCors();
builder.Services.ConfigureRateLimiting();

builder.Services.ConfigureMapsterSettings();
builder.Services.ConfigureBusinessLogic(builder.Configuration);

var dbSettings = builder.Configuration.GetSection(nameof(DbSettings)).Get<DbSettings>() ?? new ();
var jobsSettings = builder.Configuration.GetSection(nameof(DocumentsJobsSettings)).Get<DocumentsJobsSettings>() ?? new ();
var vectorDbSettings = builder.Configuration.GetSection(nameof(VectorDbSettings)).Get<VectorDbSettings>() ?? new ();
var lmConfig = builder.Configuration.GetSection(nameof(GenAiProviderSettings)).Get<GenAiProviderSettings>() ?? new ();

builder.Services.ConfigureMongoDb(dbSettings);
//builder.Services.ConfigureQuartzJobs(dbSettings, jobsSettings);
builder.Services.ConfigureKernel(lmConfig);
builder.Services.ConfigureQdrant(vectorDbSettings);
builder.Services.ConfigureLocalFilesPersistance(builder.Environment.WebRootPath);

var app = builder.Build();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

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

if (builder.Environment.IsDevelopment())
{
    app.MapDevAuthEndpoints();
}

app.Run();
