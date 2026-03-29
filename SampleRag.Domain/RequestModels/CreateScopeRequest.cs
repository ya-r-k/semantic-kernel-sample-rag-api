using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.RequestModels;

public class CreateScopeRequest
{
    public string Name { get; set; } = null!;

    public UserRole[] Roles { get; set; } = null!;
}
