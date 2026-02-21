using Microsoft.AspNetCore.SignalR;

namespace SampleRag.API.Hubs;

public class DocumentsIndexingHub : Hub
{
    public async Task SubscribeToJob(string jobId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"job_{jobId}");
    }

    public async Task UnsubscribeFromJob(string jobId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"job_{jobId}");
    }
}
