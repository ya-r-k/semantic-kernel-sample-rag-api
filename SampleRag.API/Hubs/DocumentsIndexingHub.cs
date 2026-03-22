using Microsoft.AspNetCore.SignalR;

namespace SampleRag.API.Hubs;

public class DocumentsIndexingHub : Hub
{
    public async Task SubscribeToJob(string jobId)
    {
        await this.Groups.AddToGroupAsync(this.Context.ConnectionId, $"job_{jobId}");
    }

    public async Task UnsubscribeFromJob(string jobId)
    {
        await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, $"job_{jobId}");
    }
}
