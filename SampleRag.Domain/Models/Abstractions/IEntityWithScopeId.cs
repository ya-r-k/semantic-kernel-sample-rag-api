namespace SampleRag.Domain.Models.Abstractions;

public interface IEntityWithScopeId
{
    public Guid ScopeId { get; set; }
}
