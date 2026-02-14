# Feature Specification: Demo RAG API

**Feature Branch**: `001-demo-rag-api`  
**Created**: 2025-02-14  
**Status**: Draft  
**Input**: User description: "Требуется разработать демонстрационный пример RAG API. В данное приложение я могу загружать файлы pdf, а потом общатся с помощью языковых моделей с самой системой а система уже определяет сама каие файлы использовать для ответа, и в ответе предоставляет список источников на котрых ответ сформирован, в том числе со страницами. Файлы могут загружаться только администраторами, обычные пользователи делать это не смогут. Чаты должны иметь возможность чтобы ими владели несколько пользователей. еще должна быть возможность отправки сообщения и чтобы самостоятельно создавался чат и название для него генерировалось на основе запроса к системе. должна быть реализована система обратной связи макисмально простая т.е. на ответах от системы пользователь может ставить лайки или дизлайки если ответ правильный или неправильный. и учитывать что это все на строне API т.е. это для API нужно сделать"

## Clarifications

### Session 2025-02-14

- Q: Are uploaded PDFs in a single global pool for all users, or scoped (e.g. by group/tenant/category)? → A: Scoped — documents belong to a group/tenant/category; only questions in that scope (or from those users) use those documents.
- Q: When a chat exists, how do multiple owners get associated with it? → A: Creator at creation; add owners later — creating user is the initial owner; additional owners are added later via an explicit add-owner or invite API.
- Q: How does the API know the caller is an administrator? → A: Identity provider / token — caller presents a token (e.g. JWT); the API validates it and reads a role/claim (e.g. role: Administrator) from the token.
- Q: Is each chat bound to a single scope, or is scope supplied per question? → A: Chat bound to scope — when a chat is created, it is associated with one scope; all questions in that chat use only documents in that scope.
- Q: How are scopes created or defined, and how does the system decide if a user can use a scope? → A: Explicit scope API — scopes are created or managed via the API (e.g. by admins); the system enforces which users can use which scope when creating chats or asking questions.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Admin uploads PDFs for RAG (Priority: P1)

An administrator uploads PDF files into the system, associating each upload with a scope (group/tenant/category). Those files become available for the system to use only when answering questions that fall within the same scope. Only users with an administrator role can upload files; regular users cannot perform uploads.

**Why this priority**: Without uploaded content, the RAG system has nothing to search over. This is the foundation for all chat and answer behaviour.

**Independent Test**: An admin uploads one or more PDFs via the API and can confirm the files are accepted and stored. A non-admin attempt to upload is rejected. Delivers: content ready for RAG.

**Acceptance Scenarios**:

1. **Given** the caller is an administrator, **When** they upload a valid PDF file to a scope (group/tenant/category), **Then** the system accepts and stores the file in that scope and returns a success indication.
2. **Given** the caller is not an administrator, **When** they attempt to upload a file, **Then** the system rejects the request and does not store the file.
3. **Given** the caller is an administrator, **When** they upload a file that is not a PDF or exceeds size limits, **Then** the system rejects the upload with a clear reason.

---

### User Story 2 - User asks questions and receives answers with sources (Priority: P2)

A user sends a question in a chat. Each chat is bound to one scope when created; the system uses only documents in that chat’s scope to produce an answer and returns both the answer text and a list of sources (documents and page numbers) that were used to form the answer.

**Why this priority**: This is the core RAG value: question in, grounded answer with provenance out.

**Independent Test**: User sends a question via the API and receives a response containing answer text and a list of sources (including document identifiers and page numbers). Delivers: verifiable, source-backed answers.

**Acceptance Scenarios**:

1. **Given** a chat is bound to a scope and at least one PDF has been uploaded to that scope, **When** a user sends a question in that chat, **Then** the system returns an answer and a list of sources (document and page references) used for that answer, drawn only from that scope.
2. **Given** no relevant content exists for a question, **When** a user sends the question, **Then** the system responds indicating no or limited sources, or that the answer is not grounded in documents.
3. **Given** a user sends a question, **When** the system responds, **Then** each source in the list can be identified (e.g. by document and page) so the user can trace the answer back.

---

### User Story 3 - Multi-owner chats and auto-created chats with generated titles (Priority: P3)

Users can participate in chats that have multiple owners. When a chat is created (e.g. by sending a first message without an existing chat), the creating user is the initial owner. Additional owners are added later via an explicit add-owner or invite API. A user can send a message without specifying an existing chat, providing the scope for the new chat; the system creates a new chat bound to that scope, generates a title for it based on the message, and sets the sender as the sole initial owner.

**Why this priority**: Enables flexible collaboration and a simple “just send a message” flow without forcing users to create and name chats manually.

**Independent Test**: User sends a message without a chat identifier; the API creates a new chat, assigns a generated title, and returns the chat and message. Chats can be shared with multiple owners. Delivers: low-friction chat creation and shared ownership.

**Acceptance Scenarios**:

1. **Given** a user sends a message without an existing chat and supplies a scope they are allowed to use, **When** the request is processed, **Then** the system creates a new chat bound to that scope, generates a title from the message content, sets the user as the initial owner, and returns the new chat and the message.
2. **Given** a chat exists and the caller is an owner, **When** they add another user as owner via the add-owner/invite API, **Then** that user becomes an owner and can send and receive messages in that chat.
3. **Given** a chat exists with multiple owners, **When** any owner sends a message, **Then** the message is added to that chat and the response includes the chat and message.
4. **Given** a user sends a message in an existing chat they own or share, **When** the request is processed, **Then** the message is added to that chat and the response includes the chat and message.

---

### User Story 4 - Simple feedback (like/dislike) on system answers (Priority: P4)

A user can indicate whether a system answer was helpful by submitting a like (correct/helpful) or dislike (incorrect/not helpful). The system records this feedback for the specific answer.

**Why this priority**: Enables minimal, low-effort feedback to improve or evaluate answer quality without complex forms.

**Independent Test**: User submits like or dislike for a given system answer; the API records the feedback and confirms. Delivers: collectible signal on answer correctness/quality.

**Acceptance Scenarios**:

1. **Given** the system has returned an answer to the user, **When** the user submits a like, **Then** the system records positive feedback for that answer and confirms.
2. **Given** the system has returned an answer to the user, **When** the user submits a dislike, **Then** the system records negative feedback for that answer and confirms.
3. **Given** the user has already submitted feedback for an answer, **When** they submit feedback again (e.g. change from like to dislike), **Then** the system updates the recorded feedback for that answer.

---

### Edge Cases

- What happens when the user asks a question but no documents have been uploaded (in that scope)? System should respond without crashing and indicate that no or limited sources are available.
- What happens when a PDF fails processing (e.g. corrupted or unsupported)? System rejects or marks the file as failed and does not use it for RAG.
- What happens when a message is sent with an invalid or deleted chat identifier? System returns a clear error and does not create data in an invalid state.
- What happens when a non-owner attempts to add an owner to a chat? System rejects the request; only existing owners can add new owners.
- What happens when a user tries to create a chat in a scope they are not allowed to use? System rejects the request; the system enforces scope access when creating chats.
- How does the system handle duplicate or near-duplicate feedback (e.g. multiple likes from the same user for the same answer)? Last feedback wins or feedback is idempotent per user and answer.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow only administrators to upload files; non-administrator upload attempts MUST be rejected.
- **FR-001b**: System MUST provide an explicit scope API so that scopes can be created or managed (e.g. by administrators); the system MUST enforce which users can use which scope when creating chats, sending questions, or uploading files.
- **FR-002**: System MUST accept PDF file uploads and store them in a scope (group/tenant/category) so they can be used only when answering questions in that scope.
- **FR-003**: System MUST enforce a maximum file size for uploads (per project constraints).
- **FR-004**: System MUST answer user questions in a chat using only stored documents within that chat’s scope and return both the answer text and a list of sources (document and page references) used to form the answer.
- **FR-005**: System MUST support chats that can have multiple owners (users who can send and receive messages in the chat); the creating user is the initial owner. Each chat MUST be bound to exactly one scope at creation.
- **FR-006**: System MUST allow sending a message without an existing chat; in that case it MUST create a new chat bound to a scope supplied by the caller (only if the caller is allowed to use that scope), generate a title for the chat based on the message content, and set the sender as the sole initial owner.
- **FR-007**: System MUST provide an explicit add-owner (or invite) API so that an existing owner can add additional owners to a chat; only an existing owner MAY add new owners.
- **FR-008**: System MUST allow users to submit simple feedback (like or dislike) for a specific system answer and MUST persist that feedback.
- **FR-009**: System MUST expose all behaviour above via API only (no direct file or database access for these operations).

### Key Entities

- **Scope (group/tenant/category)**: A container that groups documents and defines which users can use it for Q&A, uploads, and chat creation; scopes are created or managed via the explicit scope API (e.g. by admins); the system enforces scope access for creating chats, asking questions, and uploading files.
- **Document/File**: An uploaded PDF; has identity, storage reference, scope membership, and may have extracted text and page-level structure for retrieval and source citation.
- **Chat**: A conversation container; has an identity, a title (possibly system-generated), a single scope (bound at creation), and a set of owners; the creator is the initial owner; additional owners are added via the add-owner/invite API; all RAG answers in the chat use only documents in the chat’s scope.
- **Message**: A single user or system message in a chat; has content, sender (user or system), and ordering within the chat.
- **Source reference**: A reference to a part of a document (e.g. document identifier and page number or range) used to ground an answer.
- **Feedback**: A user’s like or dislike for a specific system answer (message); tied to the answer and the user.

## Assumptions

- Caller identity and role (administrator vs regular user) are determined by a validated token (e.g. issued by an identity provider); the API validates the token and reads role/claims (e.g. administrator) from it. The spec does not define the identity provider or token format in detail.
- “Administrator” and “user” are roles or attributes supplied in or derived from that token when processing requests.
- Generated chat titles are derived from the first user message (e.g. summarised or truncated) and need only be human-readable, not unique.
- Documents are scoped (group/tenant/category). Scopes are created and managed via the API; the system enforces which users can use which scope. Each chat is bound to one scope at creation; the system uses only documents in that chat’s scope when producing answers for questions in that chat.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: An administrator can upload a PDF and receive confirmation of success within a few seconds under normal load.
- **SC-002**: A user can send a question and receive an answer with at least one source reference (when relevant content exists) in a single API interaction.
- **SC-003**: A user can start a new conversation by sending one message and receive back a new chat with a generated title and the stored message.
- **SC-004**: A user can submit like or dislike for a system answer and the feedback is recorded and can be retrieved or used for evaluation.
- **SC-005**: Non-administrator callers cannot successfully upload files; 100% of such attempts are rejected by the API.
