using Mapster;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.Application.Filters.Render;

public class RetrivePluginRenderFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        await next.Invoke(context);

        var tool = context.Function.Name.Adapt<AiTool>();
        if (tool is AiTool.InternalDocumentData)
        {
            //context.RenderedPrompt = 
        }

        //logger.LogInformation($"Rendered prompt:\n{context.RenderedPrompt}");
    }
}
