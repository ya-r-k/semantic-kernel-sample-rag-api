namespace SampleRag.Domain.Models.Configs;

public class JwtSettings
{
    public string? Authority { get; set; }

    public string? Audience { get; set; }

    public string? Issuer { get; set; }

    public string SigningKey { get; set; }

    public string? MetadataAddress { get; set; }

    public bool RequireHttpsMetadata { get; set; } = true;
}
