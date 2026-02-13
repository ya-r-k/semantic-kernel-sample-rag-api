namespace SampleRag.Domain.RequestModels;

public class UploadDocumentRequestModel
{
    public string Name { get; set; }

    public FileDataRequestModel File { get; set; }
}
