<!--
  Sync Impact Report
  Version change: 1.0.0 → 1.1.0
  Modified principles: IV (Qdrant expanded with vector config + collection ensure), VII (Domain-only models, class-only, IVectorEntity)
  Added sections: VIII Code Style (Control Flow), IX Primary Constructors, X Endpoint Validation
  Removed sections: none
  Templates: plan-template.md ✅ (Constitution Check updated with new principles); spec-template.md ✅ (no changes); tasks-template.md ✅ (no changes)
  Follow-up TODOs: none
-->

# Sample RAG API Constitution

## Core Principles

### I. C# and Semantic Kernel with Ollama Only

- Implementation language is **C#**.
- Access to language and embedding models MUST use **Semantic Kernel**.
- The only permitted runtime provider for LLM and embeddings is **Ollama**; no direct use of OpenAI, Azure OpenAI, or other cloud providers for model execution.

*Rationale*: Ensures consistent local/on-prem model usage and a single integration path via Semantic Kernel and Ollama.

### II. ASP.NET Minimal API

- The application is a **web application** built with **ASP.NET**.
- All HTTP endpoints MUST be exposed via **Minimal API** (no MVC controllers for API surface).

*Rationale*: Keeps the API surface explicit, lightweight, and aligned with modern ASP.NET patterns.

### III. Mapster-Only Mapping

- When object-to-object mapping is required, the project MUST use **Mapster** only (no AutoMapper or manual mapping libraries).
- Mapster MUST be configured **globally in DI** (single registration point).

*Rationale*: Single mapping strategy avoids inconsistency and simplifies maintenance.

### IV. Qdrant as Vector Store

- The **vector database** for embeddings MUST be **Qdrant**; no other vector store may be used for semantic/embedding storage.
- Vector store **configuration** MUST use the prescribed collection definition pattern: `VectorStoreCollectionDefinition` with `EmbeddingGenerator`, and `Properties` containing at least `VectorStoreKeyProperty` (e.g. Id, `typeof(Guid)`), `VectorStoreDataProperty` entries (e.g. PageNumber, ChunkIndex), and `VectorStoreVectorProperty` for the vector (e.g. "Vector", `typeof(ReadOnlyMemory<float>)`, with dimensions, `DistanceFunction.CosineSimilarity`, `IndexKind.Hnsw`). Exact property names and dimensions MUST match the domain model used for the collection.
- Collections MUST be verified or created at startup in the DI/Infrastructure layer (e.g. an `EnsureCollectionsExistsAsync`-style flow that lists existing collections and creates or recreates any missing or misconfigured ones using the same vector size, distance, and quantization config as defined for the project). Qdrant client and collection registration (e.g. `AddQdrantVectorStore`, `AddQdrantCollection`) MUST be centralized in the same DI configuration that runs this ensure step.

*Rationale*: One vector store with a consistent schema and startup guarantee keeps dependencies and operations predictable and avoids runtime collection errors.

### V. File Storage and Access

- Uploaded files MUST be stored **locally under wwwroot** (or a configured equivalent path).
- The **API is the only gateway** between file storage and the user: files MUST NOT be directly accessible (e.g. no public static file serving of uploads, or serving must be disabled/restricted so that access is only via API endpoints).
- Maximum size for a single file upload MUST NOT exceed **20 MB**.

*Rationale*: Centralized access through the API ensures access control, auditing, and size limits.

### VI. Clean Architecture and Repository

- The solution MUST follow **Clean Architecture** with these layers: **Domain**, **Application**, **API**, **Infrastructure**.
- Data access MUST use the **Repository** pattern.
- All repository and service **interfaces** MUST live in the **Domain** project; implementations live in Application or Infrastructure as appropriate.

*Rationale*: Clear dependency direction and testability; domain remains free of infrastructure details.

### VII. Attribute-Free Domain Models and DI Configuration

- **All models** (entities, DTOs, and any type used as a data contract or persistence model) MUST be defined in the **Domain** project; no models in Application, API, or Infrastructure except references to Domain types.
- Domain and application models MUST NOT use persistence or ORM attributes (e.g. no Mongo, Entity Framework, or similar attributes on entities/DTOs). Models MUST be **attribute-free**.
- All persistence mapping and schema configuration (Mongo, EF, vector store, etc.) MUST be done in **configuration classes** registered via **DI**, not on the model types themselves.
- Mapster type configuration is also applied globally via DI, not via attributes on models.
- Models MUST be declared as **class** only; **record** types MUST NOT be used for models.
- Any model that has a **Vector** (embedding) field MUST implement **IVectorEntity** (e.g. `IVectorEntity<Guid, float>` and `IEntity<Guid>` as applicable). When a type has two or more vector fields, IVectorEntity usage is not further specified by this constitution.

*Rationale*: Keeps models technology-agnostic, confined to the Domain, and confines persistence concerns to configuration and infrastructure.

### VIII. Code Style (Control Flow)

- All **if** statements MUST use curly braces for the body. Single-line or block bodies are both allowed only when wrapped in braces, e.g. `if (condition) { logic }`. No bare single-statement bodies without braces.

*Rationale*: Avoids bugs from misleading indentation and keeps style consistent and reviewable.

### IX. Primary Constructors in Application and Infrastructure

- In the **Application** and **Infrastructure** layers, all classes MUST use **primary constructors** only; no instance fields except when the class is a **base class** that explicitly requires a **protected** field for derived types.

*Rationale*: Reduces boilerplate and keeps dependency injection and state explicit at the type level.

### X. Endpoint Validation via Filters

- All validations that apply to Minimal API endpoints MUST be implemented through **Endpoint filters**, not inside the endpoint handler logic. Endpoint logic MUST assume validated input where validation is required.

*Rationale*: Keeps handlers focused on orchestration and ensures validation is reusable, testable, and consistent across endpoints.

## Additional Constraints

- **Stack summary**: C#, ASP.NET, Semantic Kernel, Ollama, Qdrant, Mapster; file storage under wwwroot; 20 MB upload limit.
- New features and endpoints MUST comply with the principles above (Minimal API, Repository pattern, interfaces in Domain, models in Domain only, class-only attribute-free models, IVectorEntity for vector models, primary constructors in Application/Infrastructure, if-with-braces, vector store config and collection-ensure in DI, endpoint validation via filters).

## Development and Structure

- Use the four-layer structure: Domain (interfaces + all models), Application (services + use cases), API (Minimal API endpoints + endpoint filters for validation), Infrastructure (repositories, external integrations, persistence and vector store configuration, collection-ensure on startup).
- When adding new entities or storage, introduce configuration in Infrastructure/DI rather than attributing models. Ensure vector collections exist at startup via the same DI configuration that registers the Qdrant client and collections.

## Governance

- This constitution overrides ad-hoc technical decisions; any exception MUST be documented and justified (e.g. in a Complexity Tracking or design doc).
- Amendments require updating this file, bumping the version (semantic: MAJOR = incompatible principle changes, MINOR = new/expanded principles, PATCH = clarifications/typos), and updating the Sync Impact Report comment at the top.
- All PRs and reviews SHOULD verify that changes comply with the stated principles (language, Minimal API, Mapster, Qdrant and vector config/collection-ensure, file access, Clean Architecture, Repository pattern, Domain-only attribute-free class models, IVectorEntity for vector models, primary constructors in Application/Infrastructure, if-with-braces, endpoint validation via filters).

**Version**: 1.1.0 | **Ratified**: 2025-02-14 | **Last Amended**: 2025-02-22
