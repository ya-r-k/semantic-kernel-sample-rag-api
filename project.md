# Codebase Analysis: SampleRag API

## 📁 Project Structure

### Directory tree (up to 3rd level)

```
semantic-kernel-sample-rag-api/
├── .editorconfig               # Code style (braces, naming, formatting)
├── .github/                    # GitHub config (e.g. workflows)
├── Directory.Build.props       # EnforceCodeStyleInBuild; StyleCop.Analyzers for all .csproj
├── stylecop.json               # StyleCop settings (referenced by Infrastructure)
├── SampleRag.API/              # Web host, endpoints, filters, middleware
│   ├── Endpoints/              # Minimal API route groups (Chats, Messages, Documents, Files, KnowledgeScopes)
│   ├── Filters/                # Endpoint filters (DocumentUpload, File, ScopeUserAccess)
│   ├── Hubs/                   # SignalR hub (DocumentsIndexingHub — not registered)
│   ├── Middleware/             # DevAuthHandler
│   └── Properties/
├── SampleRag.Application/      # Application services, jobs, Semantic Kernel plugins
│   ├── Factories/              # PromptExecutionSettingsFactory
│   ├── Jobs/                   # ChunkVectorizationJob, DocumentChunkingJob (Quartz)
│   ├── Plugins/                # TimePlugin, RetrievalPlugin (Kernel plugins)
│   └── Services/               # DocumentService, DocumentChunkService, MessagesService, KnowledgeScopeUserService
├── SampleRag.Di/               # DI composition root (MongoDB, Qdrant, Kernel, Mapster)
│   └── Mapping/                # Mapster config (Message↔ChatMessageContent, tool calls, etc.)
├── SampleRag.Domain/            # Entities, DTOs, interfaces, config models
│   ├── Interfaces/             # IRepository, IVectorRepository, IFileRepository, service contracts
│   │   ├── Factories/
│   │   └── Services/
│   ├── Models/                 # Domain entities and configs
│   │   ├── Abstractions/       # IEntity<TId>, IVectorEntity<TId,TVector>, IEntityWithScopeId
│   │   ├── Configs/            # DbSettings, VectorDbSettings, GenAiProviderSettings, JwtSettings
│   │   └── Enums/              # GenerationStep, AiTool
│   └── RequestModels/          # UploadDocumentRequestModel, SendMessageRequest, CreateChatRequest, GetBatchByModel
├── SampleRag.Infrastructure/   # Persistence, vector store, file storage, embedding generators
│   ├── DataGenerators/         # SemanticKernelDataGenerator
│   ├── EmbeddingGenerators/    # DocumentChunkEmbeddingGenerator, DocumentEmbeddingGenerator
│   └── Repositories/
│       ├── Files/              # LocalFileRepository
│       ├── Mongo/              # MongoBaseRepository<T>, KnowledgeScopeRepository, KnowledgeScopeUserRepository
│       └── Vector/             # QdrantDocumentChunkRepository
├── Scripts/                    # docker.run-deps.bat, docker.run-api.bat, backup/restore
└── specs/                      # Feature specifications and checklists
```

**Directory purposes**

- **SampleRag.API** — ASP.NET Core host: OpenAPI/Swagger, rate limiter config, JWT or Dev auth, Minimal API endpoint groups. Validation via **endpoint filters** (`DocumentUploadValidationFilter`, `FileValidationFilter`, `ScopeUserAccessFilter`).
- **SampleRag.Application** — Application logic: RAG streaming (`SemanticKernelDataGenerator` + Kernel), document ingestion and chunking (`DocumentService`, `DocumentChunkService`, jobs), chat/message handling (`MessagesService`). **Plugins** (`TimePlugin`, `RetrievalPlugin`) live under `Plugins/`. Depends on Domain only; services use **primary constructors**.
- **SampleRag.Domain** — Shared kernel: entities and request DTOs; attribute-free classes; vector entities implement `IVectorEntity<Guid, float>`. Interfaces for repositories and application services are in Domain.
- **SampleRag.Infrastructure** — Persistence: MongoDB generic repository, Qdrant vector repository (Microsoft.Extensions.VectorData), local file store, embedding generators. Uses primary constructors.
- **SampleRag.Di** — Composition: wires MongoDB (Bson Guid serializer), Qdrant (collections ensured at startup), Semantic Kernel (Ollama chat + embeddings), Mapster, and all application/infrastructure services.

**Code organization**

The solution follows **Clean Architecture** (API → Di → Application + Infrastructure; Application and Infrastructure depend on Domain). Grouping is by **layer**. Endpoints are grouped by resource in static classes; validation is centralized in endpoint filters. **Chats** are handled directly by endpoints injecting `IRepository<Guid, Chat>` (no dedicated ChatService).

---

## 🛠 Technology Stack

| Category | Technology | Version / notes |
|----------|------------|------------------|
| **Framework** | ASP.NET Core (Minimal API) | net10.0 |
| **Runtime** | .NET | 10.0 |
| **DI** | Microsoft.Extensions.DependencyInjection | Via `WebApplication.CreateSlimBuilder`; registration in SampleRag.Di |
| **Data access** | MongoDB.Driver | 3.6.0; no EF Core |
| **Vector store** | Qdrant (Microsoft.Extensions.VectorData.Abstractions, Semantic Kernel connector) | SK 1.72.0-preview; Qdrant.Client 1.17.0 |
| **AI / embeddings** | Microsoft Semantic Kernel, Ollama | SK 1.72.0; OllamaSharp 5.4.16; Ollama connector 1.72.0-alpha; embeddings e.g. mxbai-embed-large (1024 dims) |
| **Auth** | JWT Bearer or Dev handler | `JwtSettings.Enabled` → JWT (Authority, Audience, Issuer); else `DevAuthHandler` scheme |
| **API docs** | Swagger/OpenAPI (Swashbuckle) | 10.1.4; OpenAPI in Development only |
| **Mapping** | Mapster / MapsterMapper | 7.4.0; global config and Scoped IMapper in Di |
| **Scheduling** | Quartz | 3.15.1 (Application; jobs not wired in Program.cs) |
| **MediatR** | MediatR | 14.0.0 (referenced in Application; no handlers found) |
| **Testing** | — | No test projects in solution |

**External services:** MongoDB (primary store), Qdrant (vector DB; collections ensured at startup), Ollama (LLM and embeddings). File storage: **local filesystem** under `wwwroot/assets/documents`. Scripts: `Scripts/docker.run-deps.bat` for Qdrant, MongoDB, Ollama containers.

---

## 🏗 Architecture

**Layered flow**

- **API** — Endpoints inject application services or repositories; return `Results.*`; stream `IAsyncEnumerable<MessagePart>` for chat. Validations (document upload, file size/type, scope access) are in **endpoint filters**.
- **Application** — DocumentService, DocumentChunkService, MessagesService, KnowledgeScopeUserService; depend on `IRepository<Guid, T>`, `IVectorRepository<DocumentChunk>`, `IFileRepository`, `IDataGenerator`, Kernel. Services use **primary constructors**. Chats: create/list/delete are in **ChatsEndpoints** using `IRepository<Guid, Chat>` directly.
- **Domain** — Entities (`Chat`, `Message`, `Document`, `DocumentChunk`, `KnowledgeScope`) implement `IEntity<Guid>`; vector models also `IVectorEntity<Guid, float>`. Interfaces for repositories and services in Domain.
- **Infrastructure** — `MongoBaseRepository<T>` (primary constructor; protected `_collection`), `QdrantDocumentChunkRepository` (VectorStoreCollectionDefinition: Id, PageNumber, ChunkIndex, Vector 1024, CosineSimilarity, Hnsw), `LocalFileRepository`, embedding generators, `SemanticKernelDataGenerator`.

**Dependency injection**

- **SampleRag.Di** registers repositories and application services (Transient), `IMongoDatabase` and settings (Singleton), Kernel and Qdrant vector store. `EnsureCollectionsExistsAsync` runs at startup to create missing Qdrant collections (vector size, distance, quantization from config). Kernel and Qdrant configured via `ConfigureAiDependencies` and `ConfigureDependencies` from Program.cs.

**Repository pattern**

- Generic `IRepository<TId, TModel>`: `AddAsync`, `UpdateAsync`, `RemoveByIdsAsync`, `GetByIdsAsync`, `GetBatchByAsync(Expression<Func<TModel, bool>>?, int?)`. Implemented by `MongoBaseRepository<T>` (collection name = `typeof(TModel).Name`). `IVectorRepository<DocumentChunk>`: `UpsertChunksAsync`, `RetrieveChunksAsync` (by query or scopeId+query), `RemoveByAsync(documentId)`.

**Endpoint organization**

- Static classes per resource: `ChatsEndpoints`, `MessagesEndpoints`, `DocumentsEndpoints`, `FilesEndpoints`, `KnowledgeScopesEndpoints` (route prefix `api/knowledgescopes`). Chats and Messages use `.RequireAuthorization()`; Chats and create-chat use `ScopeUserAccessFilter`; Documents use `DocumentUploadValidationFilter` and `FileValidationFilter`. Knowledge scope create/user management use `RequireAdministrator`.

**Middleware**

- `app.UseAuthentication()` and `app.UseAuthorization()`. Custom middleware sets `X-Content-Type-Options: no-sniff`. Rate limiter (fixed window: 4 req/12s, queue 2) is configured but **not applied** (no `app.UseRateLimiter()` or endpoint policy).

**Error handling and validation**

- No global exception handler. Validation: **endpoint filters** only. Repository handles `MongoBulkWriteException` by returning successfully inserted items.

---

## 🔌 API Design & Endpoints

**HTTP methods and REST usage**

- **POST** `api/chats` — create chat (body: `CreateChatRequest`: Name, ScopeId, OwnerIds); 201 Created; `ScopeUserAccessFilter`.
- **GET** `api/chats` — list with `batchSize`, `lastUsedIndex` (Guid cursor); 200 OK.
- **DELETE** `api/chats/{id}` — 204 No Content.
- **POST** `api/messages` — send message; returns streaming `IAsyncEnumerable<MessagePart>`.
- **POST** `api/documents` — upload document (body: `UploadDocumentRequestModel`); 201 Created; filters for validation and file rules.
- **GET** `api/files/assets/documents/{fileName}` — file download (PDF).
- **POST** `api/knowledgescopes` — create scope (RequireAdministrator); **POST** `api/knowledgescopes/filter` — list with `GetBatchByModel`; **POST/DELETE** `.../users` for scope users.

**Request/Response models**

- Request DTOs in Domain: `UploadDocumentRequestModel` (Name, ScopeId, File with base64 Content + FileName), `SendMessageRequest`, `CreateChatRequest` (Name, ScopeId, OwnerIds), `CreateGroupRequest`, `GetBatchByModel`, `AddScopeUserRequest`. `CreateChatRequest` implements `IEntityWithScopeId` for `ScopeUserAccessFilter`.

**File upload**

- JSON body with base64 `File.Content` and `File.FileName`. `FileValidationFilter` enforces size limit (constant 1.5 MB; error message says "20 MB" — **inconsistent**), PDF extension and content-type allowlist. Stored via `IFileRepository.SaveAsync` under wwwroot.

**Pagination / filtering**

- Cursor-style: `batchSize` and optional `lastUsedIndex` (Guid for chats). Repository `GetBatchByAsync` with expression. No URL versioning or HATEOAS.

**Rate limiting**

- Fixed window configured in Program.cs; **not applied** (no `UseRateLimiter()` or policy on routes).

---

## 📦 Data Layer and Persistence

**Database**

- **MongoDB**: connection and database from `DbSettings`. `BsonSerializer` registers `GuidSerializer(GuidRepresentation.Standard)` in Di before client creation.

**Migration strategies**

- No EF Core. MongoDB is schema-less; schema implied by entity types. Qdrant collections **ensured at startup** in Di: `EnsureCollectionsExistsAsync` lists collections and recreates missing ones with `VectorParams` (Size, Distance, QuantizationConfig from `VectorDbSettings.Collections`). Quantization: Binary, Scalar, or Product via `GetQuantizationConfig`.

**Data modeling**

- **Code-first**: entities in Domain, attribute-free classes. MongoDB collection name = type name. Vector store: `VectorStoreCollectionDefinition` with key/data/vector properties in `QdrantDocumentChunkRepository`.

**File storage**

- **Local filesystem**: `LocalFileRepository` under `WebRootPath` + `assets/documents`; saves base64-decoded bytes.

**Caching / transactions**

- No Redis or in-memory cache. No explicit transaction or distributed transaction handling.

---

## 📋 Logging and Observability

- **Logging**: Default ASP.NET Core logging (appsettings: Default Information, Microsoft.AspNetCore Warning). No Serilog/NLog or structured logging.
- **Destinations**: Console; no file or external sink (Seq, ELK, Application Insights).
- **Correlation**: No correlation ID or trace IDs in logs.
- **Health checks**: None (`MapHealthChecks`/`AddHealthChecks` not used).
- **Monitoring**: No custom metrics or APM.

---

## ✅ Code Quality

- **Linter / style**: **`.editorconfig`** at repo root: braces, naming (PascalCase, interface prefix `I`), formatting, `csharp_prefer_braces = true`, `csharp_style_namespace_declarations = block_scoped`, primary constructor preference. **StyleCop.Analyzers** (1.1.118) applied via **Directory.Build.props**; **stylecop.json** referenced in Infrastructure for ordering/layout. **Directory.Build.props**: `EnforceCodeStyleInBuild`, `AnalysisLevel = latest-recommended`.
- **Naming**: Consistent PascalCase. **Typo**: `GenerateAiResponce` (should be `GenerateAiResponse`) in `IMessagesService` and `MessagesService`.
- **Type safety**: Strong C# typing; nullable reference types enabled. Domain models and request models in Domain.
- **Tests**: No test projects or test files.
- **API documentation**: Swagger enabled; no XML docs on endpoints or models in reviewed files.
- **Notable**: Rate limiter not applied; SignalR hub `DocumentsIndexingHub` present but not registered (`AddSignalR`/`MapHub` not in Program); MediatR and Quartz referenced but not wired. FileValidationFilter: 1.5 MB constant vs "20 MB" message. **RetrievalPlugin** currently returns mock chunks (`Task.FromResult` with fake `DocumentChunk[]`) instead of calling `chunkRepository.RetrieveChunksAsync` — placeholder for RAG retrieval.

---

## 🔧 Key Components

### 1. ChatsEndpoints + ScopeUserAccessFilter (API)

Chat creation with scope access enforced by filter; repository injected directly.

```csharp
group.MapPost("/", async ([FromBody] CreateChatRequest request, IRepository<Guid, Chat> chatRepository, ClaimsPrincipal user, CancellationToken ct) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "unknown";
    var ownerIds = request.OwnerIds?.Length > 0 ? request.OwnerIds : [userId];
    var chat = new Chat { Name = request.Name, ScopeId = request.ScopeId, OwnerIds = ownerIds };
    var result = await chatRepository.AddAsync(chat);
    // ...
})
    .RequireAuthorization()
    .AddEndpointFilter<ScopeUserAccessFilter>()
```

**Purpose**: Create chat with optional owner list; scope access via `IKnowledgeScopeUserService.HasAccessAsync`. **Dependencies**: `IRepository<Guid, Chat>`, `ScopeUserAccessFilter` (uses `IEntityWithScopeId`).

---

### 2. MessagesService (Application)

Streaming RAG response: new chat creation, history load, Semantic Kernel streaming, message persist.

```csharp
public async IAsyncEnumerable<MessagePart> GenerateAiResponce(SendMessageRequest request, string userId)
{
    var userMessage = request.Adapt<Message>();
    if (userMessage.ChatId.Equals(Guid.Empty))
    {
        var chat = userMessage.Adapt<Chat>();
        await chatRepository.AddAsync(chat);
        yield return chat.Adapt<MessagePart>();
    }
    await foreach (var part in GenerateAiMessage(userMessage))
        yield return part;
}
```

**Purpose**: Orchestrates chat creation when needed and streaming LLM response via `IDataGenerator.GenerateStreamingData(messagesHistory.Append(userMessage), "naive-rag")`. **Dependencies**: IDataGenerator, IRepository<Guid, Chat>, IRepository<Guid, Message>.

---

### 3. QdrantDocumentChunkRepository (Infrastructure)

Vector store access with prescribed collection definition and scope-aware retrieval.

```csharp
public class QdrantDocumentChunkRepository(
    IEmbeddingGenerator<DocumentChunk, Embedding<float>> embeddingGenerator,
    VectorStore vectorStore,
    VectorDbSettings settings) : IVectorRepository<DocumentChunk>
{
    private readonly VectorStoreCollection<Guid, DocumentChunk> vectorCollection =
        vectorStore.GetCollection<Guid, DocumentChunk>("document-chunks", new VectorStoreCollectionDefinition
        {
            EmbeddingGenerator = embeddingGenerator,
            Properties =
            [
                new VectorStoreKeyProperty("Id", typeof(Guid)),
                new VectorStoreDataProperty("PageNumber", typeof(int)),
                new VectorStoreDataProperty("ChunkIndex", typeof(int?)),
                new VectorStoreVectorProperty("Vector", typeof(ReadOnlyMemory<float>), dimensions: 1024)
                { DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw },
            ]
        });
    // UpsertChunksAsync, RetrieveChunksAsync(query), RetrieveChunksAsync(scopeId, query), RemoveByAsync(documentId)
}
```

**Purpose**: Upsert/search/delete document chunks in Qdrant; scope-filtered overload for RAG. **Inputs/Outputs**: Chunks for upsert; query + topK (and optional scopeId) for search; documentId for delete.

---

### 4. ScopeUserAccessFilter (API)

Endpoint filter for scope-based access using `IKnowledgeScopeUserService.HasAccessAsync`.

```csharp
public class ScopeUserAccessFilter(
    IKnowledgeScopeUserService scopeAccessService,
    ClaimsPrincipal user) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null) return Results.BadRequest("Entity with ScopeId required");
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
        if (!await scopeAccessService.HasAccessAsync(data.ScopeId, userId))
            return Results.Json(new { error = "No access to scope" }, statusCode: 403);
        return await next.Invoke(context);
    }
}
```

**Purpose**: Ensures caller has access to the scope of the entity before running the endpoint. **Dependencies**: IKnowledgeScopeUserService, ClaimsPrincipal.

---

### 5. ServiceCollectionExtensions.ConfigureQdrant (Di)

Qdrant registration and collection ensure at startup.

```csharp
services.AddQdrantVectorStore(_ => qdrantClient, sp => new QdrantVectorStoreOptions { ... })
    .AddQdrantCollection<Guid, DocumentChunk>("document-chunks")
    .AddQdrantCollection<Guid, ApiDocument>("documents")
    .AddQdrantCollection<Guid, KnowledgeScope>("knowledge-groups");
_ = Task.Run(() => EnsureCollectionsExistsAsync(qdrantClient, vectorDbSettings));
```

**Purpose**: Single place for Qdrant client, vector store, collections, and startup collection creation with config-driven vector size, distance, and quantization.

---

## 🔒 Security and Validation

- **Authentication/Authorization**: **JWT** when `JwtSettings.Enabled` (Authority, Audience, Issuer); otherwise **Dev** scheme (`DevAuthHandler`). Policies: `RequireAdministrator` for knowledge-scope create and user management. Chats and Messages use `RequireAuthorization()`.
- **Input validation**: **Endpoint filters** only (no FluentValidation or DataAnnotations on models): `DocumentUploadValidationFilter` (name length, scopeId, file presence), `FileValidationFilter` (content, size, PDF extension), `ScopeUserAccessFilter` (scope access via `HasAccessAsync`).
- **File upload**: Size and PDF-only in `FileValidationFilter`; error message says 20 MB but constant is 1.5 MB — align constant/message.
- **CORS**: Not configured in reviewed code.
- **HTTPS**: JwtSettings has `RequireHttpsMetadata` (configurable).
- **Sensitive data**: Config in appsettings.json (MongoDB, Qdrant, Ollama URLs); no secrets manager shown. Header `X-Content-Type-Options: no-sniff` set.

---

## ⚙️ Performance and Infrastructure

- **Build**: SDK `Microsoft.NET.Sdk.Web` (API) and `Microsoft.NET.Sdk` (others); net10.0; nullable and implicit usings; PublishAot false. **Directory.Build.props**: EnforceCodeStyleInBuild, StyleCop.Analyzers for all .csproj.
- **Dev setup**: **Scripts/docker.run-deps.bat** for Qdrant, MongoDB, Ollama containers; **Scripts/docker.run-api.bat** for API. No single-command dev script in root.
- **CI/CD**: `.github` present; workflow content not verified.
- **Docker**: Scripts use `docker run` for qdrant, mongodb, ollama with volumes and resource limits. No Dockerfile for the API in the listed tree.
- **Health/monitoring**: No health checks or readiness/liveness endpoints.

---

## 📋 Summary & Recommendations

**Summary**

SampleRag is a **Clean Architecture** ASP.NET Core Minimal API for a RAG demo: chats (create/list/delete via repository), messages (streaming LLM via Semantic Kernel + Ollama), document upload (base64 JSON → local disk + MongoDB), knowledge scopes with user association, and file download. Persistence: MongoDB and local files; vector store: Qdrant with startup collection ensure. **Validation is in endpoint filters**; models in Domain; vector entities implement `IVectorEntity`; Application/Infrastructure use primary constructors. Auth: JWT or Dev handler. **ChatService removed** — ChatsEndpoints use `IRepository<Guid, Chat>` directly. **Plugins** moved to `Application/Plugins/`. Rate limiter, SignalR hub, MediatR, and Quartz are configured or referenced but not fully wired. **RetrievalPlugin** currently returns mock data; vector retrieval exists in `QdrantDocumentChunkRepository.RetrieveChunksAsync(scopeId, query)`.

**Strengths**

- Clear layer separation and dependency rule (API → Di → Application + Infrastructure → Domain).
- Endpoint filters for validation and scope access keep handlers thin.
- Vector store and collection definition with EnsureCollectionsExistsAsync; scope-aware retrieval in repository.
- Domain-centric models and primary constructors in Application/Infrastructure.
- CreateChatRequest with OwnerIds and IEntityWithScopeId for consistent scope filtering.

**Recommendations**

1. **Apply rate limiting**: Call `app.UseRateLimiter()` and/or attach a policy to endpoint groups.
2. **Fix typo**: Rename `GenerateAiResponce` → `GenerateAiResponse` in IMessagesService and MessagesService.
3. **Align file size**: Make FileValidationFilter use 20 MB (or fix error message to match 1.5 MB) and document limit in one place.
4. **Wire RetrievalPlugin**: Replace mock return in `RetrieveRelevantChunksAsync` with `chunkRepository.RetrieveChunksAsync` (and pass scope/query from context if needed).
5. **Remove or use optional pieces**: Register and map DocumentsIndexingHub if needed, or remove; same for MediatR and Quartz if not planned.
6. **Add tests**: Introduce a test project (e.g. xUnit) for services, repositories, and filters.
7. **Observability**: Add health checks (MongoDB, Qdrant, optional Ollama), structured logging, and correlation IDs for production.
8. **Security**: Enforce HTTPS and CORS where needed; consider multipart upload and stronger content validation for production.

**Project complexity**

Suitable for **mid-level** developers: Clean Architecture, async streaming, filter-based validation, and vector/RAG concepts are clear; hardening (tests, observability, consistent use of configured features, real RAG in RetrievalPlugin) requires some discipline.
