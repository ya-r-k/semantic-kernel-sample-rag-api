# Chunks indexation progress flow

This note describes how chunk vectorization updates progress in the current codebase, where the weak points are, and how `DocumentsCount` should be handled.

## Current flow

The progress update starts in `ChunkVectorizationJob`:

1. Load a batch of chunks where `IsVectorized = false`.
2. Send the batch to the vector store with `UpsertChunksAsync`.
3. Mark every chunk in the batch as vectorized.
4. Persist the chunk state back to MongoDB.
5. Recalculate document progress for the affected document ids.
6. Recalculate scope progress for the affected document ids.

The document-level recalculation is straightforward:

- group chunks by `DocumentId`
- count total chunks per document
- count vectorized chunks per document
- update `Document.IndexPercentage`

The scope-level recalculation is more complex:

- start from the touched `DocumentId` values
- group chunks by `DocumentId`
- join documents to get `ScopeId`
- group again by `ScopeId`
- compute `IndexPercentage`
- compute `DocumentsCount`
- update `KnowledgeScope.IndexPercentage`

## Weak points in the current flow

### 1. Scope progress is batch-relative, not scope-relative

`KnowledgeScopeRepository.RecalculateIndexPercentageAsync` only sees the `documentsIds` from the current job batch. That means the scope percentage is calculated from the current slice of documents, not from all documents in the scope.

If a scope has 100 documents and the job processes only 5 of them, the scope progress is based on those 5 documents only. The number can therefore jump around depending on which batch ran last.

### 2. `DocumentsCount` is calculated but not persisted

The scope aggregation already produces `DocumentsCount`, but the update only writes `IndexPercentage`.

So today:

- `KnowledgeScope.DocumentsCount` exists in the model
- the scope pipeline computes a value
- the value is discarded

That makes the field effectively dead data unless another code path updates it.

### 3. `DocumentsCount` is not a good fit for the vectorization job

The chunk vectorization job is about embedding progress, not about catalog cardinality.

Counting documents inside the same job couples two different concerns:

- vectorization state
- scope/document inventory state

That coupling makes the progress logic harder to reason about and more expensive to run than necessary.

### 4. The scope aggregation mixes partial data with a total-like metric

The current pipeline groups the processed documents into scopes and then counts documents inside those groups.

That works only if the intent is:

- "How many of the processed documents belong to each scope?"

It does not work if the intent is:

- "How many documents exist in each scope?"

Those are different questions.

### 5. The aggregation is brittle

The scope repository pipeline uses raw BSON field names and a lookup to `Document` by `_id`.

That works, but it is easier to break than a typed query because:

- field names are stringly typed
- the pipeline depends on BSON representation details
- the `chunkFilter` variable is created but never used
- `Guid` handling is more fragile when the pipeline is hand-built

### 6. Progress can become inconsistent on partial failures

The job does three stateful operations in sequence:

- write vectors
- mark chunks vectorized
- recalculate document and scope progress

If a failure happens in the middle, the data can temporarily disagree:

- vectors may already exist
- chunks may still be marked unvectorized
- document or scope percentages may not match the chunk state

The vector upsert is idempotent, but the progress numbers are not protected by a transaction.

## Recommended model

Split the responsibilities.

### A. Keep vectorization progress in the chunk/document flow

Use the vectorization job to update:

- `DocumentChunk.IsVectorized`
- `Document.IndexPercentage`

These values are directly tied to chunk processing.

### B. Keep document count in the scope lifecycle

Treat `KnowledgeScope.DocumentsCount` as scope metadata, not vectorization state.

Update it when documents are created or deleted, or compute it from the `Document` collection with a dedicated method that takes `scopeId`.

This makes the count stable and cheap to reason about.

## Best place to calculate `DocumentsCount`

The effective choice is a **separate method**, not the same batch-based `RecalculateIndexPercentageAsync` call.

Recommended shape:

- `RecalculateIndexPercentageAsync(Guid[] documentsIds)` for chunk-progress updates
- `RecalculateDocumentsCountAsync(Guid scopeId)` for scope inventory updates

Why separate:

- it avoids recalculating counts on every chunk batch
- it avoids mixing partial batch data with total scope metadata
- it keeps scope totals correct even when no chunks are currently being vectorized

If you want a single method, it should still be scope-driven, not batch-driven:

- accept `scopeId`
- aggregate over all documents or all chunks in that scope
- update both `IndexPercentage` and `DocumentsCount`

The current `documentsIds` input is the main limitation. It is fine for document progress, but it is not enough for an accurate scope-wide document count.

## Practical recommendation for this codebase

Use this split:

1. `DocumentRepository.RecalculateIndexPercentageAsync(Guid[] documentsIds)` keeps the document-level percentage accurate.
2. `KnowledgeScopeRepository.RecalculateIndexPercentageAsync(Guid[] documentsIds)` should be changed to scope-wide recalculation, or replaced with a scope-driven method.
3. `DocumentsCount` should be maintained separately from vectorization progress, ideally on document create and delete operations.

That gives you:

- correct document progress
- stable scope counts
- less coupling between vectorization and metadata maintenance

