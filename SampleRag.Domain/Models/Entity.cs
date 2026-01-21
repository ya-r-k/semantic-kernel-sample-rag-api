namespace SampleRag.Domain.Models;

public class Entity<TId> where TId : unmanaged
{
    public TId Id { get; set; }
}
