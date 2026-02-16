namespace SampleRag.Domain.Models.Configs;

public class JwtSettings
{
    public string? Authority { get; set; }

    public string? Audience { get; set; }

    public string? Issuer { get; set; }

    public string? MetadataAddress { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// When true and Authority is set, JWT validation is enabled.
    /// When false or Authority is empty, auth is disabled (dev mode).
    /// </summary>
    public bool Enabled => !string.IsNullOrWhiteSpace(Authority);
}
