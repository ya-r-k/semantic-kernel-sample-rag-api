# API Contracts: Demo RAG API

**Branch**: `001-demo-rag-api` | **Date**: 2025-03-14  
**Base path**: `/api`  
**Auth**: Bearer JWT; role and identity from claims. Scope access enforced via ScopeUser store.  
**Note**: List operations use **POST …/filter** with a filter model (cursor-style with `lastId`/`batchSize`); there are no GET list endpoints.

---

## Knowledge Scopes (Scopes / Groups)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/knowledgescopes | Create scope(s). Body: single `CreateScopeRequest` or array. Returns 201 + created scope(s). | Admin |
| POST | /api/knowledgescopes/filter | List scopes. Body: `GetBatchByModel` (optional `lastId`, `batchSize`). Caller-scoped when user id present. Returns 200 + list. | User |
| POST | /api/knowledgescopes/{id}/users | Add user(s) to scope. Body: `AddScopeUserRequest`. Returns 204. | Admin |
| DELETE | /api/knowledgescopes/{id}/users/{userId} | Remove user from scope. Returns 204. | Admin |

**CreateScopeRequest**: `{ "name": "string", "usersIds": ["string"] }`  
**AddScopeUserRequest**: `{ "usersId": ["string"]? }`  
**GetBatchByModel**: `{ "lastId": "guid?", "batchSize": number }`  
**Response (201 create)**: Created scope(s) with Id, Name, etc.

---

## Documents (Upload & Management)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/documents | Upload document. Body: `UploadDocumentRequestModel` (Name, ScopeId, File with base64 Content + FileName). Returns 201 + document metadata. | Admin/scope access (currently commented out in code) |
| POST | /api/documents/filter | List/filter documents. Body: `GetDocumentsByModel`. Returns 200 + list. | User (auth commented out) |
| POST | /api/documents/filter/ids | Get documents by ids. Body: `Guid[]`. Returns 200 + list. | User (auth commented out) |
| DELETE | /api/documents/{id} | Delete document by id. Returns 204. | Admin (auth commented out) |
| DELETE | /api/documents/chunks | Remove all document chunks. Returns 204. | Admin (auth commented out) |
| DELETE | /api/documents/chunks/embeddings | Remove all chunk embeddings. Returns 204. | Admin (auth commented out) |

**UploadDocumentRequestModel**: `{ "name": "string", "scopeId": "guid", "file": { "content": "base64string", "fileName": "string" } }`  
**Response (201)**: Document with Id, Name, LocalLink, ScopeId.  
**Validation**: PDF only; file size enforced in code (1.5 MB limit; error message says "20 MB" — inconsistent).  
**Errors**: 400 validation (missing/invalid file, type, size); 403 if auth enabled and not admin or no scope access.

---

## Chats

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/chats | Create chat. Body: `CreateChatRequest`. Returns 201 + chat. Scope access enforced via filter. | User; scope access required |
| POST | /api/chats/filter | List chats. Body: `GetChatsByModel` (batchSize, lastId, optional scopeId). Returns 200 + list. | User |
| POST | /api/chats/{id}/owners | Add owner. Body: `AddChatOwnerRequest` (UserId). Only existing owners. Returns 204 (or 200). | Owner |
| PATCH | /api/chats/{id}/name/generate | Generate chat name (not implemented; throws NotImplementedException). | User |
| DELETE | /api/chats/{id} | Delete chat. Returns 204. | User |

**CreateChatRequest**: `{ "name": "string", "scopeId": "guid", "ownerIds": ["string"]? }` — if ownerIds omitted, caller is used as sole owner.  
**GetChatsByModel**: extends GetBatchByModel; `{ "lastId": "guid?", "batchSize": number, "scopeId": "guid?" }`  
**AddChatOwnerRequest**: `{ "userId": "string" }`  
**Response (201)**: Chat with Id, Name, ScopeId, OwnerIds.  
**Note**: There is no GET /api/chats/{id} endpoint; get-by-id is used internally (e.g. in owners endpoint).

---

## Messages (RAG & Streaming)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/messages | Send message. Body: `SendMessageRequest`. Returns stream (IAsyncEnumerable of `MessagePartResponse`) or 201 with message. When ChatId is empty, service may create a new chat and stream it first. | User; must be owner if chatId provided |
| POST | /api/messages/filter | List/filter messages. Body: `GetMessagesByModel`. Returns 200 + list. | User (auth commented out) |

**SendMessageRequest**: `{ "chatId": "guid", "text": "string" }` — use `chatId: "00000000-0000-0000-0000-000000000000"` for “create chat and send” (new chat created by service; ScopeId not in request model in current implementation).  
**Response (stream)**: MessagePart stream (e.g. chat first if new, then text chunks + final message with sources).  
**Sources**: Response may include source references (documentId, pageNumber) where implemented.

---

## Feedback

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/feedbacks | Submit like/dislike for a message. Body: `FeedbackRequest`. Idempotent per (messageId, user). Returns 204. | User |
| POST | /api/feedbacks/filter | Get feedback by filter. Body: `GetFeedbackByModel`. Returns 200 + list. | User |

**FeedbackRequest**: `{ "messageId": "guid", "isLike": true | false }`  
**Response**: 204 No Content.

---

## Files (Download)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | /api/files/assets/documents/{fileName} | Download file by name. Returns 200/206 (range) or 404. Served as application/pdf. | Optional (auth commented out; spec says API is gateway) |

---

## Error Responses

- **400** Bad Request: validation (e.g. missing scopeId, invalid file type/size, missing UserId on add-owner).  
- **401** Unauthorized: missing or invalid token.  
- **403** Forbidden: not admin where required; or no scope access.  
- **404** Not Found: chat, message, document, scope, or file not found.  
- **429** Too Many Requests: rate limit (when applied; currently not applied in code).  
- **500** Internal Server Error: e.g. create failed (no entity returned).

---

## Consistency with Spec

- **FR-001**: Admin-only upload → POST /api/documents (admin/scope auth currently commented out).  
- **FR-001b**: Scope API → POST/GET replaced by POST /api/knowledgescopes and POST /api/knowledgescopes/filter; enforcement via ScopeUser on upload, chat create, messages.  
- **FR-002/003**: Upload with scopeId; size enforced (1.5 MB in code; message says 20 MB).  
- **FR-004**: RAG in POST /api/messages; response can include source references.  
- **FR-005/006**: Chats with scopeId and ownerIds; “send without chat” via empty ChatId + text (ScopeId not in request in current impl).  
- **FR-007**: POST /api/chats/{id}/owners.  
- **FR-008**: Feedback via POST /api/feedbacks (body includes messageId), not POST /api/messages/{messageId}/feedback.  
- **FR-009**: File access only through GET /api/files/assets/documents/{fileName}.
