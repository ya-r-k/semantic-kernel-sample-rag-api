# SampleRag API

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-supported-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![Semantic Kernel](https://img.shields.io/badge/RAG-Semantic%20Kernel-0A66C2)](https://learn.microsoft.com/semantic-kernel/)

SampleRag API is an internal Retrieval-Augmented Generation backend for scoped chat, document upload, and feedback workflows built with ASP.NET Core, Semantic Kernel, Ollama, Qdrant, and MongoDB.

## Table of Contents

- [What This Project Does](#what-it-does)
- [API surface](#api-surface)
- [Why This Project Is Useful](#why-this-project-is-useful)
- [Requirements](#requirements)
- [Setup](#setup)
- [Quick start](#quick-start)
- [Documentation](#documentation)
- [Building & testing](#building-&-testing)
- [Contributing](#contributing)
- [License](#license)

## What it does

- Streams chat responses over SSE for lower perceived latency.
- Keeps chats and documents tied to knowledge scopes and access rules.
- Stores uploaded files on disk and metadata in MongoDB.
- Uses Qdrant for semantic retrieval over document chunks.
- Captures feedback so answer quality can be reviewed later.

## API surface

- `api/auth` - development login helper
- `api/chats` - create, list, update owners, delete
- `api/messages` - send streamed messages and list history
- `api/documents` - upload, list, and clean up documents/chunks
- `api/files` - scoped file download
- `api/knowledgescopes` - scope management
- `api/feedbacks` - feedback capture and listing

## Why This Project Is Useful

- Scoped RAG: keep document access and chat context tied to knowledge scopes.
- Practical local-first stack: run models with Ollama and vector DB with Qdrant.
- API-first development: OpenAPI/Swagger enabled in development.
- Production-oriented baseline: JWT auth, rate limiting, structured logging, style analysis.
- Container-friendly: scripts for dependency containers and API container startup.

## Requirements

- .NET 10 SDK
- Docker Desktop, if you want to run the dependency containers
- MongoDB, Qdrant, and Ollama if you are not using the provided scripts

## Setup

1. Clone the repository.
2. Restore and build the solution.

```bash
dotnet restore
dotnet build SampleRag.API.slnx
```

3. Start the local dependencies.

```bat
Scripts\docker.run-deps.bat
```

4. Run the API.

```bat
Scripts\docker.run-api.bat
```

If you want to run the API locally instead of in Docker, review `SampleRag.API/appsettings.json` and make sure the MongoDB, Qdrant, Ollama, and JWT settings match your environment.

Default development URL: `http://localhost:5234`
Swagger UI: `http://localhost:5234/swagger`

## Quick start

In development, get a token first:

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

Then try the normal flow:

1. Create a knowledge scope with `POST /api/knowledgescopes`.
2. Upload a document with `POST /api/documents`.
3. Create or open a chat with `POST /api/chats`.
4. Send a message with `POST /api/messages` and read the streamed response.
5. Submit feedback with `POST /api/feedbacks`.

See `specs/001-demo-rag-api/contracts/api-endpoints.md` for request shapes and endpoint details.

## Documentation

- Deep technical overview: `project.md`
- Endpoint contracts: `specs/001-demo-rag-api/contracts/api-endpoints.md`
- Quickstart spec: `specs/001-demo-rag-api/quickstart.md`

## Building & testing

```bash
dotnet build SampleRag.API.slnx
dotnet test SampleRag.API.slnx
```

There is currently no test project in the solution, so `dotnet build` is the main verification step.

## Contributing

There is no `CONTRIBUTING.md` file yet. Keep changes focused, run the build locally, and include any relevant verification notes in pull requests.

## License

No `LICENSE` file is currently present in the repository.
