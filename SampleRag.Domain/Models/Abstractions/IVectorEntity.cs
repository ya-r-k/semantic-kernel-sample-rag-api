namespace SampleRag.Domain.Models.Abstractions;

public interface IVectorEntity<TKey, TVectorItem>
{
    TKey Id { get; set; }

    ReadOnlyMemory<TVectorItem> Vector { get; set; }
}
