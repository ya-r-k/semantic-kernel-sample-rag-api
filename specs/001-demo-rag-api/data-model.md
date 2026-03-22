# Data Model: Demo RAG API

**Branch**: `001-demo-rag-api` | **Date**: 2025-02-14  
**Source**: [spec.md](./spec.md) Key Entities + FR; aligned with [plan.md](./plan.md) and [research.md](./research.md).

---

## Entity Relationship Overview

- **Scope** (KnowledgeGroup): Container for documents and chats; has access list (ScopeUser).
- **ScopeUser**: Which users can use which scope (for upload, chat create, RAG).
- **Document**: Uploaded PDF; belongs to one Scope; has storage path and optional extracted metadata.
- **DocumentChunk** (vector store): Chunk of text with DocumentId, ScopeId, PageNumber; stored in Qdrant with embedding (mapping in config/Infrastructure).
- **Chat**: Bound to one Scope; has title (possibly generated), list of OwnerIds (user identifiers).
- **Message**: Belongs to one Chat; has content, sender (user vs system), ordering; system messages may carry source references.
- **SourceReference**: Value type in API response: DocumentId + PageNumber (and optional chunk id) for each cited source.
- **Feedback**: User’s like/dislike on a specific system message (MessageId); one per user per message (last wins).

---

## Entities (Domain / Persistence)

### Scope (alias KnowledgeGroup)

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| Name | string | Display name |

**Validation**: Name required.  
**Persistence**: MongoDB collection (e.g. KnowledgeGroupData or Scopes).  
**Note**: Access controlled via ScopeUser.

---

### ScopeUser

| Field | Type | Description |
|-------|------|-------------|
| ScopeId | Guid | FK to Scope |
| UserId | string | User identifier (from token sub or equivalent) |

**Uniqueness**: (ScopeId, UserId).  
**Persistence**: MongoDB collection ScopeUsers (or embedded/list in Scope if preferred).  
**Use**: Enforce “user can use scope” on upload, chat create, and RAG.

---

### Document

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| Name | string | Original file name |
| LocalLink | string | Path under wwwroot (API-only access) |
| ScopeId | Guid | FK to Scope; document belongs to one scope |
| BriefDescription | string? | Optional; for display |

**Validation**: Name, LocalLink, ScopeId required. Max file size 20 MB enforced at upload.  
**Persistence**: MongoDB (DocumentData). Align existing KnowledgeGroupIds to single ScopeId per spec.

---

### DocumentChunk (vector store payload)

Logical model for what is stored in Qdrant (actual mapping in Infrastructure/config; no attributes on Domain model).

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Key for upsert/deletion |
| DocumentId | Guid | Source document |
| ScopeId | Guid | For scope-filtered retrieval |
| PageNumber | int | 1-based page in PDF |
| ChunkIndex | int? | 0-based when page is split into multiple chunks |
| Text | string | Chunk text |
| Embedding | float[] | From Ollama embedding model (dimension from model) |

**Persistence**: Qdrant; schema and mapping configured in DI (constitution: no attributes on domain models).  
**Lifecycle**: Created during document ingestion after PDF chunking; deleted or updated when document is removed or re-ingested.

---

### Chat

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| Title | string | Display title (may be system-generated) |
| ScopeId | Guid | Chat bound to one scope; RAG uses only this scope’s documents |
| OwnerIds | string[] | User identifiers who can send/receive and add owners |

**Validation**: Title, ScopeId required; at least one OwnerId at creation (creator).  
**Persistence**: MongoDB (ChatData). Add ScopeId; rename or repurpose UsersIds to OwnerIds (string[] to match token sub).  
**State**: No formal state machine; soft delete if needed.

---

### Message

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| ChatId | Guid | FK to Chat (align type with Chat.Id) |
| Text | string | Content |
| AiGenerated | bool | true for system messages |
| CreatedAt | DateTime? | Set when persisted |
| SourceReferences | SourceReference[]? | For AI messages: document + page list |

**Validation**: ChatId, Text required.  
**Persistence**: MongoDB (MessageData). Fix ChatId type to Guid; add or reuse field for source references (e.g. list of { DocumentId, PageNumber }).  
**Note**: DocumentPagesIds in current model to be replaced or interpreted as part of source citation (document + page).

---

### SourceReference (value type)

| Field | Type | Description |
|-------|------|-------------|
| DocumentId | Guid | Cited document |
| PageNumber | int | Page number (1-based) |

Used in API response and optionally stored with Message for system replies.

---

### Feedback

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | PK |
| MessageId | Guid | The system message (answer) being rated |
| UserId | string | Who submitted feedback |
| IsLike | bool | true = like, false = dislike |

**Uniqueness**: One Feedback per (MessageId, UserId); upsert on submit (last wins).  
**Persistence**: MongoDB collection Feedbacks.

---

## State Transitions

- **Document**: Created on upload → (optional) Re-ingested or Deleted. No published “processing” state required for MVP; ingestion can be synchronous or background.
- **Chat**: Created (with ScopeId + initial owner) → owners may be added → no delete requirement in spec (add if needed).
- **Message**: Append-only within a chat.
- **Feedback**: Create or update by (MessageId, UserId).

---

## Cross-Cutting Rules

1. **Scope isolation**: All RAG retrieval filters by ScopeId (chat’s scope). Document upload and chat creation require scope access (ScopeUser).
2. **Identifiers**: Use Guid for Ids; UserId from token (string). OwnerIds and ScopeUser.UserId are string to match typical JWT `sub` or nameidentifier.
3. **Constitution**: Domain entities have no Mongo/Qdrant/EF attributes; mapping and schema in configuration/Infrastructure. DocumentChunk is implemented as a DTO or config-mapped type in Infrastructure for Qdrant.
