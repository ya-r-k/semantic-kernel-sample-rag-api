using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace SampleRag.Application.KernelFunctions.Plugins;

public class TimePlugin
{
    [KernelFunction("GetCurrentDate")]
    [Description("To get current date and time")]
    [return: Description("Returns current date and time in dddd dd-MMM-yyyy hh:mm:ss")]
    public string GetCurrentDate()
    {
        return DateTime.UtcNow.ToString("dddd dd-MMM-yyyy hh:mm:ss");
    }
}
