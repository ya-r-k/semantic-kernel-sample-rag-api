namespace SampleRag.Domain.Models.Abstractions;

public interface IEntity<TId>
    where TId : unmanaged
{
    TId Id { get; set; }
}
