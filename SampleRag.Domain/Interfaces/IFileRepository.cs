namespace SampleRag.Domain.Interfaces;

public interface IFileRepository
{
    Task<string> SaveAsync(string directoryPath, string fileName, string data);

    Task<string> MoveAsync(string oldFilePath, string newFilePath);

    Task<Stream?> GetAsync(string directoryPath, string fileName);
}
