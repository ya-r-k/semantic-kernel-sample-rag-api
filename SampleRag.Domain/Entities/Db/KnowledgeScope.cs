using SampleRag.Domain.Models.Abstractions;

namespace SampleRag.Domain.Entities.Db;

public class KnowledgeScope : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; }
}
