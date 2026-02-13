using SampleRag.Application.Interfaces;
using SampleRag.Application.Interfaces.Services;
using SampleRag.Domain.Models;
using SampleRag.Domain.RequestModels;

namespace SampleRag.Application.Services;

public class DocumentService(
    IRepository<int, DocumentData> documentsRepository,
    IFileRepository fileRepository) : IDocumentService
{
    public async Task<IEnumerable<DocumentData>> AddAsync(params UploadDocumentRequestModel[] items)
    {
        var savingData = items.Select(x => new DocumentData
        {
            Name = x.Name,
        }).ToArray();

        for (var i = 0; i < savingData.Length; i++) 
        {
            savingData[i].LocalLink = await fileRepository.SaveAsync(items[i].File.Data, items[i].File.FileName);
        }

        return await documentsRepository.AddAsync(savingData);
    }

    public async Task<IEnumerable<DocumentData>> GetByIdsAsync(params int[] ids)
    {
        return await documentsRepository.GetByIdsAsync(ids);
    }

    public Task RemoveByIdsAsync(params int[] ids)
    {
        return documentsRepository.RemoveByIdsAsync(ids);
    }
}
