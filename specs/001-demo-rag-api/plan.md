# Implementation Plan: Demo RAG API

**Branch**: `001-demo-rag-api` | **Date**: 2025-02-14 | **Spec**: [spec.md](./spec.md)  
**Input**: Feature specification from `specs/001-demo-rag-api/spec.md`; comparison with [project.md](../../project.md) for implemented vs missing.

## Summary

Deliver a demo RAG API where: (1) admins upload PDFs into scoped storage, (2) users chat in scope-bound conversations and receive answers grounded in those documents with source references (document + page), (3) chats support multiple owners and auto-creation with generated titles, (4) users can submit like/dislike feedback on answers. The plan closes gaps between the current codebase and the spec, adds a full RAG pipeline (PDF chunking → embedding → Qdrant → retrieval → LLM with sources), scope API and enforcement, auth (token/role), and feedback. **PDF chunking**: Implemented via page-based or fixed-size text chunking with page metadata for source citation; details in [research.md](./research.md).

## Technical Context

**Language/Version**: C# / .NET 10  
**Primary Dependencies**: ASP.NET Core (Minimal API), Microsoft Semantic Kernel, Ollama (OllamaSharp), Qdrant (Semantic Kernel connector), MongoDB.Driver, Mapster  
**Storage**: MongoDB (chats, messages, documents, scopes, feedback), Qdrant (vector store for document chunks), local filesystem under wwwroot for PDF files  
**Target Platform**: ASP.NET web host (Linux/Windows)  
**Project Type**: Web (API only)  
**Performance Goals**: Upload confirmation within a few seconds; RAG answer in single request (spec SC-001, SC-002)  
**Constraints**: Max file upload 20 MB (constitution); files only via API (no direct static serving of uploads); Ollama + Qdrant only  
**Scale/Scope**: Demo; single deployment, no horizontal scaling requirements specified  

## Implemented vs Missing (Gap Analysis)

Comparison of [project.md](../../project.md) with [spec.md](./spec.md):

| Spec requirement | Current state | Gap / action |
|------------------|---------------|--------------|
| **FR-001** Admin-only upload | No auth; no role checks | Add JWT/token validation; reject non-admin uploads. |
| **FR-001b** Scope API + enforcement | Knowledge groups exist (POST/GET); no “who can use” enforcement | Add scope access model; enforce scope on upload, chat create, and RAG. |
| **FR-002** PDF in scope | Documents saved without scope; no PDF→vector pipeline | Associate document with scope; implement chunking + embedding + Qdrant per scope. |
| **FR-003** Max file size 20 MB | No size limit in code | Enforce 20 MB (and optionally content-type) in upload pipeline. |
| **FR-004** RAG: answer + sources (doc + page) | No retrieval; no source citation; MessageData has DocumentPagesIds but unused | Implement: chunk PDFs, embed, store in Qdrant by scope; on message, retrieve by scope → LLM with context → return answer + source list (document id + page). |
| **FR-005** Chat bound to scope; multi-owner | ChatData has Name, UsersIds; no ScopeId; “owners” not distinguished | Add ScopeId to chat; treat UsersIds as owner list; bind RAG to chat’s scope. |
| **FR-006** Send message without chat → create chat, generated title | Chats created explicitly (POST body); no “create on first message” flow | Add “send message without chatId” flow: create chat with scope + LLM-generated title, then add message. |
| **FR-007** Add-owner API | No add-owner/invite endpoint | Add endpoint for existing owner to add another owner to a chat. |
| **FR-008** Like/dislike feedback | No feedback model or API | Add Feedback entity and endpoints (submit, optionally get). |
| **FR-009** API-only access to files | GET api/files/assets/documents/{fileName} exists | Ensure no static file serving of uploads; keep file access only via API (OK). |
| **PDF chunking** | DocumentPageData exists but not used; no ingestion pipeline | Design and implement chunking (page-based or fixed-size with page metadata); see research.md. |
| **Constitution** | Minimal API, SK, Ollama, Qdrant, wwwroot, Clean Arch, Repository | Mapster: ensure global DI config. Models: DocumentPageData uses vector attributes → move to config/DTO in Infrastructure. |

**Bug fixes from project.md**: Ensure list endpoints (GET) for chats and groups return `Results.Ok` (fix any use of `Results.Created` for list responses); align `MessageData.ChatId` type with `ChatData.Id` (Guid); fix DataGenerator streaming condition in SampleRag.Application/Services/DataGenerator.cs if inverted (streaming when it should not, or vice versa).

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **I. C# / Semantic Kernel / Ollama only**: Compliant; stack is C#, SK, Ollama (no OpenAI/Azure).  
- **II. ASP.NET Minimal API**: Compliant; all endpoints are Minimal API.  
- **III. Mapster only, global DI**: Mapster in use; ensure single global registration in DI.  
- **IV. Qdrant as vector store**: Compliant; Qdrant used for embeddings.  
- **V. Files in wwwroot; API gateway only; 20 MB**: Files under wwwroot; access via API only; add 20 MB enforcement.  
- **VI. Clean Architecture, Repository, interfaces in Domain**: Compliant; layers and interfaces in Domain.  
- **VII. No attributes on models; config via DI**: DocumentPageData uses `[VectorStoreKey]` etc.; move vector mapping to configuration or Infrastructure DTO so domain models stay attribute-free.

**Verdict**: PASS with one design action: move vector-store mapping off domain models into configuration/Infrastructure.

## Project Structure

### Documentation (this feature)

```text
specs/001-demo-rag-api/
├── plan.md              # This file
├── research.md          # Phase 0 (PDF chunking, scope, auth)
├── data-model.md        # Phase 1
├── quickstart.md        # Phase 1
├── contracts/           # Phase 1 (API contracts)
├── checklists/          # requirements.md, rag-models-vector-memory.md
└── tasks.md             # Phase 2 (/speckit.tasks – not created by plan)
```

### Source Code (repository root)

```text
SampleRag.API/           # Host, Minimal API endpoints, middleware
├── Endpoints/           # ChatsEndpoints, MessagesEndpoints, DocumentsEndpoints, FilesEndpoints, KnowledgeGroupsEndpoints
├── Hubs/
└── wwwroot/assets/documents/

SampleRag.Application/
└── Services/             # MessageService, DocumentService, DataGenerator (+ RAG pipeline, scope checks)

SampleRag.Domain/
├── Interfaces/          # Repository, services (move to Domain if under Application.Interfaces)
├── Models/              # ChatData, MessageData, DocumentData, KnowledgeGroupData, DocumentPageData, etc.
├── RequestModels/
└── (add Feedback, scope access interfaces as needed)

SampleRag.Infrastructure/
└── Repositories/        # Mongo, Files; + vector/chunk persistence if needed

SampleRag.Di/            # DI composition, Mapster global config
```

**Structure Decision**: Existing Clean Architecture layout is retained. New work: scope (knowledge group) enforcement, RAG pipeline in Application + Qdrant, auth middleware, feedback and add-owner endpoints, and PDF chunking/embedding in DocumentService or dedicated service.

## Complexity Tracking

| Item | Why needed | Simpler alternative rejected |
|------|------------|-----------------------------|
| Vector mapping in config/Infrastructure | Constitution: no persistence attributes on domain models | Keeping attributes on DocumentPageData would violate Principle VII. |
| Scope access store | FR-001b requires “which users can use which scope” | Without it, scope enforcement cannot be implemented. |

## Phase 0: Research Summary

See [research.md](./research.md) for:

- **PDF chunking**: Page-based vs fixed-size; overlap; metadata (document id, scope, page number) for source citation; C# libraries (e.g. PdfPig, iText, or SK document loaders if available) respecting constitution.
- **Scope model**: How to store and resolve “user U can use scope S” (e.g. scope–user table or claims).
- **Auth**: JWT validation and reading role/scope claims without introducing a new identity provider in scope.

## Phase 1: Design Artifacts

- [data-model.md](./data-model.md) — Entities and relationships aligned to spec (Scope, Chat+ScopeId+OwnerIds, Message+ChatId Guid, Document+ScopeId, Feedback, chunk/embedding model for Qdrant).
- [contracts/](./contracts/) — API contracts for scope, chats, messages (including “send without chatId”), documents, feedback, add-owner.
- [quickstart.md](./quickstart.md) — How to run the API and exercise main flows (create scope, upload PDF, create chat, send message, feedback).
