# Project deep-dive: SampleRag API

## Intended use & consuming projects

Not specified by maintainer. Operationally, this API is meant for an isolated internal environment inside the organization, so the likely consumers are internal web clients or service clients that need scoped RAG chat, document ingestion, and feedback capture.

The most important product constraints are response time, scoped access control, and answering in the same language as the user. Document language is not required to match the question language; retrieval quality matters more than document language. No automated testing plan is currently scheduled.

## API purpose & public surface

SampleRag is an ASP.NET Core Minimal API solution that wraps a scoped Retrieval-Augmented Generation workflow around MongoDB, Qdrant, Ollama, and Semantic Kernel. It is not a library in the narrow sense; it is a layered application with a clear composition root and service boundaries.

Main projects:

- `SampleRag.API` - HTTP host, endpoint groups, filters, auth, rate limiting, Swagger, Serilog
- `SampleRag.Application` - orchestration services, plugins, filters, background jobs, prompt-related helpers
- `SampleRag.Domain` - entities, request models, abstractions, service contracts, config models
- `SampleRag.Infrastructure` - MongoDB, local file storage, Qdrant, Semantic Kernel data generation, embeddings
- `SampleRag.Di` - dependency registration and composition root

Public surface is centered around these endpoint groups:

- `api/chats`
- `api/messages`
- `api/documents`
- `api/files`
- `api/knowledgescopes`
- `api/feedbacks`

The main runtime orchestration happens through `MessagesService`, `DocumentService`, `KnowledgeScopeService`, `ChatService`, `DocumentChunkService`, `SemanticKernelDataGenerator`, and the endpoint filters that guard scope and chat access.

## Project structure

The solution is organized by runtime layer, which fits the current application reasonably well:

```text
SampleRag.API/
SampleRag.Application/
SampleRag.Domain/
SampleRag.Infrastructure/
SampleRag.Di/
Scripts/
specs/
```

- `SampleRag.API` contains the host entry point, endpoint groups, auth, CORS, rate limiting, and validation/access filters.
- `SampleRag.Application` contains business orchestration, Quartz jobs, Semantic Kernel plugins, and invocation filters.
- `SampleRag.Domain` contains shared contracts and request/response models used across layers.
- `SampleRag.Infrastructure` contains persistence and external integration code.
- `SampleRag.Di` is the composition root that wires everything together.
- `Scripts` contains local orchestration scripts for dependencies and running the API.
- `specs` contains the feature-spec documentation that should stay aligned with the actual endpoint contract.

Assessment:

- The layering is good for a modular API. The dependency direction is mostly easy to follow, and the host stays thin.
- The structure is a little less strict than a pure domain-driven system because HTTP request models live in `Domain`. That is acceptable for a small API, but it blurs the line between domain contracts and transport contracts.
- Endpoint logic is grouped by resource, which helps discoverability.
- Filters are split by purpose, which is a good fit for the access-control model.
- `SampleRag.Application` is doing several jobs at once: orchestration, plugins, filters, and jobs. That is not wrong, but it means the folder needs discipline so it does not become a catch-all.

## Technology stack

Runtime and language:

- .NET 10 / `net10.0` across all projects
- nullable reference types enabled
- implicit usings enabled
- primary constructors used in many services and repositories
- async streams used for chat response streaming
- `InvariantGlobalization=true` on the API host
- `PublishAot=false`

Build and code-style tooling:

- `Directory.Build.props` enables `EnforceCodeStyleInBuild`
- `AnalysisLevel=latest-recommended`
- `StyleCop.Analyzers` is applied solution-wide
- `.editorconfig` enforces folder/name alignment, braces, primary constructors, PascalCase, and namespace conventions
- warnings are not treated as errors, so style is enforced more than correctness

Key packages and why they matter:

- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT auth
- `Swashbuckle.AspNetCore` and `Microsoft.AspNetCore.OpenApi` - OpenAPI/Swagger
- `Serilog` and Serilog sinks/enrichers - request and application logging
- `Mapster` - object adaptation across API, domain, and Semantic Kernel boundaries
- `Microsoft.SemanticKernel` plus Ollama and Qdrant connectors - LLM orchestration and vector retrieval
- `OllamaSharp` - Ollama integration
- `MongoDB.Driver` - primary document store
- `Microsoft.Extensions.VectorData.Abstractions` and `Qdrant.Client` - vector store abstraction and search
- `Quartz` - background jobs for chunking and vectorization
- `PdfPig` - document extraction support
- `MediatR` - referenced but not central to the current runtime flow

External services:

- MongoDB for primary persistence
- Qdrant for vector search
- Ollama for chat completion and embeddings
- local filesystem under `wwwroot/assets/documents` for uploaded files

## Design patterns & architecture

### Layered architecture / Clean Architecture

The solution follows a layered design with `API -> Di -> Application + Infrastructure -> Domain`. That is the strongest architectural feature in the repo. The API host remains mostly declarative, while persistence and AI orchestration are pushed out of the route handlers.

This is a good fit for an internal API because it makes the runtime graph easy to reason about and keeps cross-cutting concerns centralized. The downside is that some transport contracts still live in `Domain`, so the boundary is practical rather than pure.

### Dependency injection and composition root

`SampleRag.Di` is a real composition root. It registers MongoDB, Qdrant, Semantic Kernel, the file repository, the service layer, and background jobs. The host program only calls the registration methods.

That placement is correct. It keeps the API host readable and makes the wiring easy to audit. The main caution is that the composition layer is now the place where many subsystems meet, so startup behavior and registration order matter.

### Repository pattern

MongoDB and Qdrant are both wrapped behind repository abstractions. Mongo persistence is handled by generic and specialized repositories, while vector search is encapsulated in `QdrantDocumentChunkRepository`.

This is a sensible use of the repository pattern here because the app has two very different persistence models:

- document records and access data in MongoDB
- chunk embeddings and similarity search in Qdrant

The pattern is mostly correct, but it should stay narrow. The repository interfaces should describe storage behavior, not business rules.

### Endpoint filters as policy objects

Endpoint filters are used well and consistently for:

- validating request bodies
- enforcing scope access
- checking chat access
- verifying route-scope ownership for file download

This is a strong design choice because it keeps access policy near the boundary and avoids repeating checks in every handler. The implementation is more explicit than a middleware-only approach and better aligned with endpoint-specific rules.

### Semantic Kernel plugin pipeline

Semantic Kernel is used as a plugin-driven orchestration layer rather than as a black box. The app registers typed plugins such as `TimePlugin` and `RetrievalPlugin`, plus prompt-directory YAML plugins from config. Function-choice behavior is selected by execution setting, which gives the app a controlled tool-use pipeline.

This is the right abstraction level for a RAG application. The weakness is that the current model-facing function contract is not fully sanitized for hidden parameters. The runtime can inject values, but the metadata path still needs tightening.

### Adapter / mapping layer

Mapster is used throughout the solution to adapt between request models, entities, and Semantic Kernel types. That works as a light adapter layer and keeps endpoint handlers small.

This is good placement for Mapster because the project has many DTO-to-entity conversions and response-shaping steps. The trade-off is that mapping rules can become implicit unless they are kept close to the domain model or covered by tests.

### Streaming pipeline

Message generation is streamed to the client using SSE. That is the correct UX choice for a chat-first API because it improves perceived latency and keeps the client responsive while the model is still generating.

The important point is that streaming does not eliminate latency; it only improves time-to-first-token and perceived responsiveness. Retrieval, embedding, chat history loading, prompt assembly, and function invocation still all happen before or during streaming.

### Background jobs

Quartz jobs are part of the ingestion pipeline and help keep document processing off the request path. That is a good architectural fit for response-time goals because it separates upload acknowledgement from heavier chunking/vectorization work.

## Code excerpts worth noting

The message endpoint is a good example of the boundary style: auth, scope checks, and chat checks are pushed into filters, while the handler stays thin.

```30:39:SampleRag.API/Endpoints/MessagesEndpoints.cs
group.MapPost("/", ([FromBody] SendMessageRequest message, IMessagesService messagesService, ClaimsPrincipal user) =>
{
    var userId = user.Adapt<string>();

    return Results.ServerSentEvents(messagesService.GenerateAiResponce(message, userId));
})
    .RequireAuthorization()
    .RequireRateLimiting("send-message")
    .AddEndpointFilter<BodyScopeAccessFilter>()
    .AddEndpointFilter<ChatAccessFilter>();
```

The body-scope filter is the main access-control building block for request models that carry a `ScopeId`.

```8:35:SampleRag.API/Filters/BodyScopeAccessFilter.cs
public class BodyScopeAccessFilter(
    IKnowledgeScopeService scopeService) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var data = context.Arguments.OfType<IEntityWithScopeId>().FirstOrDefault();
        if (data is null || data.ScopeId == Guid.Empty)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["scopeId"] = ["Scope ID is required"],
            });
        }

        var role = context.HttpContext.User.Adapt<UserRole>();
```

The hidden-argument mechanism for Semantic Kernel is real, but it is split across invocation filters and function registration. That is useful, but not yet fully hidden from the model-facing contract.

```5:14:SampleRag.Application/Filters/Invocation/NonAiArgumentsApplyingFilter.cs
public class NonAiArgumentsApplyingFilter(IDictionary<string, object> nonAiArguments) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        foreach (var pair in nonAiArguments)
        {
            context.Arguments[pair.Key] = pair.Value;
        }

        await next.Invoke(context);
    }
}
```

```52:77:SampleRag.Di/Registries/SemanticKernelRegistry.cs
private static void ConfigurePromptExecutionSettings(this IServiceCollection services)
{
    services.AddSingleton(sp =>
    {
        var kernel = sp.GetRequiredService<Kernel>();
        /*var transformedFunctions = kernel.Plugins
            .SelectMany(plugin => plugin.Select(f =>
                KernelFunctionFactory.CreateFromMethod(
                    method: async (Kernel kernel, KernelFunction currentFunction, KernelArguments currentArgs, CancellationToken cancellationToken) =>
                    {
                        return await currentFunction.InvokeAsync(kernel, currentArgs, cancellationToken);
                    },
                    functionName: f.Name,
                    description: f.Description,
                    parameters: [.. f.Metadata.Parameters.Where(p => p.Name != "scopeId")],
                    returnParameter: f.Metadata.ReturnParameter))).ToArray();*/
```

The scoped vector search path is another good example of the app’s architecture, and also one of the main latency risks.

```69:104:SampleRag.Infrastructure/Repositories/Vector/QdrantDocumentChunkRepository.cs
public async Task<IEnumerable<DocumentChunk>> RetrieveChunksAsync(Guid scopeId, string query, int topK = 5, CancellationToken ct = default)
{
    using var qdrantClient = new QdrantClient(new Uri(settings.Url));

    var queryEmbedding = await embeddingGenerator.GenerateAsync(
    [
        new DocumentChunk
        {
            Text = query,
        },
    ], cancellationToken: ct);

    var filter = new Filter();
    filter.Must.Add(new Condition()
    {
        Field = new FieldCondition
        {
            Key = nameof(DocumentChunk.ScopeIdValue),
            Match = new Match
            {
                Keyword = scopeId.ToString(),
            },
        },
    });
```

## API behavior & endpoint design

### Chats

`api/chats` is auth-protected and scope-aware.

- `POST /api/chats` creates a chat
- `POST /api/chats/filter` returns a filtered batch
- `POST /api/chats/{id}/owners` adds a participant, but only the current owner can do it
- `PATCH /api/chats/{id}/name/generate` is present but not implemented
- `DELETE /api/chats/{id}` removes a chat

The design is sensible, although the route surface is not purely RESTful because `filter` endpoints use POST and ownership management uses a custom sub-route. That choice is reasonable for cursor-style paging and rule-based updates.

### Messages

`api/messages` is the heart of the RAG flow.

- `POST /api/messages` streams the answer as SSE
- `POST /api/messages/filter` lists message history
- `POST /api/messages/complexity` and `POST /api/messages/language` are utility endpoints for analysis/pre-processing

The send path uses both `BodyScopeAccessFilter` and `ChatAccessFilter`, and it is rate limited with the `send-message` policy. That is a good boundary for protecting the model and storage layers, but the current fixed-window policy is strict enough that it can become a UX bottleneck if the chat is used interactively.

Response-language behavior is not enforced at the endpoint boundary. In practice, that means the language will still depend on prompt behavior and model compliance unless the orchestration layer explicitly adds a language instruction or post-processing rule.

### Documents

`api/documents` is admin-oriented and uses both validation and access filters.

- `POST /api/documents` uploads a document
- `PUT /api/documents` updates document metadata and file location
- `POST /api/documents/filter` lists documents
- `POST /api/documents/filter/ids` fetches by ids
- `DELETE /api/documents/{id}` removes a document
- `DELETE /api/documents/chunks` clears chunk state
- `DELETE /api/documents/chunks/embeddings` clears vector embeddings

The upload path stores the file on disk under `assets/documents/{scopeId}` and persists metadata in MongoDB. Chunking and vectorization are separated into jobs, which helps keep upload latency under control.

### Files

`api/files` is used for scoped file download.

- `GET /api/files/assets/documents/{scopeId}/{fileName}` returns a PDF stream

The route is protected by a route-scope access filter, which is a clean fit because this authorization rule is driven by the URL rather than by the request body.

### Knowledge scopes

`api/knowledgescopes` is the administrative layer for scope creation and updates.

- `POST /api/knowledgescopes` creates scopes
- `PUT /api/knowledgescopes/{id}` updates a scope
- `POST /api/knowledgescopes/filter` lists scopes in batches

This group is role-sensitive and is the root of the app’s authorization model. Scope checks are not incidental here; they are a first-class concept.

### Feedback

`api/feedbacks` captures user reactions.

- `POST /api/feedbacks` submits feedback
- `POST /api/feedbacks/filter` lists feedback records

This is a small surface, but it matters because it closes the loop for internal evaluation and relevance tuning.

## Data flow and storage

### MongoDB

MongoDB is the primary transactional store. `MongoPersistanceRegistry` registers the driver, configures the GUID serializer, and maps document/vector entities so vector fields are not serialized into Mongo payloads.

That is a good separation because Mongo stores business records, while Qdrant stores vector payloads and similarity state.

### Qdrant

Qdrant stores document chunks and powers semantic retrieval. The repository supports both generic query search and scope-filtered search. The scoped path is relevant because the API is not just doing semantic retrieval; it is doing semantically relevant retrieval constrained by access control.

The main performance concern is that scoped search currently creates a new `QdrantClient` inside the repository method instead of reusing a shared client or collection path. That adds connection churn and makes response-time tuning harder.

### Local file storage

Uploaded document files are written to local disk under `wwwroot/assets/documents/{scopeId}`. The file repository abstraction is a good fit for this because it keeps the application code from depending directly on the filesystem.

The main trade-off is operational: local disk is simple and fast, but it is not ideal if the app later needs horizontal scaling or shared storage.

### Semantic Kernel orchestration

`SemanticKernelDataGenerator` streams chat completions and can accept extra outer arguments. `MessagesService` uses that path to inject `scopeId` when it exists, which is the right shape for the requirement that the model should not have to know hidden function arguments.

However, the current implementation only partially hides those arguments. The runtime injection exists, but the function metadata still needs to be trimmed or wrapped so the model-facing schema shows only the arguments it should reason about.

That is the most important Semantic Kernel design issue in the repo.

### Scope-based access flow

The app uses three access patterns:

- body-scope access for requests that carry a `ScopeId`
- route-scope access for file download URLs
- chat access for message send and message history

That is a strong design because it keeps security policy aligned with the shape of the request rather than trying to force a single generic rule onto every endpoint.

## Testing strategy

There is no test project in the solution, and no test plan is currently scheduled.

That means the current verification model is runtime/manual rather than automated. For this codebase, the lack of tests is not just a quality issue; it is also a latency and correctness risk because the most important behavior lives at boundaries:

- scope filtering
- message streaming
- repository interactions
- hidden Semantic Kernel arguments
- response-language behavior

Because tests are not planned, the gap should be acknowledged explicitly in the repo docs and in operational practice.

## Test data: sources, builders, storage

There is no formal test data strategy yet because there is no test suite.

Current data shape is mostly runtime data:

- document uploads are base64 JSON payloads
- files are written to local disk
- MongoDB documents are schema-light and entity-driven
- Qdrant chunks are generated during ingestion and stored by vector properties

If testing is introduced later, the best low-friction approach would be fixture builders for request models plus temp-folder/file-system isolation for file storage.

## Documentation & discoverability

The repository has useful docs in `README.md`, `project.md`, and `specs/001-demo-rag-api/`. That is a good foundation, but the docs need to stay in sync with the actual code.

For discoverability:

- `README.md` should stay concise and operational
- `project.md` should stay technical and current
- endpoint contracts in `specs/` should mirror the actual route surface

The current solution is reasonably easy to navigate because folders map to runtime layers and endpoint groups map to route groups.

## Code quality & maintainability

Strengths in code quality:

- nullable reference types are enabled
- code style is enforced through `Directory.Build.props` and `.editorconfig`
- primary constructors reduce boilerplate in services and repositories
- Serilog is wired at startup, which helps with operational debugging
- route groups are explicit and resource-oriented

Current maintainability risks:

- `TreatWarningsAsErrors=false`, so style is enforced more strongly than correctness
- the typo `GenerateAiResponce` still exists in the public service contract
- some support code looks experimental or half-finished, such as the commented function-signature transformation and the prompt render filter stub
- the repository abstraction is useful, but the Qdrant scoped search path creates a performance hotspot
- no automated tests means regressions will be harder to catch

## Strengths

- Clear layer separation between API, application, infrastructure, and domain
- Good use of endpoint filters for validation and access control
- SSE streaming is the right UX choice for chat responses
- Scope-aware authorization is a first-class concern rather than an afterthought
- Ingestion is split from request handling through Quartz jobs
- Semantic Kernel is used as an orchestration layer, not as a monolith hidden inside endpoints
- Logging, Swagger, CORS, and auth are wired centrally in the host

## Weak points & risks

- No automated tests or test project
- Response language is not enforced deterministically in the message pipeline
- The hidden Semantic Kernel argument pattern is only partially complete; the model-facing schema still needs to hide non-AI parameters
- Scoped Qdrant retrieval creates a fresh client per call, which is a latency risk
- The `send-message` rate limit is very strict and may hurt interactive chat UX if not tuned carefully
- Some code is still experimental or incomplete, including the unimplemented chat-name generation path and the prompt render filter stub
- `GenerateAiResponce` is still misspelled in the public API surface
- The app depends on local file storage, which is simple but can become a deployment constraint

## Recommendations

1. Finish the hidden-parameter design for Semantic Kernel. Keep `scopeId` out of the model-facing function schema and inject it only at invocation time, so the LLM sees only the arguments it can actually reason about.
2. Remove the per-call `QdrantClient` construction from scoped retrieval. Reuse a shared client or a shared collection path to cut overhead and make response time more predictable.
3. Add explicit response-language control to the orchestration layer. Detect the request language early and pass an instruction or prompt variable so replies reliably match the user’s language.
4. Revisit the `send-message` rate limit. The current fixed-window policy may protect resources, but it is probably too restrictive for normal chat usage unless the organization really wants that throttling.
5. Add latency instrumentation around history load, retrieval, prompt assembly, tool invocation, and first-token streaming so response-time bottlenecks are visible.
6. Clean up dormant or incomplete pieces such as the typo in `GenerateAiResponce`, the commented prompt-shaping code, and the placeholder render filter.
7. If testing remains out of scope, document that explicitly and define a small manual smoke-check list for scope access, message streaming, and document retrieval so operational validation is at least repeatable.
