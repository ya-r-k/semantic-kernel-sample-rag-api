using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Infrastructure.Repositories.Files;

public class LocalFileRepository(FilesStorageSettings config) : IFileRepository
{
    public async Task<string> SaveAsync(string directoryPath, string fileName, string data)
    {
        var result = Path.Combine("assets\\documents", fileName);
        var path = Path.Combine(config.BasePath, result);

        path = Path.Combine(config.BasePath, path);
        await File.WriteAllBytesAsync(path, Convert.FromBase64String(data));

        return result;
    }

    public async Task<Stream?> GetAsync(string directoryPath, string fileName)
    {
        var path = Path.Combine(config.BasePath, directoryPath, fileName);
        if (!File.Exists(path))
        {
            return default;
        }

        var memoryStream = new MemoryStream();
        using var stream = File.OpenRead(path);

        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        return memoryStream;
    }
}
