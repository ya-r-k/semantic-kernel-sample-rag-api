using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Models;

public class ChatData : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int[] UsersIds { get; set; }
}
