using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace SampleRag.Application.Plugins;

public class TimePlugin
{
    [KernelFunction("GetCurrentTime")]
    [Description("To get current date and time")]
    [return: Description("Returns current date and time in dddd dd-MMM-yyyy hh:mm:ss")]
    public string GetCurrentTime()
    {
        return DateTime.UtcNow.ToString("dddd dd-MMM-yyyy hh:mm:ss");
    }
}
