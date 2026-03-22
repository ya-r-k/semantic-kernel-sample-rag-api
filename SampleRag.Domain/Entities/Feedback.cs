using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities;

public class Feedback : IEntity<Guid>
{
    public Guid Id { get; set; }

    public Guid MessageId { get; set; }

    public string UserId { get; set; } = null!;

    public bool IsLike { get; set; }
}
