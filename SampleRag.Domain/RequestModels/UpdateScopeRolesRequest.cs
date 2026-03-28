using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.RequestModels;

public class UpdateScopeRolesRequest
{
    public UserRole[] AddingRoles { get; set; } = null!;

    public UserRole[] RemovingRoles { get; set; } = null!;
}
