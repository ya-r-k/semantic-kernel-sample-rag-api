---
description: 'Task list for Demo RAG API feature implementation'
---

# Tasks: Demo RAG API

**Input**: Design documents from `specs/001-demo-rag-api/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not requested in the feature specification; no test tasks included.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **API**: `SampleRag.API/Endpoints/`, `SampleRag.API/` (middleware, Program.cs)
- **Application**: `SampleRag.Application/Services/`
- **Domain**: `SampleRag.Domain/Models/`, `SampleRag.Domain/Interfaces/`
- **Infrastructure**: `SampleRag.Infrastructure/Repositories/`, configuration
- **DI**: `SampleRag.Di/`

**Terminology**: In this feature, _scope_ and _group_ refer to the same concept (container for documents and chats). The API exposes it as `/api/knowledgescopes`; the data model uses Scope (alias KnowledgeGroup).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization, dependencies, and constitution compliance

- [x] T001 Ensure project structure per plan (SampleRag.API, Application, Domain, Infrastructure, Di) and add any missing folders
- [x] T002 Add NuGet packages: PdfPig, Microsoft.AspNetCore.Authentication.JwtBearer (as needed) to SampleRag.API and Application
- [x] T003 [P] Configure Mapster global DI registration in SampleRag.Di
- [x] T004 [P] Move vector-store mapping off domain models (constitution VII): remove [VectorStoreKey], [VectorStoreData], [VectorStoreVector] from SampleRag.Domain/Models/DocumentPageData.cs (or retire DocumentPageData from Domain); add DocumentChunk DTO and Qdrant payload schema in SampleRag.Infrastructure for vector persistence

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Auth, scope access, and core fixes that MUST be complete before any user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Implement JWT bearer authentication and RequireAdministrator policy in SampleRag.API (Program.cs or middleware)
- [x] T006 Add ScopeUser model in SampleRag.Domain/Models; add IScopeUserRepository in SampleRag.Domain/Interfaces and implement in SampleRag.Infrastructure/Repositories (MongoDB collection ScopeUsers)
- [x] T007 Implement scope access enforcement (check user can use scope) in SampleRag.Application/Services for use on upload, chat create, and messages
- [x] T008 Fix GET list endpoints to return Results.Ok: ChatsEndpoints.cs and KnowledgeGroupsEndpoints.cs (per plan bug fixes)
- [x] T009 Fix MessageData.ChatId type to Guid and align with ChatData.Id in SampleRag.Domain/Models/MessageData.cs
- [x] T010 Configure Qdrant client and collection schema (payload: DocumentId, ScopeId, PageNumber, ChunkIndex, Text) in SampleRag.Infrastructure
- [x] T011 [P] Implement Scope API: POST /api/knowledgescopes, GET /api/knowledgescopes, POST /api/knowledgescopes/{id}/users, DELETE /api/knowledgescopes/{id}/users/{userId} in SampleRag.API/Endpoints/KnowledgeGroupsEndpoints.cs (or dedicated ScopesEndpoints) with admin/scope-access auth

**Checkpoint**: Foundation ready — user story implementation can begin

---

## Phase 3: User Story 1 - Admin uploads PDFs for RAG (Priority: P1) 🎯 MVP

**Goal**: Admin uploads PDFs into a scope; files are stored, chunked, embedded, and stored in Qdrant. Non-admin uploads rejected. Max 20 MB and PDF-only enforced.

**Independent Test**: Admin uploads a PDF via API and receives success; document is stored with scopeId. Non-admin upload is rejected with 403. File over 20 MB or non-PDF is rejected with 400.

- [X] T012 [P] [US1] Add ScopeId to DocumentData and ensure document repository supports scope in SampleRag.Domain/Models/DocumentData.cs and SampleRag.Infrastructure/Repositories
- [X] T013 [US1] Implement PDF chunking (PdfPig, page-based, optional sub-split for long pages) in SampleRag.Application/Services (DocumentService or dedicated ChunkingService)
- [X] T014 [US1] Implement embedding generation and Qdrant upsert per chunk (Semantic Kernel + Ollama embedding, Qdrant store by scope) in SampleRag.Application/Services
- [X] T015 [US1] Implement document upload pipeline: validate admin + scope access, 20 MB max, PDF content-type; save file under wwwroot and DocumentData; trigger chunk + embed + Qdrant in SampleRag.API/Endpoints/DocumentsEndpoints.cs and SampleRag.Application/Services
- [X] T016 [US1] Enforce 20 MB max file size and PDF validation in upload pipeline in SampleRag.API/Endpoints/DocumentsEndpoints.cs or Application service

**Checkpoint**: User Story 1 complete — admin can upload PDFs; content ready for RAG

---

## Phase 4: User Story 2 - User asks questions and receives answers with sources (Priority: P2)

**Goal**: User sends a question in a chat; system uses only documents in that chat’s scope, returns answer text and sources (document + page).

**Independent Test**: User sends a question via POST /api/messages with chatId + text; response contains answer and sources array with documentId and pageNumber.

- [ ] T017 [P] [US2] Add ScopeId and OwnerIds to ChatData; add SourceReferences (document + page) to MessageData in SampleRag.Domain/Models/ChatData.cs and MessageData.cs
- [ ] T018 [US2] Implement RAG retrieval: filter by ScopeId in Qdrant, return chunks with DocumentId and PageNumber in SampleRag.Application/Services
- [ ] T019 [US2] Implement RAG answer flow: retrieve chunks, build prompt with context, call LLM via Semantic Kernel, attach source list to system message in SampleRag.Application/Services
- [ ] T020 [US2] Implement POST /api/messages with chatId + text: validate caller is owner, run RAG, return answer and sources in SampleRag.API/Endpoints/MessagesEndpoints.cs
- [ ] T021 [US2] Implement POST /api/chats (title, scopeId, owner from token) with scope access check in SampleRag.API/Endpoints/ChatsEndpoints.cs

**Checkpoint**: User Stories 1 and 2 complete — RAG answers with sources work in scope-bound chats

---

## Phase 5: User Story 3 - Multi-owner chats and auto-created chats with generated titles (Priority: P3)

**Goal**: Send message without chatId creates a new chat with generated title and sets sender as owner; existing owners can add owners via API.

**Independent Test**: POST /api/messages with scopeId + text creates new chat with generated title and returns chat + message; POST /api/chats/{id}/owners adds owner (caller must be owner).

- [ ] T022 [US3] Implement "send message without chatId": accept scopeId + text, create chat with generated title, add user message, run RAG, return new chat and message in SampleRag.API/Endpoints/MessagesEndpoints.cs and SampleRag.Application/Services
- [ ] T023 [US3] Implement chat title generation from first message (LLM or truncation) in SampleRag.Application/Services
- [ ] T024 [US3] Implement POST /api/chats/{id}/owners (add owner; only existing owners) in SampleRag.API/Endpoints/ChatsEndpoints.cs

**Checkpoint**: User Stories 1–3 complete — multi-owner and auto-created chats with generated titles

---

## Phase 6: User Story 4 - Simple feedback (like/dislike) on system answers (Priority: P4)

**Goal**: User can submit like or dislike for a system answer; feedback persisted and idempotent per (messageId, userId).

**Independent Test**: POST /api/messages/{messageId}/feedback with isLike true/false records feedback; resubmit updates (last wins).

- [ ] T025 [P] [US4] Add Feedback model in SampleRag.Domain/Models; add IFeedbackRepository in SampleRag.Domain/Interfaces and implement in SampleRag.Infrastructure/Repositories (MongoDB collection Feedbacks)
- [ ] T026 [US4] Implement POST /api/messages/{messageId}/feedback (upsert like/dislike by messageId and caller userId) in SampleRag.API/Endpoints/MessagesEndpoints.cs and SampleRag.Application/Services
- [ ] T027 [US4] Implement GET /api/messages/{messageId}/feedback (optional) in SampleRag.API/Endpoints/MessagesEndpoints.cs

**Checkpoint**: All user stories complete — feedback recorded and retrievable

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Bug fixes and validation

- [ ] T028 Fix DataGenerator streaming condition if inverted in SampleRag.Application/Services/DataGenerator.cs: ensure the condition that controls when to stream (e.g. when to use streaming vs non-streaming response) is correct; fix if it is reversed (streaming when it should not, or vice versa)
- [ ] T029 Run quickstart.md validation: create scope, upload PDF, create chat, send message, add owner, submit feedback; verify non-admin upload 403 and scope access 403; verify FR-009 (API-only file access): ensure uploads under wwwroot/assets/documents are not served via static files middleware—file access only via API; validate SC-001 (upload confirmation within a few seconds) manually under normal load

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational
- **User Story 2 (Phase 4)**: Depends on Foundational and US1 (documents and chunks must exist for RAG)
- **User Story 3 (Phase 5)**: Depends on Foundational and US2 (messages and RAG flow)
- **User Story 4 (Phase 6)**: Depends on Foundational (only needs messages and auth)
- **Polish (Phase 7)**: Depends on completion of desired user stories

### User Story Dependencies

- **US1 (P1)**: After Foundational only — no other story required
- **US2 (P2)**: After Foundational + US1 (needs scopes, documents, chunks in Qdrant)
- **US3 (P3)**: After Foundational + US2 (extends messages and chats)
- **US4 (P4)**: After Foundational; can be done in parallel with US2/US3 once messages exist

### Within Each User Story

- Models/entities before services; services before endpoints
- Core implementation before integration

### Parallel Opportunities

- T003 and T004 can run in parallel (Phase 1)
- T011 can run in parallel with other Phase 2 tasks after T005–T010 as needed
- T012 is parallel within US1; T017 is parallel within US2; T025 is parallel within US4
- US4 can be started after Foundational in parallel with US2/US3 if message IDs are available

---

## Parallel Example: User Story 1

```text
# After T011–T012, these can run in parallel where independent:
T012: Add ScopeId to DocumentData (Domain + Infrastructure)
T013: Implement PDF chunking (Application) — then T014, T015, T016 in order
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Admin upload PDF, confirm stored and chunked; non-admin rejected
5. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → scope API and auth ready
2. Add US1 → upload and ingestion → MVP
3. Add US2 → RAG answers with sources
4. Add US3 → multi-owner and auto-created chats
5. Add US4 → feedback
6. Polish and quickstart validation

### Suggested MVP Scope

- **MVP**: Phases 1–3 (Setup + Foundational + User Story 1). Delivers: admin-only upload, scope API, PDF chunking and embedding into Qdrant, 20 MB and PDF validation.

---

## Notes

- [P] tasks use different files or have no dependency on incomplete work
- [Story] label maps task to user story for traceability
- Each user story is independently testable at its checkpoint
- Commit after each task or logical group
- File paths reference the existing Clean Architecture layout under the repository root
- Success criteria SC-001 and SC-002 (“few seconds”, “single API interaction”) are validated manually in T029; for production, add numeric targets (e.g. p95 latency, single HTTP request) if needed
