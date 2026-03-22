namespace SampleRag.Domain.RequestModels;

public class CreateScopeRequest
{
    public string Name { get; set; } = null!;

    public string[] UsersIds { get; set; } = null!;
}
