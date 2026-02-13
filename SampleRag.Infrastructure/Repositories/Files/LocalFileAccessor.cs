using SampleRag.Application.Interfaces;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Infrastructure.Repositories.Files;

public class LocalFileRepository(FilesStorageSettings config) : IFileRepository
{
    public async Task<string> SaveAsync(string data, string fileName)
    {
        var path = Path.Combine(config.BasePath, "assets/documents", fileName);

        await File.WriteAllBytesAsync(path, Convert.FromBase64String(data));

        return fileName;
    }

    public async Task<Stream> GetAsync(string directoryPath, string fileName)
    {
        var path = Path.Combine(config.BasePath, directoryPath, fileName);
        return File.OpenRead(path);
    }
}
