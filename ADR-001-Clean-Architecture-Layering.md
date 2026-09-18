# ADR-001: Clean Architecture layering

**Status:** Accepted
**Date:** 2026-09-16

## Context

This app needs the domain logic testable in isolation, and the boundary between "business rules" and
"infrastructure concerns" (database, HTTP, Semantic Kernel calls) needs to be visible in the code structure itself, not just in intent.

Left unstructured, it's easy for EF Core entities, HTTP concerns, or MediatR plumbing to leak into the `ReportPackage` aggregate, which would
undermine the exact thing (DDD invariants) this project is meant to practice.

## Decision

Split the solution into five projects, with dependencies flowing inward only:

```
Domain          — no project references. Aggregates, entities, domain
                  events, invariants. Pure C#, no EF/HTTP/MediatR types.
Application  →  Domain
                  CQRS commands/queries (MediatR), the compile saga
                  orchestration, repository *interfaces*, and the
                  ICommentaryDraftingService abstraction.
Infrastructure → Application (and transitively Domain)
                  EF Core + Postgres, repository implementations, the
                  Semantic Kernel-backed commentary drafting service,
                  MassTransit saga persistence.
Api          →  Application, Infrastructure, ServiceDefaults
                  Minimal API endpoints. Thin — translates HTTP to
                  MediatR commands/queries and back.
Web          →  ServiceDefaults
                  Blazor. Calls Api over HTTP; does not reference
                  Domain/Application/Infrastructure directly.
```

`Domain` having zero project references is the load-bearing rule here — it's what makes the aggregate unit-testable with no database, no HTTP
context, and no mocks.

`AppHost` and `ServiceDefaults` sit outside this stack — they're Aspire orchestration/telemetry concerns, not application layers, so the
dependency rule above doesn't apply to them.

## Consequences

**Positive**
- `ReportPackage`'s invariants can be unit-tested in `Domain.Tests` with no infrastructure spun up — fast feedback, and confidence the rules actually hold.
- The dependency direction is enforceable and easy to explain: "what can `Domain` know about?" → nothing outside itself.
- `Infrastructure` is swappable in principle (e.g., a different ORM, or a fake `ICommentaryDraftingService` for tests) without touching
  `Application` or `Domain`.

**Negative / trade-offs**
- Five projects (plus `AppHost`/`ServiceDefaults`) is more ceremony than an app this size strictly needs — a single project would ship faster.
  Accepted because the layering *is* the point of the exercise, not overhead incidental to it.
- Requires discipline: it's easy to accidentally reference an EF type from `Application` (e.g. returning `DbSet<T>` from a query) and quietly
  violate the boundary. No analyzer enforces this yet — worth adding an architecture test (e.g. with `NetArchTest`) later if this becomes a
  real risk.
- `Web` talking to `Api` only over HTTP (rather than referencing `Application` directly and calling MediatR in-process) costs one
  extra network hop per interaction — deliberate, confirmed by the walking skeleton (`Weather.razor` → named `"Api"` `HttpClient` on the
  `https+http://api` service-discovery address), so the Api's contract is exercised the same way any external client would use it, and the
  OpenTelemetry trace across Blazor → Api is real rather than simulated.

## Alternatives considered

- **Single project, folders instead of assemblies** — rejected: nothing stops a domain type from taking an EF Core dependency, so the boundary
  isn't real, just a convention.
- **`Web` calls `Application` in-process (no separate `Api`)** — rejected for this project specifically: the plan wants a real minimal-API layer
  to practice, and an in-process call wouldn't produce a genuine cross-service trace for the OpenTelemetry goal.

## Follow-up

- Confirmed working: the walking skeleton (`AppHost` → `Api` → `Web`) proves service discovery and OpenTelemetry tracing end to end without
  violating this layering.
- Not yet decided: whether to add an automated architecture test enforcing the dependency rule, or leave it as a documented-but-
  unenforced convention. Revisit once `Application`/`Infrastructure` have real code in them.
