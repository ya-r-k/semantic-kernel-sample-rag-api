# Research: Demo RAG API (Phase 0)

**Branch**: `001-demo-rag-api` | **Date**: 2025-02-14  
**Purpose**: Resolve NEEDS CLARIFICATION and technology choices for PDF chunking, scope enforcement, and auth.

---

## 1. PDF Document Chunking

**Decision**: Use **page-based chunking** with optional sub-splitting for very long pages. Store one embedding record per logical chunk with metadata: `DocumentId`, `ScopeId`, `PageNumber`, and `Text`. Use **PdfPig** for PDF text extraction (pure C#, no cloud; respects constitution).

**Rationale**:

- Spec requires source references as **document + page**. Page-based chunks give a direct mapping: each retrieved chunk has a single page number.
- Sub-splitting long pages (e.g. by token or character limit with overlap) keeps embedding size within model limits and improves retrieval granularity while still attributing to a page.
- PdfPig is well-maintained, Apache 2.0, and provides per-page text and ordering; no external services (Ollama/Qdrant only per constitution).

**Alternatives considered**:

- **Fixed-size sliding window only**: Would require storing start/end page or paragraph for each chunk; more complex metadata and less clear “page” in API response. Rejected for MVP in favour of simpler page-first model.
- **Semantic Kernel document loaders**: If SK provides a PDF loader that yields pages/chunks, we can align with it; otherwise PdfPig is the primary extractor and we feed text into SK embedding + Qdrant.
- **Cloud PDF APIs**: Excluded by constitution (no external cloud for this; local only).

**Implementation outline**:

1. On document upload (after saving file and DocumentData with ScopeId): open PDF with PdfPig, iterate pages, extract text per page (e.g. `ContentOrderTextExtractor.GetText(page)`).
2. For each page: if text length under threshold (e.g. 500–800 tokens or ~2–4K chars), treat as one chunk; else split into overlapping segments and tag each with same `PageNumber` and `DocumentId`.
3. Generate embedding per chunk via Semantic Kernel (Ollama embedding model); store in Qdrant with payload: `DocumentId`, `ScopeId`, `PageNumber`, `ChunkIndex` (if page split). Use Qdrant collection or filter by `ScopeId` so retrieval is scope-isolated.
4. Constitution: vector storage mapping (payload schema) must be in configuration/Infrastructure, not attributes on Domain models (see plan.md Constitution Check).

**Chunk size / overlap**: Default page-as-chunk; for long pages use ~512 tokens target with 64-token overlap (or equivalent character-based values). Exact numbers can be configurable in appsettings.

---

## 2. Scope API and Enforcement

**Decision**: Treat **KnowledgeGroupData** as the scope entity. Add a **scope–user access** store (e.g. collection or table `ScopeUser` with ScopeId + UserId) so that “user U can use scope S” is explicit. Scope creation/management (create, list, assign users) is admin-only; upload, chat creation, and send-message require the caller to have access to the given scope.

**Rationale**:

- Spec FR-001b: scopes created/managed via API; system enforces which users can use which scope. A separate access list allows flexible assignment without encoding in JWT (JWT can carry identity; scope membership can be looked up in DB).
- Alternative: encode “allowed scope IDs” in token claims. Possible but pushes more into identity provider; the access-list approach keeps the API self-contained and testable.

**Alternatives considered**:

- **Only JWT claims**: Requires IdP to know scope membership; acceptable if IdP is the source of truth. For demo, we keep scope–user in our store so the API can enforce without depending on claim shape.
- **No per-user scope list (all users see all scopes)**: Would violate spec (explicit enforcement). Rejected.

**Implementation outline**:

- Add `ScopeUser` or equivalent (ScopeId, UserId); endpoints: create scope (admin), list scopes (admin or filtered by access), add/remove user to/from scope (admin).
- On upload: require scope Id in request; check caller has access to that scope; store document with that scope.
- On chat creation (explicit or via “send message without chatId”): require scope Id; check access; create chat with ScopeId.
- On RAG: chat already has ScopeId; retrieval uses only vectors tagged with that ScopeId (Qdrant filter).

---

## 3. Authentication and Administrator Role

**Decision**: Validate **JWT** (or bearer token) in middleware; read **identity** (user id) and **role** (e.g. “Administrator”) from claims. No new identity provider in scope: assume tokens are issued elsewhere; API only validates signature and reads claims. For demo, a simple claim structure (e.g. `sub`, `role`) is sufficient.

**Rationale**:

- Spec and clarifications: “Identity provider / token; API validates token and reads role/claim.” So we do not implement login/signup; we only validate and authorize.
- Use standard ASP.NET Core JWT bearer authentication; add policy “RequireAdministrator” for upload and scope management; for “user can use scope S”, combine identity with scope–user store (see §2).

**Alternatives considered**:

- **API key or header (X-Role: Admin)**: Simpler but less realistic; spec chose token/IdP. Rejected.
- **Full IdP in repo**: Out of scope; spec says “existing API/auth context.” We only consume tokens.

**Implementation outline**:

- Add `Microsoft.AspNetCore.Authentication.JwtBearer`; configure from appsettings (authority, audience, issuer). Optional: allow running without auth for local dev with a “dev user” claim set by middleware.
- Endpoints: document upload, scope create/update, and “add user to scope” require admin role; send message, create chat, feedback require authenticated user; scope access checked via ScopeUser store.

---

## 4. Summary Table

| Topic | Decision | Key dependency |
|-------|----------|-----------------|
| PDF chunking | Page-based (+ optional split for long pages); PdfPig for extraction | PdfPig, SK embeddings, Qdrant |
| Chunk storage | Qdrant with payload DocumentId, ScopeId, PageNumber; mapping in config/Infra | Constitution: no attributes on domain models |
| Scope enforcement | Scope–user access store; check on upload, chat create, RAG | MongoDB or existing store |
| Auth | JWT validation; role and identity from claims | JwtBearer, no IdP in repo |
