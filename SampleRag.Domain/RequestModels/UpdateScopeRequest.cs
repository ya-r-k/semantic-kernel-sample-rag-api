using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.RequestModels;

public class UpdateScopeRequest : CreateScopeRequest
{
    public UserRole[] RemovingRoles { get; set; } = null!;
}
