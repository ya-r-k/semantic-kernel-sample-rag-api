using System.Linq.Expressions;
using SampleRag.Domain.Interfaces;
using SampleRag.Domain.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IRepository<Guid, Document> documentsRepository,
    IFileRepository fileRepository) : IDocumentService
{
    public async Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] items)
    {
        var savingData = items.Select(x => new Document
        {
            Name = x.Name,
            ScopeId = x.ScopeId,
        }).ToArray();

        for (var i = 0; i < savingData.Length; i++)
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync(items[i].File.Content, items[i].File.FileName);
        }

        return await documentsRepository.AddAsync(savingData);
    }

    public async Task<IEnumerable<Document>> GetBatchByAsync(Expression<Func<Document, bool>> expression, int batchSize)
    {
        return await documentsRepository.GetBatchByAsync(expression, batchSize);
    }

    public async Task<IEnumerable<Document>> GetByIdsAsync(params Guid[] ids)
    {
        return await documentsRepository.GetByIdsAsync(ids);
    }

    public Task RemoveByIdsAsync(params Guid[] ids)
    {
        return documentsRepository.RemoveByIdsAsync(ids);
    }

    public Task UpdateAsync(params Document[] items)
    {
        return documentsRepository.UpdateAsync(items);
    }
}
