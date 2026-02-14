# RAG, Models (Ollama), Vector Stores & Kernel Memory — Requirements Quality Checklist

**Purpose**: Validate that requirements for RAG behaviour, language/embedding models (Ollama), vector stores, and kernel memory are complete, clear, consistent, and measurable.  
**Created**: 2025-02-14  
**Feature**: [spec.md](../spec.md) | **Constraints**: [.specify/memory/constitution.md](../../../.specify/memory/constitution.md)

**Note**: This checklist validates the quality of requirements (completeness, clarity, consistency), not implementation behaviour.

---

## Requirement Completeness

- [ ] CHK001 Are requirements for which document set is used for retrieval (scope boundary) explicitly stated for RAG answers? [Completeness, Spec §FR-004, Key Entities]
- [ ] CHK002 Are requirements for source citation (document + page references) in RAG responses fully specified so they can be verified? [Completeness, Spec §FR-004, User Story 2]
- [ ] CHK003 Is the lifecycle of uploaded documents (storage → indexing → availability for retrieval) specified in requirements? [Gap]
- [ ] CHK004 Are requirements for embedding and indexing of PDF content (e.g. chunking, scope association) documented so retrieval behaviour is testable? [Gap]
- [ ] CHK005 Are requirements for the vector store scope isolation (per-scope collections or filtering) explicitly stated so that “only documents in that scope” is verifiable? [Completeness, Spec §FR-004, Constitution IV]
- [ ] CHK006 Is the permitted model provider (Ollama only) for both LLM and embeddings documented as a non-negotiable constraint? [Completeness, Constitution I]

---

## Requirement Clarity

- [ ] CHK007 Is “system uses only documents in that chat’s scope” defined in a way that can be objectively verified (e.g. no cross-scope retrieval)? [Clarity, Spec §FR-004]
- [ ] CHK008 Are “document and page references” in source lists defined with enough specificity (e.g. identifier format, page range) to be testable? [Clarity, Spec §Key Entities – Source reference]
- [ ] CHK009 Is the boundary between “kernel memory” (or semantic store) and “vector store” (Qdrant) specified so that responsibilities are unambiguous? [Clarity, Gap]
- [ ] CHK010 Is “answer grounded in documents” or “no/limited sources” behaviour specified so that empty or irrelevant retrieval outcomes are testable? [Clarity, Spec §User Story 2, Edge Cases]
- [ ] CHK011 Are requirements for generated chat titles (e.g. “derived from first user message”) specific enough to define acceptable vs unacceptable outputs? [Clarity, Spec §Assumptions]

---

## Requirement Consistency

- [ ] CHK012 Do scope-related requirements align across FR-001b, FR-002, FR-004, FR-005, FR-006 and the Scope entity (create, enforce, use)? [Consistency, Spec §FR-*]
- [ ] CHK013 Are “documents in that scope” and “chat’s scope” used consistently so that retrieval scope is unambiguous in all RAG scenarios? [Consistency, Spec §FR-004, §Key Entities]
- [ ] CHK014 Does the constitution’s “Qdrant as vector store” and “Ollama only” align with any plan/spec references to external model or store choices? [Consistency, Constitution I, IV]

---

## Acceptance Criteria Quality

- [ ] CHK015 Can “answer with at least one source reference (when relevant content exists)” be verified without implementation details? [Measurability, Spec §SC-002]
- [ ] CHK016 Are acceptance criteria for “no or limited sources” / “not grounded in documents” defined in measurable terms? [Acceptance Criteria, Spec §User Story 2, Edge Cases]
- [ ] CHK017 Is “feedback is recorded and can be retrieved or used for evaluation” specified so that persistence and retrieval of feedback are testable? [Measurability, Spec §SC-004, §FR-008]

---

## Scenario Coverage

- [ ] CHK018 Are requirements defined for the flow: upload PDF → process → store in vector store → available for RAG within scope? [Coverage, Spec §User Story 1, §FR-002]
- [ ] CHK019 Are requirements specified for the RAG flow: user question → retrieval from scope → LLM answer → source list returned? [Coverage, Spec §User Story 2, §FR-004]
- [ ] CHK020 Are exception scenarios (e.g. PDF processing failure, empty scope, retrieval timeout) addressed in requirements? [Coverage, Spec §Edge Cases]
- [ ] CHK021 Is behaviour when the vector store or model is unavailable or slow specified or explicitly deferred? [Gap, Exception Flow]

---

## Edge Case Coverage

- [ ] CHK022 Are requirements for “no documents uploaded (in that scope)” explicitly stated so the system’s response is verifiable? [Edge Case, Spec §Edge Cases]
- [ ] CHK023 Is behaviour when a document is deleted or removed from a scope specified (e.g. impact on existing answers or future retrieval)? [Gap]
- [ ] CHK024 Are requirements for duplicate or overlapping content across documents in a scope (e.g. ranking, deduplication) defined or explicitly out of scope? [Edge Case, Gap]
- [ ] CHK025 Is the maximum size or count of retrievable chunks per question (or per scope) specified or explicitly left undefined? [Edge Case, Gap]

---

## Non-Functional & Constraints

- [ ] CHK026 Are performance or latency expectations for RAG answers (e.g. “within few seconds”) quantified where they affect acceptance? [Clarity, Spec §SC-001, §SC-002]
- [ ] CHK027 Is the 20 MB upload limit and its enforcement referenced in requirements so it is traceable to the constitution? [Traceability, Constitution V, Spec §FR-003]
- [ ] CHK028 Are requirements for Semantic Kernel and kernel memory usage (e.g. what is stored, keyed by scope/chat) documented so design is constrained? [Gap]

---

## Dependencies & Assumptions

- [ ] CHK029 Is the assumption that “caller identity and role from token” is sufficient for admin vs user and scope access explicitly stated and validated? [Assumption, Spec §Assumptions]
- [ ] CHK030 Are dependencies on an external identity provider and token format documented so integration scope is clear? [Dependency, Spec §Assumptions]
- [ ] CHK031 Is the assumption that Ollama provides both LLM and embedding models (or separate model names) documented where it affects requirements? [Assumption, Constitution I]

---

## Ambiguities & Conflicts

- [ ] CHK032 Is “page-level structure” for documents defined so that source references (e.g. page number or range) are unambiguous? [Ambiguity, Spec §Key Entities – Document/File]
- [ ] CHK033 Are there any conflicts between technology-agnostic spec language and constitution constraints (Ollama, Qdrant, Semantic Kernel) that need resolving? [Conflict]
- [ ] CHK034 Is “kernel memory” (Semantic Kernel memory store) distinguished from “vector store” (Qdrant) in requirements or constraints so roles are clear? [Ambiguity, Gap]

---

## Notes

- Check items off as completed: `[x]`
- Add comments or findings inline; reference spec section or constitution where relevant.
- Items marked [Gap] indicate likely missing requirements; resolve in spec or plan before implementation.
