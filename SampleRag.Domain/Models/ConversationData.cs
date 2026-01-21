namespace SampleRag.Domain.Models;

public class ConversationData<TId> where TId : unmanaged
{
    public TId Id { get; set; }

    public string Name { get; set; }

    public TId UsersIds { get; set; }
}
