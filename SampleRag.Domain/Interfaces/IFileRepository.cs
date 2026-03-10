namespace SampleRag.Domain.Interfaces;

public interface IFileRepository
{
    Task<string> SaveAsync(string directoryPath, string fileName, string data);

    Task<Stream?> GetAsync(string directoryPath, string fileName);
}
