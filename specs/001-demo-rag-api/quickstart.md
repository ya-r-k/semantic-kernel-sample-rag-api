# Quickstart: Demo RAG API

**Branch**: `001-demo-rag-api`  
**Purpose**: Run the API and exercise main flows (scope, upload, chat, RAG, feedback) once implementation is complete.

---

## Prerequisites

- .NET 10 SDK  
- MongoDB running (connection string in appsettings)  
- Qdrant running (URL in appsettings)  
- Ollama running with chat and embedding models (e.g. llama3.2, nomic-embed-text)  
- JWT issuer (or use dev/no-auth mode if configured)

---

## Configuration

- **appsettings.json**: Set `DbSettings:ConnectionString`, `DbSettings:DatabaseName`; `VectorDbSettings:Url`; `GenAiProviderSettings:Url`, `TextModel`, `TextEmbeddingModel`.  
- **File storage**: wwwroot path (e.g. `wwwroot/assets/documents`).  
- **Auth**: Configure JwtBearer (Authority, Audience) or disable for local dev.

---

## Run the API

```bash
cd SampleRag.API
dotnet run
```

Swagger: https://localhost:7xxx/swagger (or port from launchSettings).

---

## Main Flows

### 1. Create a scope (admin)

```http
POST /api/knowledgescopes
Authorization: Bearer <admin-token>
Content-Type: application/json

{ "name": "Product Docs" }
```

Expect 201 with `{ "id": "<scopeId>", "name": "Product Docs" }`.  
(Optional) Add user to scope: `POST /api/knowledgescopes/<scopeId>/users` with `{ "userId": "..." }`.

### 2. Upload a PDF (admin, with scope access)

```http
POST /api/documents
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "scopeId": "<scopeId>",
  "name": "manual.pdf",
  "file": { "data": "<base64>", "fileName": "manual.pdf" }
}
```

Expect 201/204. Server chunks PDF, embeds, stores in Qdrant under that scope. Max 20 MB.

### 3. Create a chat (user with scope access)

**Option A – explicit create**

```http
POST /api/chats
Authorization: Bearer <user-token>

{ "title": "My chat", "scopeId": "<scopeId>" }
```

**Option B – send message without chatId (auto-create)**

```http
POST /api/messages
Authorization: Bearer <user-token>

{ "scopeId": "<scopeId>", "text": "What is the refund policy?" }
```

Expect new chat with generated title + first user message + RAG answer + sources.

### 4. Send message in existing chat (RAG)

```http
POST /api/messages
Authorization: Bearer <user-token>

{ "chatId": "<chatId>", "text": "Summarize chapter 2" }
```

Expect stream or JSON with answer and `sources: [{ "documentId": "...", "pageNumber": 2 }, ...]`.

### 5. Add owner to chat

```http
POST /api/chats/<chatId>/owners
Authorization: Bearer <owner-token>

{ "userId": "<another-user-id>" }
```

### 6. Submit feedback on answer

```http
POST /api/messages/<messageId>/feedback
Authorization: Bearer <user-token>

{ "isLike": true }
```

---

## Verification Checklist

- [ ] Scope created and listed.  
- [ ] PDF upload succeeds; document appears with scopeId.  
- [ ] Chat created (explicit or via first message) with scopeId and owner.  
- [ ] Message in chat returns answer with at least one source (document + page) when content exists.  
- [ ] Feedback accepted and idempotent (resubmit updates).  
- [ ] Non-admin upload returns 403.  
- [ ] Request with scope caller cannot use returns 403.
