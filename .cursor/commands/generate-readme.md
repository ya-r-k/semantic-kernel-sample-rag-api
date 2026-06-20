# Generate or refresh `README.md` for SampleRag API

## Role

You are a senior engineer experienced with **ASP.NET Core APIs** and concise open-source README conventions. You write a **short, accurate, scannable** `README.md` that helps someone understand, run, and call the SampleRag API - not a full architecture review.

---

## Separation from `project.md` (mandatory)

| File | Purpose |
|------|---------|
| **`README.md`** (this command) | **Onboarding only**: what the API is, prerequisites, how to run it, minimal usage, links, contributing note, license note. |
| **`project.md`** | Deep technical analysis, design patterns, data flow, test strategy, risks, weak points, and **recommendations** - produced by the **analyze-project** command. |

**Do not** duplicate `project.md` in the README. At most, add one line such as: “For a detailed technical overview and improvement notes, see `project.md`” if that file exists or will be maintained.

**Do not** put long architecture breakdowns, weak points, or prioritized improvement backlogs in the README.

---

## Task

1. Inspect the repository solution, API host, supporting projects, and scripts.
2. Create or update **`README.md`** for this repository as an **API onboarding guide**, not a library README.

---

## Required README sections

Use GitHub-flavored Markdown and a sensible heading hierarchy (H1 once: project title).

1. **Title and one-line description** - What the API is and what it powers.
2. **What it does / why use it** - A few bullets covering the main capabilities of the SampleRag system.
3. **Requirements** - State the actual runtime and local dependencies, such as:
   - .NET SDK version used by the solution
   - Docker Desktop if local containers are used
   - MongoDB, Qdrant, and Ollama if they are run separately
4. **Installation / setup** - Since this repo is source-first, document clone + restore/build, then either:
   - the Docker scripts under `Scripts/`, or
   - running the API locally with the expected config values
5. **Quick start** - A copy-paste-friendly flow showing the typical API usage, such as:
   - create a knowledge scope
   - upload a document
   - create a chat
   - send a message
   - submit feedback
6. **Documentation** - Link to `project.md` for the deep dive and to any spec docs that are useful for API consumers.
7. **Building & testing** - `dotnet build` and `dotnet test` if test projects exist, plus any repo-specific scripts that matter.
8. **Contributing** - Short paragraph and a pointer to `CONTRIBUTING.md` if present.
9. **License** - One line and a link to `LICENSE` if present, or a note that no license file exists yet.

Optional, if valuable and still concise:

- Badges for .NET, Docker, or docs when URLs are known.
- A tiny “API surface” section listing the main route groups, if that helps orientation.

---

## Guidelines

- **Audience**: Developers who want to run the API or integrate with it.
- **Tone**: Direct and practical; avoid marketing fluff.
- **Links**: Prefer relative links for files in the repo.
- **Size**: Keep it easy to skim and well below a long technical spec.

### What NOT to include in `README.md`

- Full architectural breakdown, dependency graphs, or pattern catalogs -> `project.md`
- Detailed test-data inventory, mock strategies, or coverage analysis -> `project.md`
- Prioritized technical debt / risk registers -> `project.md`
- Extensive endpoint reference -> separate docs or API browser; README stays overview + one example flow

---

## After generating

- Ensure commands, paths, and project names match the actual solution.
- If the repo is not NuGet-published, do not invent package installation steps.
- If the repository is source-only, document project usage and local run instructions instead of package-based installation.

**Now create or update `README.md` for this repository according to the above, without overlapping the responsibilities of `project.md`.**
