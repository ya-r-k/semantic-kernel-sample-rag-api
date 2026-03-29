using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.RequestModels;

public class CreateScopeRequest
{
    public string Name { get; set; } = null!;

    public UserRole[] AddingRoles { get; set; } = null!;
}
