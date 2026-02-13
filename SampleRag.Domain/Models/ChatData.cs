namespace SampleRag.Domain.Models;

public class ChatData : Entity<int>
{
    public string Name { get; set; }

    public int[] UsersIds { get; set; }
}
