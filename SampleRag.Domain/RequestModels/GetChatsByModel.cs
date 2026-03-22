// <copyright file="GetChatsByModel.cs" company="SampleRag">
// Copyright (c) SampleRag. See .specify/memory/constitution.md for project governance.
// </copyright>

namespace SampleRag.Domain.RequestModels;

public class GetChatsByModel : GetBatchByModel
{
    public Guid? ScopeId { get; set; }
}
