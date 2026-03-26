namespace SampleRag.Domain.Interfaces.Factories;

public interface ISettingsFactory<T>
    where T : new()
{
    T GetSettings(string settingName, IDictionary<string, object>? outerArguments = default);
}
