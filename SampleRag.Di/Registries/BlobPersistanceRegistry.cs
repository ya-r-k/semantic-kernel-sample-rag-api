using Microsoft.Extensions.DependencyInjection;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Configs;
using SampleRag.Infrastructure.Repositories.Files;

namespace SampleRag.Di.Registries;

public static class BlobPersistanceRegistry
{
    public static void ConfigureLocalFilesPersistance(this IServiceCollection services, string webRootPath)
    {
        services.AddSingleton(new FilesStorageSettings
        {
            BasePath = webRootPath,
        });

        services.AddTransient<IFileRepository, LocalFileRepository>();
    }
}
