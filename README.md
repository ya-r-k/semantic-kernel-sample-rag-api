# SampleRag API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/docker-supported-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![RAG](https://img.shields.io/badge/RAG-Semantic%20Kernel-0A66C2)](https://learn.microsoft.com/semantic-kernel/)

Backend API for a scoped Retrieval-Augmented Generation (RAG) chat system built with ASP.NET Core, Semantic Kernel, Ollama, Qdrant, and MongoDB.

## Table of Contents

- [What This Project Does](#what-this-project-does)
- [Why This Project Is Useful](#why-this-project-is-useful)
- [How to Get Started](#how-to-get-started)
- [How to Use the API](#how-to-use-the-api)
- [Where to Get Help](#where-to-get-help)
- [Who Maintains and Contributes](#who-maintains-and-contributes)

## What This Project Does

`SampleRag API` provides backend endpoints for:

- JWT-authenticated chat and message flows
- Document upload and ingestion for RAG
- Vector search with Qdrant and embeddings from Ollama
- Knowledge scopes for access control
- Message feedback capture

Main API route groups:

- `api/auth` (development login helper)
- `api/chats`
- `api/messages`
- `api/documents`
- `api/files`
- `api/knowledgescopes`
- `api/feedbacks`

Solution structure:

- `SampleRag.API` - HTTP API and endpoint composition
- `SampleRag.Application` - application logic and orchestration
- `SampleRag.Domain` - entities, contracts, domain models
- `SampleRag.Infrastructure` - persistence and external integrations
- `SampleRag.Di` - dependency registration/configuration

## Why This Project Is Useful

- Scoped RAG: keep document access and chat context tied to knowledge scopes.
- Practical local-first stack: run models with Ollama and vector DB with Qdrant.
- API-first development: OpenAPI/Swagger enabled in development.
- Production-oriented baseline: JWT auth, rate limiting, structured logging, style analysis.
- Container-friendly: scripts for dependency containers and API container startup.

## How to Get Started

### Prerequisites

- .NET 10 SDK
- Docker Desktop (recommended for local dependencies)

### 1) Clone repository

```bash
git clone https://github.com/ya-r-k/semantic-kernel-sample-rag-api
cd semantic-kernel-sample-rag-api
```

### 2) (Optional) Restore and build project
```bash
dotnet restore
dotnet build
```

### 3) (Optional) Verify configuration

Review `SampleRag.API/appsettings.json` and adjust if needed:

- `DbSettings:ConnectionString`
- `VectorDbSettings:Url`
- `GenAiProviderSettings:Url`
- `GenAiProviderSettings:TextModel`
- `GenAiProviderSettings:TextEmbeddingModel`


Default development URL: `http://localhost:5234`
Swagger UI: `http://localhost:5234/swagger`

### 4) (Optional) Start required dependencies

Use `Scripts/docker.run-deps.bat` to launch:

- MongoDB (`localhost:27017`)
- Qdrant (`localhost:6334`)
- Ollama (`localhost:11434`)

The script also pulls default models used by this project:

- `qwen3:4b`
- `qwen3.5:0.8b`
- `mxbai-embed-large:335m`


### 5) Run the API in Docker

Use `Scripts/docker.run-api.bat` to run Sample.RagApi in Docker. Starting `Scripts/docker.run-deps.bat` separately is unnecessary because it is already launched by `Scripts/docker.run-api.bat`.

It builds and starts the API container on port `5234` and connects it to the shared `samplerag-net` network.

## How to Use the API

In `Development`, you can generate a short-lived token via:

```http
POST /api/auth/login
Content-Type: application/json

{
  "userId": "admin-1",
  "email": "admin@example.com",
  "role": "Admin",
  "password": "dev-only"
}
```

Then call protected endpoints with:

```http
Authorization: Bearer <jwt-token>
```

Example flow:

1. Create scope: `POST /api/knowledgescopes`
2. Upload document: `POST /api/documents`
3. Create chat: `POST /api/chats` (or send first message directly)
4. Send message (SSE): `POST /api/messages`
5. Submit feedback: `POST /api/feedbacks`

For request/response examples, see:

- `specs/001-demo-rag-api/quickstart.md`
- `specs/001-demo-rag-api/contracts/api-endpoints.md`

## Where to Get Help

- Start with Swagger UI in local development: `http://localhost:5234/swagger`
- Implementation specs and endpoint contracts:
  - `specs/001-demo-rag-api/`
  - `specs/001-demo-rag-api/contracts/api-endpoints.md`
- If you hit setup issues, open an issue in this repository with:
  - runtime logs
  - config deltas (without secrets)
  - exact request sample

## Who Maintains and Contributes

This repository is maintained by project contributors.

### Contributing

- Fork the repository and create a feature branch.
- Make focused changes with tests/verification where possible.
- Run formatting and build checks before opening a PR:

```bash
dotnet format SampleRag.API.slnx
dotnet build
```

- Open a pull request describing the problem, approach, and verification steps.

### Notes

- No dedicated `CONTRIBUTING.md` is currently present; use the workflow above.
- No `LICENSE` file is currently present; add one before public distribution.
