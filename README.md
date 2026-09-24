# Client Report Portal

[![CI](https://github.com/rekhagunathilake/ClientReportPortal/actions/workflows/ci.yml/badge.svg)](https://github.com/rekhagunathilake/ClientReportPortal/actions/workflows/ci.yml)

An internal tool a client-service team at an asset manager would use to assemble, review, and publish each client's quarterly report — commentary, performance, and holdings — with AI-assisted drafting and full observability across the stack.

Built as a hands-on exercise across a deliberate set of modern .NET patterns; each one is a real requirement of the app, not a demo bolted on for its own sake. See [`docs/decisions/`](docs/decisions/) for the reasoning behind each decision.

## What this demonstrates

- **Domain-Driven Design** — a `ReportPackage` aggregate enforcing real invariants (e.g. can't publish until every section is approved)
- **CQRS via MediatR** — commands and queries kept separate end to end
- **Saga pattern** — an orchestrated, compensating multi-step compile pipeline (MassTransit saga state machine)
- **Semantic Kernel** — AI-drafted report commentary via a single-purpose SK plugin, called directly from `Infrastructure` behind an `ICommentaryDraftingService` abstraction
- **.NET Aspire** — local orchestration of Postgres, the API, and the web app, with a shared telemetry dashboard
- **OpenTelemetry** — traces spanning Blazor → API → the Semantic Kernel call for every AI-assisted action
- **Blazor Web App render modes** — Static SSR, Interactive Server, and WebAssembly used deliberately per page, not by default
- **Minimal APIs** — a thin HTTP layer translating to/from MediatR

## Related work

Model Context Protocol is demonstrated separately, in [`dotnet-dev-tools-mcp`](../dotnet-dev-tools-mcp) — a standalone code-review tool exposed to a real MCP host (Claude Code / Claude Desktop). It's deliberately not wired into this app: MCP is for a host that discovers and chooses tools dynamically, and this portal always wants exactly one known thing (drafted commentary), so it calls Semantic Kernel directly instead. See `docs/decisions/` for the full reasoning.

## Architecture

```mermaid
flowchart LR
    subgraph Web[Blazor Web App]
        SSR[Dashboard — Static SSR]
        ISrv[Report Editor — Interactive Server]
        Wasm[Commentary Preview — WebAssembly]
    end

    Web -->|HTTP| Api[Minimal API]
    Api --> App[Application — CQRS / MediatR / Saga]
    App --> Domain[Domain — ReportPackage aggregate]
    App --> Infra[Infrastructure]
    Infra --> DB[(Postgres)]
    Infra -->|Semantic Kernel| Model[LLM provider]

    AppHost[.NET Aspire AppHost] -.orchestrates.-> Web
    AppHost -.orchestrates.-> Api
    AppHost -.orchestrates.-> DB
```

## Report lifecycle

`ReportPackage` and its `Section`s are modeled as an explicit state machine,
with a deliberate split between transitions a human triggers and the one pair
the compile saga is allowed to call.

### ReportPackage

```mermaid
stateDiagram-v2
    [*] --> Draft : Create() (Analyst)

    Draft --> Compiling : StartCompilation() (Analyst)
    Compiling --> Compiled : MarkCompiled() (Saga)
    Compiling --> CompileFailed : MarkCompileFailed() (Saga)
    CompileFailed --> Draft : RetryCompilation() (Analyst)
    Compiled --> Published : Publish() (Analyst/Admin)
    Published --> [*]
```

### Section
```mermaid
stateDiagram-v2
    [*] --> Pending : seeded at Create()

    Pending --> InReview : SubmitSectionForReview() (Analyst)
    InReview --> Approved : ApproveSection() (Reviewer)
    InReview --> Rejected : RejectSection() (Reviewer)
    Rejected --> Pending : ReviseSection() (Analyst)
```

### Who triggers what

| Transition    |	Method  |	Trigger |
|---|---|---|
|   → Draft |	Create()    |	Analyst — "New Report"
|   Draft → Compiling   |	StartCompilation()  |	Analyst — clicks Compile
|   Compiling → Compiled |	MarkCompiled()  |	Saga (MassTransit) — all steps succeeded
|   Compiling → CompileFailed   |	MarkCompileFailed() |	Saga (MassTransit) — a step failed
|   CompileFailed → Draft   |	RetryCompilation()  |	Analyst — decides to retry
|   Compiled → Published    |	Publish()   |	Analyst/Admin — clicks Publish
|   Pending → InReview |	SubmitSectionForReview()    |	Analyst
|   InReview → Approved |	ApproveSection()    |	Reviewer
|   InReview → Rejected |	RejectSection() |	Reviewer
|   Rejected → Pending |	ReviseSection() |	Analyst

The saga only ever calls two methods on the aggregate — MarkCompiled() and MarkCompileFailed(). Every other transition originates from a human action
in the UI; Domain never sees the saga's internal steps (fetch data, render sections, assemble the PDF), only the outcome.

## Project structure
src/
  ClientReportPortal.AppHost/          Aspire orchestration
  ClientReportPortal.ServiceDefaults/  shared OTel + health checks
  ClientReportPortal.Domain/           aggregates, invariants — zero deps
  ClientReportPortal.Application/      CQRS + saga
  ClientReportPortal.Infrastructure/   EF Core, Semantic Kernel integration
  ClientReportPortal.Api/              minimal API endpoints
  ClientReportPortal.Web/              Blazor (server + WASM client)
tests/
  ClientReportPortal.Domain.Tests/
docs/
  adr/                                 architecture decision records


## Getting started
Prerequisites: .NET 10 SDK, Docker Desktop running (for the Postgres container), the [Aspire CLI](https://get.aspire.dev) installed and on PATH.

```bash
dotnet run --project src/ClientReportPortal.AppHost
```

Opens the Aspire dashboard, from which you can reach the Blazor app and inspect traces/logs/metrics across every service.

## Status
Early scaffolding — see [`docs/decisions/`](docs/decisions/) for what's decided and what's still open.

