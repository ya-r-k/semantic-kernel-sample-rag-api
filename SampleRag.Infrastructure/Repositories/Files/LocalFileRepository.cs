using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Models.Configs;

namespace SampleRag.Infrastructure.Repositories.Files;

public class LocalFileRepository(FilesStorageSettings config) : IFileRepository
{
    public async Task<string> SaveAsync(string directoryPath, string fileName, string data)
    {
        var result = Path.Combine(directoryPath, fileName);
        var path = Path.Combine(config.BasePath, result);

        this.EnsureDirectoryExist(Path.GetDirectoryName(path) ?? string.Empty);

        await File.WriteAllBytesAsync(path, Convert.FromBase64String(data));

        return result;
    }

    public Task<string> MoveAsync(string oldFilePath, string newFilePath)
    {
        var fullOldPath = Path.Combine(config.BasePath, oldFilePath);
        var fullNewPath = Path.Combine(config.BasePath, newFilePath);

        this.EnsureDirectoryExist(Path.GetDirectoryName(fullNewPath) ?? string.Empty);

        File.Move(fullOldPath, fullNewPath);

        return Task.FromResult(newFilePath);
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

    private void EnsureDirectoryExist(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }
}
