<!--
  Sync Impact Report
  Version change: (placeholder) → 1.0.0
  Modified principles: N/A (initial ratification from user-supplied principles)
  Added sections: Core Principles (7), Additional Constraints, Development & Structure, Governance
  Removed sections: none
  Templates: plan-template.md ✅ (Constitution Check references constitution file); spec-template.md ✅ (no changes); tasks-template.md ✅ (path conventions remain generic)
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

*Rationale*: One vector store keeps dependencies and operations predictable.

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

### VII. Attribute-Free Models and DI Configuration

- **Domain and application models** MUST NOT use persistence or ORM attributes (e.g. no Mongo, Entity Framework, or similar attributes on entities/DTOs).
- All persistence mapping and schema configuration (Mongo, EF, etc.) MUST be done in **configuration classes** registered via **DI**, not on the model types themselves.
- Mapster type configuration is also applied globally via DI, not via attributes on models.

*Rationale*: Keeps models technology-agnostic and confines persistence concerns to configuration and infrastructure.

## Additional Constraints

- **Stack summary**: C#, ASP.NET, Semantic Kernel, Ollama, Qdrant, Mapster; file storage under wwwroot; 20 MB upload limit.
- New features and endpoints MUST comply with the principles above (Minimal API, Repository pattern, interfaces in Domain, no attributes on models).

## Development and Structure

- Use the four-layer structure: Domain (interfaces + models), Application (services + use cases), API (Minimal API endpoints), Infrastructure (repositories, external integrations, persistence configuration).
- When adding new entities or storage, introduce configuration in Infrastructure/DI rather than attributing models.

## Governance

- This constitution overrides ad-hoc technical decisions; any exception MUST be documented and justified (e.g. in a Complexity Tracking or design doc).
- Amendments require updating this file, bumping the version (semantic: MAJOR = incompatible principle changes, MINOR = new/expanded principles, PATCH = clarifications/typos), and updating the Sync Impact Report comment at the top.
- All PRs and reviews SHOULD verify that changes comply with the stated principles (language, Minimal API, Mapster, Qdrant, file access, Clean Architecture, Repository pattern, attribute-free models, DI configuration).

**Version**: 1.0.0 | **Ratified**: 2025-02-14 | **Last Amended**: 2025-02-14
