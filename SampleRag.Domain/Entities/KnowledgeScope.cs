using SampleRag.Domain.Models.Abstractions;
using SampleRag.Domain.Models.Enums;

namespace SampleRag.Domain.Entities;

public class KnowledgeScope : IEntity<Guid>
{
    public Guid Id { get; set; }

    public string? Name { get; set; }

    public UserRole[] Roles { get; set; }
}
