namespace SampleRag.Domain.Interfaces;

public interface IFileRepository
{
    Task<string> SaveAsync(string data, string fileName);

    Task<Stream?> GetAsync(string directoryPath, string fileName);
}
