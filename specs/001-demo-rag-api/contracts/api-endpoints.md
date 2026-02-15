# API Contracts: Demo RAG API

**Branch**: `001-demo-rag-api` | **Date**: 2025-02-14  
**Base path**: `/api`  
**Auth**: Bearer JWT; role and identity from claims. Scope access enforced via ScopeUser store.

---

## Scopes (Knowledge Groups)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/groups | Create scope (Name). Returns 201 + scope. | Admin |
| GET | /api/groups | List scopes (optional: only those caller can use). Returns 200 + list. | User |
| (Optional) | POST /api/groups/{id}/users | Add user to scope. | Admin |
| (Optional) | DELETE /api/groups/{id}/users/{userId} | Remove user from scope. | Admin |

**Request (POST /api/groups)**: `{ "name": "string" }`  
**Response (201)**: `{ "id": "guid", "name": "string" }`

---

## Documents (Upload)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/documents | Upload PDF; body includes scopeId, file (base64 or multipart). Returns 201/204 + document metadata. | Admin; caller must have scope access |

**Request**: ScopeId + file (e.g. base64 Data + FileName, or multipart). Max 20 MB.  
**Response**: Document with Id, Name, LocalLink, ScopeId.  
**Errors**: 403 if not admin or no scope access; 400 if not PDF or size exceeded.

---

## Chats

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/chats | Create chat (Title, ScopeId, initial OwnerIds or single owner from token). Returns 201 + chat. | User; scope access required |
| GET | /api/chats | List chats (e.g. where caller is in OwnerIds). Query: batchSize, lastUsedIndex. Returns 200 + list. | User |
| GET | /api/chats/{id} | Get chat by id. Returns 200 or 404. | Owner |
| DELETE | /api/chats/{id} | Delete chat. Returns 204. | Owner |
| POST | /api/chats/{id}/owners | Add owner (body: userId or list). Returns 200/204. Only existing owners. | Owner |

**Request (POST /api/chats)**: `{ "title": "string", "scopeId": "guid", "ownerIds": ["string"]? }` — if ownerIds omitted, use caller as sole owner.  
**Response (201)**: `{ "id": "guid", "title": "string", "scopeId": "guid", "ownerIds": ["string"] }`

---

## Messages (including “send without chat” and RAG)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/messages | Send message. Body: either (chatId + text) or (scopeId + text) for “create chat and send”. Returns stream (SSE) or 201 with message + optional new chat. | User; must be owner if chatId provided; must have scope access if scopeId for new chat |

**Request (existing chat)**: `{ "chatId": "guid", "text": "string" }`  
**Request (new chat)**: `{ "scopeId": "guid", "text": "string" }` — server creates chat with generated title, adds message, runs RAG, returns answer with sources.  
**Response (stream)**: MessagePart stream (text chunks + final message with CreatedAt); or JSON with message + answer + sources.  
**Sources in response**: `sources: [{ "documentId": "guid", "pageNumber": 1 }, ...]`

---

## Feedback

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| POST | /api/messages/{messageId}/feedback | Submit like or dislike. Body: { "isLike": true \| false }. Idempotent per (messageId, user). Returns 200/204. | User |
| GET | /api/messages/{messageId}/feedback | Get feedback for message (optional). Returns 200 + list or own. | User |

**Request (POST)**: `{ "isLike": true }` or `{ "isLike": false }`  
**Response**: 204 No Content or 200 with updated feedback.

---

## Files (download)

| Method | Path | Description | Auth |
|--------|------|-------------|------|
| GET | /api/files/assets/documents/{fileName} | Download file by name. Only via API (no static file serving of uploads). Returns 200/206/404. | Optional: require auth (commented in project; spec says API is gateway) |

---

## Error Responses

- **400** Bad Request: validation (e.g. missing scopeId, invalid file type/size).  
- **401** Unauthorized: missing or invalid token.  
- **403** Forbidden: not admin where required; or no scope access.  
- **404** Not Found: chat, message, document, or scope not found.  
- **429** Too Many Requests: rate limit (when applied).

---

## Consistency with Spec

- FR-001: Admin-only upload → POST /api/documents requires admin.  
- FR-001b: Scope API → POST/GET /api/groups; enforcement via ScopeUser on upload, chat create, messages.  
- FR-002/003: Upload with scopeId; 20 MB enforced.  
- FR-004: RAG in POST /api/messages; response includes sources (documentId + pageNumber).  
- FR-005/006: Chats with scopeId and ownerIds; “send without chat” via scopeId + text.  
- FR-007: POST /api/chats/{id}/owners.  
- FR-008: POST /api/messages/{messageId}/feedback.  
- FR-009: All behaviour via API; file access only through GET files endpoint.
