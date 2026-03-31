namespace SampleRag.Domain.Interfaces.Factories;

public interface ISettingsFactory<T>
    where T : new()
{
    T GetSettings(string settingsName, IDictionary<string, object>? outerArguments = default);
}
