# Codebase Analysis: SampleRag API

## 📁 Project Structure

### Directory tree (up to 3rd level)

```
semantic-kernel-sample-rag-api/
├── .cursor/                    # Cursor IDE commands and config
├── .editorconfig               # Code style (braces, naming, format); constitution-aligned
├── .github/                    # GitHub config (e.g. workflows)
├── .specify/                   # Specify tooling (scripts, templates, constitution)
├── Directory.Build.props       # Enables code style in build; adds StyleCop.Analyzers to all .csproj
├── stylecop.json               # StyleCop settings (documentation, ordering, layout)
├── SampleRag.API/              # Web host, endpoints, filters, middleware
│   ├── Endpoints/              # Minimal API route groups (Chats, Messages, Documents, Files, KnowledgeScopes)
│   ├── Filters/               # Endpoint filters (validation, scope access)
│   ├── Hubs/                   # SignalR hub (DocumentsIndexingHub)
│   ├── Middleware/             # DevAuthHandler
│   ├── Properties/
│   └── wwwroot/assets/documents/
├── SampleRag.Application/      # Application services, jobs, kernel plugins
│   ├── DataGenerators/        # SemanticKernelDataGenerator
│   ├── Factories/             # PromptExecutionSettingsFactory
│   ├── Jobs/                  # ChunkVectorizationJob, DocumentChunkingJob
│   ├── KernelFunctions/Plugins/  # TimePlugin, RetrievalPlugin
│   └── Services/              # DocumentService, MessagesService, ChatService, etc.
├── SampleRag.Di/               # DI composition root (MongoDB, Qdrant, Kernel, Mapster)
├── SampleRag.Domain/           # Entities, DTOs, interfaces, config models
│   ├── Interfaces/            # IRepository, IVectorRepository, IFileRepository, service contracts
│   │   ├── Factories/
│   │   └── Services/
│   ├── Models/                # Domain entities and configs
│   │   ├── Abstractions/      # IEntity<TId>, IVectorEntity<TId,TVector>, IEntityWithScopeId
│   │   ├── Configs/           # DbSettings, VectorDbSettings, GenAiProviderSettings, etc.
│   │   └── Enums/
│   └── RequestModels/         # UploadDocumentRequestModel, SendMessageRequest, CreateGroupRequest
├── SampleRag.Infrastructure/   # Persistence, vector store, file storage, embedding generators
│   ├── EmbeddingGenerators/   # DocumentChunkEmbeddingGenerator, DocumentEmbeddingGenerator
│   └── Repositories/
│       ├── Files/             # LocalFileRepository
│       ├── Mongo/             # MongoBaseRepository<T>, KnowledgeScopeUserRepository
│       └── Vector/             # QdrantDocumentChunkRepository
└── specs/                     # Feature specifications and checklists
```

**Directory purposes**

- **SampleRag.API** — ASP.NET Core host: OpenAPI/Swagger, rate limiter config, JWT or Dev auth, and Minimal API endpoint groups. Validation is implemented via **endpoint filters** (e.g. `DocumentUploadValidationFilter`, `FileValidationFilter`, `ScopeUserAccessFilter`).
- **SampleRag.Application** — Application logic: RAG streaming (SemanticKernelDataGenerator + Kernel), document ingestion and chunking (DocumentService, DocumentChunkService, jobs), chat and message handling (MessagesService, ChatService). Depends on Domain only; services use **primary constructors**.
- **SampleRag.Domain** — Shared kernel: **all models** (entities and request DTOs) live here; attribute-free **class** types; vector entities implement `IVectorEntity<Guid, float>`. Interfaces for repositories and application services are defined in Domain.
- **SampleRag.Infrastructure** — Persistence: MongoDB generic repository, Qdrant vector repository with `VectorStoreCollectionDefinition`, local file store, and embedding generators. Uses primary constructors; base repository uses a protected field for the collection.
- **SampleRag.Di** — Composition: wires MongoDB (with Bson Guid serializer), Qdrant (with `EnsureCollectionsExistsAsync` at startup), Semantic Kernel (Ollama chat + embeddings), Mapster, and all application/infrastructure services.

**Code organization**

The solution follows **Clean Architecture** (API → Di → Application + Infrastructure; Application and Infrastructure → Domain). Grouping is by **layer**, not vertical slice. Endpoints are grouped by resource in static classes; validation is centralized in endpoint filters.

---

## 🛠 Technology Stack

| Category | Technology | Version / notes |
|----------|------------|------------------|
| **Framework** | ASP.NET Core (Minimal API) | net10.0 |
| **Runtime** | .NET | 10.0 |
| **DI** | Microsoft.Extensions.DependencyInjection | Via `WebApplication.CreateSlimBuilder`; registration in SampleRag.Di |
| **Data access** | MongoDB.Driver | 3.6.0; no EF Core |
| **Vector store** | Qdrant (Semantic Kernel connector + Qdrant.Client) | SK 1.70.0-preview; Qdrant.Client 1.15.1 |
| **AI / embeddings** | Microsoft Semantic Kernel, Ollama | SK 1.70.0; OllamaSharp 5.4.16 (via Di); embeddings mxbai-embed-large (1024 dims) |
| **Auth** | JWT Bearer or Dev handler | JwtSettings.Enabled → JWT; else `DevAuthHandler` scheme |
| **API docs** | Swagger/OpenAPI (Swashbuckle) | 10.1.2; OpenAPI in Development only |
| **Mapping** | Mapster / MapsterMapper | 7.4.0; global config and Scoped IMapper in Di |
| **Scheduling** | Quartz | 3.15.1 (referenced in Application; not wired in startup) |
| **MediatR** | MediatR | 14.0.0 (referenced in Application; not used in handlers) |
| **Testing** | — | No test projects found |

External services: **MongoDB** (primary store), **Qdrant** (vector DB; collections ensured at startup), **Ollama** (LLM and embeddings). File storage: **local filesystem** under `wwwroot/assets/documents`.

---

## 🏗 Architecture

**Layered flow**

- **API** — Endpoints inject application services; return `Results.*`; stream `IAsyncEnumerable<MessagePart>` for chat. All validations (document upload, file size/type, scope access) are implemented via **endpoint filters**, not inside handler logic.
- **Application** — DocumentService, DocumentChunkService, MessagesService, ChatService, ScopeAccessService; depend on `IRepository<Guid, T>`, `IVectorRepository<DocumentChunk>`, `IFileRepository`, `IDataGenerator`, `IScopeAccessService`, and Kernel. Services use **primary constructors** only.
- **Domain** — Entities (`Chat`, `Message`, `Document`, `DocumentChunk`, `KnowledgeScope`) implement `IEntity<Guid>`; vector models also `IVectorEntity<Guid, float>`. All models are **class**, no attributes; interfaces for repositories and services live in Domain.
- **Infrastructure** — `MongoBaseRepository<T>` (primary constructor; protected `_collection` for base), `QdrantDocumentChunkRepository` (uses `VectorStoreCollectionDefinition` with Id, PageNumber, ChunkIndex, Vector 1024, CosineSimilarity, Hnsw), `LocalFileRepository`, embedding generators.

**Dependency injection**

- **SampleRag.Di** registers repositories (Transient), application services (Transient), `IMongoDatabase` and settings (Singleton), Kernel and Qdrant vector store. `EnsureCollectionsExistsAsync` runs at startup to create missing Qdrant collections (vector size, distance, quantization from config). Kernel and Qdrant are configured in Di via `ConfigureAiDependencies` and `ConfigureDependencies` called from Program.cs.

**Repository pattern**

- Generic `IRepository<TId, TModel>`: `AddAsync`, `UpdateAsync`, `RemoveByIdsAsync`, `GetByIdsAsync`, `GetBatchByAsync(Expression<Func<TModel, bool>>?, int?)`. Implemented by `MongoBaseRepository<T>` (collection name = `typeof(TModel).Name`). `IVectorRepository<DocumentChunk>` for Qdrant upsert/search/delete by document Id.

**Endpoint organization**

- Static classes per resource: `ChatsEndpoints`, `MessagesEndpoints`, `DocumentsEndpoints`, `FilesEndpoints`, `KnowledgeGroupsEndpoints` (maps `api/knowledgescopes`). Each `MapGroup("api/...").WithTags("...")`. Chats and Messages use `.RequireAuthorization()`; Documents use validation filters.

**Middleware**

- `app.UseAuthentication()` and `app.UseAuthorization()`. Custom middleware sets `X-Content-Type-Options: no-sniff`. Rate limiter (fixed window: 4 req/12s, queue 2) is configured but **not applied** (no `app.UseRateLimiter()` or endpoint policy).

**Error handling and validation**

- No global exception handler. Validation: **endpoint filters** (`DocumentUploadValidationFilter` for name/scopeId/file; `FileValidationFilter` for size and PDF-only; `ScopeUserAccessFilter` for scope access). Repository handles `MongoBulkWriteException` by returning successfully inserted items.

---

## 🔌 API Design & Endpoints

**HTTP methods and REST usage**

- **POST** `api/chats` — create chat (201 Created); **GET** `api/chats` — list with `batchSize`, `lastUsedIndex` (200 OK); **DELETE** `api/chats/{id}` — 204 No Content.
- **POST** `api/messages` — send message; returns streaming `IAsyncEnumerable<MessagePart>` (text + optional CreatedAt).
- **POST** `api/documents` — upload document (body: `UploadDocumentRequestModel`); 201 Created; validated by `DocumentUploadValidationFilter` and `FileValidationFilter`.
- **GET** `api/files/...` — file download (path-based).
- **POST** `api/knowledgescopes` — create knowledge scope (201); **GET** `api/knowledgescopes` — list with `batchSize` (200); **POST/DELETE** `.../users` for scope users. Some routes require `RequireAdministrator`.

**Request/Response models**

- Domain entities used as API contracts where appropriate (`Chat`, `Message`, `KnowledgeScope`). Request DTOs: `UploadDocumentRequestModel` (Name, ScopeId, File with Content + FileName), `SendMessageRequest`, `CreateGroupRequest`. File upload uses base64 content in JSON.

**File upload**

- JSON body with base64 `File.Content` and `File.FileName`. `FileValidationFilter` enforces approximate 20 MB limit (via decoded size), PDF extension and content-type allowlist. Stored via `IFileRepository.SaveAsync` under wwwroot.

**Pagination / filtering**

- Cursor-style: `batchSize` and optional `lastUsedIndex` (chats). Repository `GetBatchByAsync` with expression. No standard `page`/`pageSize` or HATEOAS.

**Rate limiting**

- Fixed window configured; **not applied** (no `UseRateLimiter()` or policy on routes).

**Versioning**

- No URL or header versioning.

---

## 📦 Data Layer and Persistence

**Database**

- **MongoDB**: connection and database from `DbSettings`. `BsonSerializer` registers `GuidSerializer(GuidRepresentation.Standard)` in Di before client creation.

**Migrations**

- No EF Core. MongoDB is schema-less; schema implied by entity types. Qdrant collections are **ensured at startup** in Di: `EnsureCollectionsExistsAsync` lists collections and recreates missing ones with `VectorParams` (Size, Distance, QuantizationConfig from `VectorDbSettings.Collections`). Quantization: Binary, Scalar, or Product via `GetQuantizationConfig`.

**Data modeling**

- **Code-first**: entities in Domain, attribute-free classes. MongoDB collection name = type name. Vector store: `VectorStoreCollectionDefinition` with key/data/vector properties (e.g. Id, PageNumber, ChunkIndex, Vector 1024, CosineSimilarity, Hnsw) in `QdrantDocumentChunkRepository`.

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

- **Linter / style**: **`.editorconfig`** at repo root enforces constitution and project conventions: **IDE0011** (always use braces for if/else/for/foreach/while — Principle VIII), **csharp_prefer_braces = true**, naming (PascalCase for types/methods, async methods must end with **Async**), file-scoped namespaces, accessibility modifiers, trim trailing whitespace, final newline. **StyleCop.Analyzers** (1.1.118) is applied to all C# projects via **Directory.Build.props**; **stylecop.json** configures documentation/ordering/layout (require file header off; system usings first; newline at end of file). Constitution Principle IX (primary constructors in Application/Infrastructure) remains a convention; no analyzer enforces it.
- **Naming**: Consistent PascalCase and async suffix. **Typo**: `GenerateAiResponce` (should be `Response`) in `IMessagesService` and `MessagesService`.
- **Type safety**: Strong C# typing; nullable reference types enabled. Domain models used as DTOs where appropriate; request models in Domain.
- **Tests**: No test projects or test files.
- **API documentation**: Swagger enabled; no XML docs on endpoints or models in reviewed files.
- **Config files**: Root **`.editorconfig`** (braces, naming, formatting; references constitution); **`Directory.Build.props`** (enables code style in build, adds StyleCop.Analyzers to all `.csproj`); **`stylecop.json`** (documentation/ordering/layout).
- **Notable**: Rate limiter not applied; SignalR hub `DocumentsIndexingHub` present but not registered (`AddSignalR`/`MapHub` not in Program); MediatR and Quartz referenced but unused. FileValidationFilter reports "20 MB limit" but uses 1.5 MB constant — inconsistent.

---

## 🔧 Key Components

### 1. DocumentsEndpoints + validation filters (API)

Document upload with validation delegated to filters.

```csharp
group.MapPost("/", async ([FromBody] UploadDocumentRequestModel document,
    IDocumentService documentsService, CancellationToken ct) => { ... })
    .AddEndpointFilter<DocumentUploadValidationFilter>()
    .AddEndpointFilter<FileValidationFilter>()
    .Produces(StatusCodes.Status201Created)
    .Accepts<UploadDocumentRequestModel>("application/json");
```

**Purpose**: Ensures validation (required fields, scope, file presence, size, PDF-only) is done in filters, not in handler. **Dependencies**: IDocumentService; filters use request model and (for scope) IScopeAccessService.

---

### 2. QdrantDocumentChunkRepository (Infrastructure)

Vector store access with prescribed collection definition.

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
    // UpsertChunksAsync, RetrieveChunksAsync, RemoveByAsync(documentId)
}
```

**Purpose**: Upsert/search/delete document chunks in Qdrant with consistent schema. **Inputs/Outputs**: Chunks for upsert; query string + topK for search; documentId for delete.

---

### 3. DocumentService (Application)

Saves file to local storage and document metadata to MongoDB.

```csharp
public class DocumentService(
    IRepository<Guid, Document> documentsRepository,
    IFileRepository fileRepository) : IDocumentService
{
    public async Task<IEnumerable<Document>> AddAsync(params UploadDocumentRequestModel[] items)
    {
        var savingData = items.Select(x => new Document { Name = x.Name, ScopeId = x.ScopeId }).ToArray();
        for (var i = 0; i < savingData.Length; i++)
            savingData[i].LocalLink = await fileRepository.SaveAsync(items[i].File.Content, items[i].File.FileName);
        return await documentsRepository.AddAsync(savingData);
    }
}
```

**Purpose**: Orchestrates document upload (file + metadata). **Dependencies**: IRepository<Guid, Document>, IFileRepository.

---

### 4. ScopeUserAccessFilter (API)

Endpoint filter for scope-based access.

```csharp
public class ScopeUserAccessFilter(
    IScopeAccessService scopeAccessService,
    ClaimsPrincipal user) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null) return Results.BadRequest("Entity with ScopeId required");
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? "";
        if (!await scopeAccessService.CanUseScopeAsync(data.ScopeId, userId))
            return Results.Json(new { error = "No access to scope" }, statusCode: 403);
        return await next.Invoke(context);
    }
}
```

**Purpose**: Ensures caller has access to the scope of the entity before running the endpoint. **Dependencies**: IScopeAccessService, ClaimsPrincipal.

---

### 5. ServiceCollectionExtensions.ConfigureQdrant (Di)

Qdrant registration and collection ensure at startup.

```csharp
public static void ConfigureQdrant(this IServiceCollection services, VectorDbSettings vectorDbSettings)
{
    services.AddSingleton(vectorDbSettings);
    var qdrantClient = new QdrantClient(new Uri(vectorDbSettings!.Url));
    services.AddQdrantVectorStore(_ => qdrantClient, sp => new QdrantVectorStoreOptions { ... })
        .AddQdrantCollection<Guid, DocumentChunk>("document-chunks")
        .AddQdrantCollection<Guid, ApiDocument>("documents")
        .AddQdrantCollection<Guid, KnowledgeScope>("knowledge-groups");
    _ = Task.Run(() => EnsureCollectionsExistsAsync(qdrantClient, vectorDbSettings));
}
```

**Purpose**: Single place for Qdrant client, vector store, collections, and startup collection creation with config-driven vector size, distance, and quantization.

---

## 🔒 Security and Validation

- **Authentication/Authorization**: **JWT** when `JwtSettings.Enabled` (Authority, Audience, Issuer); otherwise **Dev** scheme (`DevAuthHandler`). Policies: `RequireAdministrator` for some knowledge-scope operations. Chats and Messages use `RequireAuthorization()`.
- **Input validation**: **Endpoint filters** only (no FluentValidation or DataAnnotations on models): `DocumentUploadValidationFilter` (name length, scopeId, file presence), `FileValidationFilter` (content required, size, PDF extension), `ScopeUserAccessFilter` (scope access).
- **File upload**: Size and PDF-only enforced in `FileValidationFilter`; error message says 20 MB but constant is 1.5 MB — should be aligned with constitution (20 MB).
- **CORS**: Not configured in reviewed code.
- **HTTPS**: JwtSettings has `RequireHttpsMetadata` (configurable).
- **Sensitive data**: Config in appsettings.json (MongoDB, Qdrant, Ollama URLs); no secrets manager usage shown. Header `X-Content-Type-Options: no-sniff` set.

---

## ⚙️ Performance and Infrastructure

- **Build**: SDK `Microsoft.NET.Sdk.Web` (API) and `Microsoft.NET.Sdk` (others); net10.0; nullable and implicit usings; PublishAot false. No Dockerfile in the analyzed tree.
- **Dev setup**: `.specify/scripts` (PowerShell) for prerequisites; no single-command dev script in root.
- **CI/CD**: `.github` present; workflow content not verified.
- **Health/monitoring**: No health checks or readiness/liveness endpoints.

---

## 📋 Summary & Recommendations

**Summary**

SampleRag is a **Clean Architecture** ASP.NET Core Minimal API for a RAG demo: chats, messages (streaming LLM via Semantic Kernel + Ollama), document upload (base64 JSON → local disk + MongoDB), knowledge scopes with user association, and file download. Persistence: MongoDB and local files; vector store: Qdrant with startup collection ensure. **Validation is implemented via endpoint filters**; models are in Domain, attribute-free classes; vector entities implement `IVectorEntity`; Application/Infrastructure use primary constructors. Auth: JWT or Dev handler. Rate limiter, SignalR hub, MediatR, and Quartz are configured or referenced but not fully wired.

**Strengths**

- Clear layer separation and dependency rule (API → Di → Application + Infrastructure → Domain).
- Endpoint filters for validation and scope access keep handlers thin.
- Vector store and collection definition aligned with constitution (VectorStoreCollectionDefinition, EnsureCollectionsExistsAsync).
- Domain-centric models and primary constructors in Application/Infrastructure.

**Recommendations**

1. **Apply rate limiting**: Call `app.UseRateLimiter()` and/or attach a policy to endpoint groups.
2. **Fix typo**: Rename `GenerateAiResponce` → `GenerateAiResponse` in interface and implementation.
3. **Align file size**: Make `FileValidationFilter` use 20 MB (constitution) and fix the constant/error message.
4. **Remove or use optional pieces**: Register and map `DocumentsIndexingHub` if needed, or remove; same for MediatR and Quartz if not planned.
5. **Add tests**: Introduce a test project (e.g. xUnit) for services, repositories, and filters.
6. **Observability**: Add health checks (MongoDB, Qdrant, optional Ollama), structured logging, and correlation IDs for production.
7. **Security**: Enforce HTTPS and CORS where needed; consider multipart upload and stronger content validation for production.

**Project complexity**

Suitable for **mid-level** developers: Clean Architecture, async streaming, and filter-based validation are clear; hardening (tests, observability, consistent use of configured features) requires some discipline. **~3,400 words**
