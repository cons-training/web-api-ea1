# GDB Migration - GitHub Issues Task Breakdown

This document converts the two migration-roadmap documents into a GitHub-issues-style delivery plan for a three-person team. It is intended to be copied into GitHub as one master issue with the child issues below.

## Source Documents

- `Documentation/Console_to_ASPNET_Core_WebAPI_Migration_Roadmap.md`
- `Documentation/Console_to_ASPNET_Core_WebAPI_Migration_Roadmap_Part2.md`

The source roadmap defines a migration from the .NET 8 GDB console application to an ASP.NET Core 8 Web API, followed by refactoring the console application into an HTTP client of that API. The breakdown below preserves the source roadmap's release order, sprint structure, testing strategy, quality gates, risks, and stated assumptions.

## Planning Baseline

| Planning item | Baseline |
|---|---|
| Team | Dev A - architecture, DI, database, integration lead; Dev B - API/controllers and functional testing; Dev C - middleware, auth, Docker, HTTP client |
| Sprint length | 4 working days |
| Daily productive capacity | 6 hours per developer |
| Nominal sprint capacity | 24 hours per developer / 72 person-hours for the team |
| Total duration | 10 sprints / 40 working days / approximately 8 weeks |
| Approximate start | 2026-10-01, as stated in the roadmap |
| Approximate finish | 2026-11-25, assuming weekdays only and no holidays |
| Branch flow | `main` -> `develop` -> `feature/*` -> `release/rN` |
| Target framework | .NET 8 / ASP.NET Core 8 |
| Data access | SQL Server stored procedures through raw ADO.NET; InMemory data is test/development support only |
| API architecture | Controllers -> application services -> domain models -> infrastructure repositories -> SQL Server |

The roadmap estimates approximately 660 person-hours of engineering work, while the allocation table shows 720 person-hours of team capacity. The difference is the planned contingency, review, testing, ceremonies, and release work. Child-issue estimates are therefore delivery estimates, not a promise that every hour is feature coding.

## GitHub Labels and Milestones

Suggested labels:

- `migration` - work belonging to this program
- `release:r1` through `release:r8` - target release
- `sprint:1` through `sprint:10` - target sprint
- `area:api`, `area:database`, `area:di`, `area:validation`, `area:middleware`, `area:security`, `area:docker`, `area:console-client`, `area:testing`, `area:documentation`
- `priority:p0`, `priority:p1`, `priority:p2`
- `type:feature`, `type:refactor`, `type:test`, `type:documentation`, `type:release`
- `blocked`, `needs-business-decision`, `risk-critical`

Suggested milestones:

| Milestone | Sprint | Approximate weekday window | Release |
|---|---:|---|---|
| Sprint 1 | 1 | Oct 1-6, 2026 | R1 |
| Sprint 2 | 2 | Oct 7-12, 2026 | R2 |
| Sprint 3 | 3 | Oct 13-16, 2026 | R2 |
| Sprint 4 | 4 | Oct 19-22, 2026 | R3 |
| Sprint 5 | 5 | Oct 23-28, 2026 | R4 |
| Sprint 6 | 6 | Oct 29-Nov 3, 2026 | R5 |
| Sprint 7 | 7 | Nov 4-9, 2026 | R6 |
| Sprint 8 | 8 | Nov 10-13, 2026 | R7 |
| Sprint 9 | 9 | Nov 16-19, 2026 | R8 |
| Sprint 10 | 10 | Nov 20-25, 2026 | R8 |

Dates are approximate. Recalculate them if the team uses a different holiday calendar or sprint start date.

---

## Master Issue

### Title

`[MASTER] Migrate GDB Console Application to ASP.NET Core Web API and HTTP Client`

### Labels

`migration`, `type:feature`, `priority:p0`

### Milestone

`GDB Migration Program - Sprints 1-10`

### Description

Migrate the existing GDB .NET 8 console application into a production-ready ASP.NET Core 8 Web API while preserving existing banking business behavior. Deliver the migration incrementally through releases R1-R8, then refactor the console application so it communicates with the API over HTTP instead of directly calling application, domain, or repository components.

The migration must preserve the existing SQL Server stored-procedure and raw ADO.NET approach. Entity Framework Core is not part of this work. The existing domain `AccountFactory` remains a legitimate domain pattern; the factories targeted for removal are the application and repository construction factories described in the roadmap.

### In scope

- Create the `GDB.Api` ASP.NET Core 8 Web API project and solution structure.
- Expose the nine existing business operations as API endpoints.
- Preserve business logic and verify database behavior against SQL Server.
- Replace construction factories with ASP.NET Core dependency injection.
- Add request validation and a consistent `ApiResponse<T>` response shape.
- Add global exception mapping, `ProblemDetails`, CORS, and URL-based API versioning.
- Add JWT authentication and role-based authorization.
- Containerize the API with Docker and verify database connectivity from the container.
- Refactor the console application into an authenticated HTTP client.
- Add unit, integration, regression, security, container, and end-to-end verification.
- Maintain release documentation, changelog entries, setup guides, and migration decisions.

### Out of scope or explicitly deferred

- Replacing raw ADO.NET and stored procedures with Entity Framework Core.
- Production-grade identity storage; the roadmap permits a hardcoded user store for the R6 phase.
- PIN hashing; this is recorded as a post-migration security enhancement.
- An external CI/CD pipeline; local Docker is sufficient for this migration phase.
- New business behavior beyond the existing banking operations and the access-control assumptions requiring confirmation.

### Master acceptance criteria

- [ ] R1-R8 child issues are completed or explicitly accepted as deferred.
- [ ] The API builds and starts on .NET 8 with the documented configuration.
- [ ] All nine business operations are available through versioned API endpoints.
- [ ] SQL Server operations use the existing stored procedures and pass the release test matrix.
- [ ] The console application uses the API for all business operations by R8.
- [ ] No direct console references remain to the API's service or repository implementation layers.
- [ ] Authentication, authorization, error handling, CORS, API versioning, and Docker gates pass.
- [ ] All previous release tests pass before each later release is tagged.
- [ ] Documentation and `CHANGELOG.md` are updated for every release.
- [ ] `develop` is merged to `release/rN`, the release is tagged, and a stakeholder demo is completed.

### Dependency and sequencing rule

Child issues are intentionally ordered. Do not start R3 DI removal until R2 behavior is verified. Do not start R6 authorization until R5 versioned routes and error handling are stable. Do not start R8 console replacement until the R7 containerized API is reachable from the host. A child issue may be split further in GitHub, but its acceptance criteria must remain intact.

### Program-level risks to track

- `RSK-1` - static `DataBaseConnectionManager` coupling; mitigate in R3 with injected configuration or a connection factory.
- `RSK-2` - missing stored procedures; validate required procedures and document them in R2.
- `RSK-3` - `GetAwaiter().GetResult()` async-over-sync risk; convert the affected service path to proper async in R2.
- `RSK-7` - JWT secret exposure; use environment variables or secret storage and document the limitation in R6.
- `RSK-8` - Docker-to-host SQL Server networking; verify `host.docker.internal` or the documented host address in R7.
- `RSK-9` - console HTTP timeout and availability behavior; implement explicit timeouts and user-facing failures in R8.
- `RSK-11` - plaintext PIN storage; keep outside this migration but create a follow-up security backlog item.

### Release gates

Every child issue is subject to the applicable gates:

- `G1 Build`: `dotnet build` succeeds.
- `G2 Test`: existing tests and new tests pass.
- `G3 Review`: at least one other developer approves the PR.
- `G4 Integration`: sprint functionality is verified through the API boundary.
- `G5 Regression`: all prior release tests pass before release.
- `G6 Security`: R6 authentication and 401/403 tests pass.
- `G7 Container`: R7 image builds and runs with a healthy API.
- `G8 E2E`: R8 console -> API -> database scenarios pass.

---

## Child Issue 1 - R1 Foundation and Repository Setup

### Title

`[R1][Sprint 1] Create GDB.Api project, solution structure, configuration, and health endpoint`

### Assignee and estimate

- Primary: Dev A for repository and configuration coordination
- Supporting: Dev B for project creation; Dev C for packages and API startup
- Estimate: 21 hours of named work; reserve the rest of the 72-hour sprint for integration, review, and release verification
- Dependencies: none
- Labels: `migration`, `release:r1`, `sprint:1`, `area:api`, `area:documentation`, `priority:p0`

### Description

Create the ASP.NET Core solution and API foundation while keeping the existing console behavior available. Establish the branch, package, configuration, Swagger, logging, and middleware conventions that all later releases depend on.

### Atomic checklist

- [ ] Inspect the repository, branches, and commit history; record the current state.
- [ ] Document the `main` -> `develop` -> `feature/*` -> `release/*` workflow.
- [ ] Document commit conventions, PR expectations, and code-review checklist.
- [ ] Create or confirm the `develop` branch.
- [ ] Create `GDB.sln` and add the existing application project.
- [ ] Create `GDB.Api` targeting `net8.0`.
- [ ] Add the API-to-application project reference.
- [ ] Convert `GdbWebApi` from executable output to a reusable library without breaking its current build path.
- [ ] Move application startup responsibility to `GDB.Api/Program.cs`.
- [ ] Add Swashbuckle and Serilog packages compatible with .NET 8.
- [ ] Remove the duplicate `System.Data.SqlClient` dependency where safe; retain `Microsoft.Data.SqlClient`.
- [ ] Configure controllers, JSON serialization, Swagger/OpenAPI, and middleware ordering.
- [ ] Add `appsettings.json` and `appsettings.Development.json` without committing secrets.
- [ ] Update `.gitignore` for ASP.NET Core build output and local configuration.
- [ ] Add temporary `GET /api/health` through `HealthController`.
- [ ] Build both projects and verify the API starts.
- [ ] Make the initial foundation commit on `develop`.

### Acceptance criteria

- [ ] `dotnet build GDB.sln` succeeds without errors.
- [ ] API starts on its configured port.
- [ ] Swagger UI loads and shows the health endpoint.
- [ ] `GET /api/health` returns HTTP 200 with a timestamp or equivalent health payload.
- [ ] The original application can still be built and run for regression comparison.
- [ ] No credentials or connection secrets are committed.

### Release closeout

Verify R1 with build, startup, Swagger, health, branch, and configuration checks. Update `README.md`, `CHANGELOG.md`, and `MIGRATION-DECISIONS.md`; merge to `release/r1` and tag the release.

## Child Issue 2 - R2 Account API Migration

### Title

`[R2][Sprint 2] Migrate account operations to API controllers with temporary factory wiring`

### Assignee and estimate

- Primary: Dev B
- Supporting: Dev A for database and temporary service wiring
- Estimate: 24 hours of Sprint 2 implementation work
- Dependencies: Child Issue 1
- Labels: `migration`, `release:r2`, `sprint:2`, `area:api`, `area:database`, `type:feature`, `priority:p0`

### Description

Expose account behavior through an `AccountsController` while temporarily using the existing factories. Keep the console application working directly until R8 and prove that API responses match existing behavior.

### Atomic checklist

- [ ] Change `AccountService`, `AccountRepositoryDB`, and required factory types to public visibility where cross-project access requires it.
- [ ] Create `AccountsController` with `[ApiController]` and the account route.
- [ ] Implement `GET /api/accounts/{accountNumber}`.
- [ ] Implement `GET /api/accounts`.
- [ ] Implement `GET /api/accounts/{accountNumber}/balance`.
- [ ] Implement `POST /api/accounts` for Savings, Current, FixedDeposit, and Salary account types.
- [ ] Implement `DELETE /api/accounts/{accountNumber}`.
- [ ] Return the intended 200, 201, 400, and 404 status codes.
- [ ] Register `IAccountService` through the existing factory as temporary R2 wiring.
- [ ] Move the connection string to `appsettings.json` and verify API database access.
- [ ] Convert `AccountService.CreateAccount` away from blocking async-over-sync behavior.
- [ ] Verify the required stored procedures exist or produce a clear startup/test failure.

### Acceptance criteria

- [ ] Account endpoints compile and are discoverable in Swagger.
- [ ] Valid account create, read, list, balance, and close operations work against SQL Server.
- [ ] Missing accounts return 404 and invalid requests return the documented error status.
- [ ] The console app still performs its existing operations directly.
- [ ] No deadlock-prone `GetAwaiter().GetResult()` remains on the migrated account path.

### Release closeout

Document account endpoints in `API-ENDPOINTS.md`, document architecture in `ARCHITECTURE.md`, run the R2 account test cases, and retain the factory wiring until the R3 issue is complete.

## Child Issue 3 - R2 Transaction API Migration

### Title

`[R2][Sprint 2] Migrate transaction operations to API controllers`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev B for controller/API review; Dev A for database verification
- Estimate: 24 hours of Sprint 2 implementation work
- Dependencies: Child Issue 1; may merge with Child Issue 2 only after shared routing and service contracts are agreed
- Labels: `migration`, `release:r2`, `sprint:2`, `area:api`, `area:database`, `type:feature`, `priority:p0`

### Description

Expose deposit, withdrawal, transfer, and recent-transaction operations through `TransactionsController`, using the existing transaction services and commands behind the temporary R2 factory wiring.

### Atomic checklist

- [ ] Create `TransactionsController` with API routing.
- [ ] Implement `POST /api/transactions/deposit`.
- [ ] Implement `POST /api/transactions/withdraw`.
- [ ] Implement `POST /api/transactions/transfer`.
- [ ] Implement `GET /api/transactions/{accountNumber}/recent`.
- [ ] Return the correct status codes for valid requests, invalid PINs, insufficient balance, invalid amounts, and missing accounts.
- [ ] Register `ITransactionService` through the existing factory.
- [ ] Register `ITransactionQueryService` through the existing factory.
- [ ] Verify synchronous repository calls do not break API request execution; record any async follow-up.
- [ ] Exercise all transaction endpoints through Swagger.

### Acceptance criteria

- [ ] Deposit increases the account balance.
- [ ] Withdrawal decreases balance only when PIN, balance, and minimum-balance rules pass.
- [ ] Transfer updates both accounts and rejects same-account transfers.
- [ ] Recent transactions returns history and a valid empty result when no history exists.
- [ ] Existing console transaction behavior remains unchanged.

## Child Issue 4 - R2 Stabilization, Regression, and Release

### Title

`[R2][Sprint 3] Stabilize all API endpoints and release the first business API increment`

### Assignee and estimate

- Primary: Dev A for integration and release ownership
- Supporting: Dev B for API/Swagger tests; Dev C for cross-endpoint and performance spot checks
- Estimate: Sprint 3, 72 person-hours of team capacity including testing, fixes, documentation, and release ceremony
- Dependencies: Child Issues 2 and 3
- Labels: `migration`, `release:r2`, `sprint:3`, `area:testing`, `area:documentation`, `type:release`, `priority:p0`

### Atomic checklist

- [ ] Re-run all account scenarios: create, view, list, balance, and close.
- [ ] Re-run all transaction scenarios: deposit, withdraw, transfer, and recent history.
- [ ] Test invalid account, invalid PIN, insufficient balance, minimum-balance, invalid amount, and same-account cases.
- [ ] Compare API behavior with the console application for all nine operations.
- [ ] Verify error responses and Swagger request/response documentation.
- [ ] Perform a cross-endpoint integration and basic performance spot check.
- [ ] Fix all P0/P1 defects and record deferred issues.
- [ ] Update `API-ENDPOINTS.md`, `ARCHITECTURE.md`, `CHANGELOG.md`, and `MIGRATION-DECISIONS.md`.
- [ ] Complete R2 release verification, tag the release, and demo the working API.

### Acceptance criteria

- [ ] All R2 test cases pass, including the console regression test.
- [ ] All nine operations are available from the API.
- [ ] The API can connect to SQL Server using the migrated connection string.
- [ ] R2 release verification checklist is complete.
- [ ] `develop` is merged into `release/r2` and tagged.

## Child Issue 5 - R3 Repository DI and Connection Management

### Title

`[R3][Sprint 4] Replace repository factories and static connection access with scoped DI`

### Assignee and estimate

- Primary: Dev A
- Supporting: Dev C for registration integration; Dev B for endpoint regression
- Estimate: 24 hours of Dev A's R3 allocation plus team integration time
- Dependencies: Child Issue 4
- Labels: `migration`, `release:r3`, `sprint:4`, `area:di`, `area:database`, `type:refactor`, `risk-critical`, `priority:p0`

### Atomic checklist

- [ ] Refactor `DataBaseConnectionManager` to receive configuration or a connection string through its constructor.
- [ ] Ensure connection creation is scoped and disposable per request.
- [ ] Register `IAccountRepository` to `AccountRepositoryDB` as scoped.
- [ ] Register `ITransactionRepository` to `TransactionRepositoryDB` as scoped.
- [ ] Remove repository factory usage from services.
- [ ] Keep InMemory repositories available only for tests or explicitly selected development scenarios.
- [ ] Verify provider registration behavior with .NET 8 and `Microsoft.Data.SqlClient`.
- [ ] Document required stored procedures and connection settings.

### Acceptance criteria

- [ ] No production request depends on repository factory construction.
- [ ] Database connections are disposed per request with no connection-pool exhaustion in verification.
- [ ] App starts without DI or provider-resolution errors.
- [ ] All R2 account and transaction tests pass after the refactor.

## Child Issue 6 - R3 Service DI and Factory Removal

### Title

`[R3][Sprint 4] Register services and transaction commands through ASP.NET Core DI`

### Assignee and estimate

- Primary: Dev C for service-registration composition
- Supporting: Dev A for DI architecture; Dev B for behavior regression
- Estimate: 24 hours of Dev C's R3 allocation plus integration time
- Dependencies: Child Issue 5
- Labels: `migration`, `release:r3`, `sprint:4`, `area:di`, `type:refactor`, `priority:p0`

### Atomic checklist

- [ ] Add repository constructor dependencies to `AccountService`.
- [ ] Add repository and command dependencies to `TransactionQueryService` and `TransactionService`.
- [ ] Register `IAccountService`, `ITransactionService`, and `ITransactionQueryService` as scoped.
- [ ] Register deposit, withdraw, and transfer commands as scoped.
- [ ] Create `ServiceRegistration.cs` or the agreed registration extension.
- [ ] Remove usage of `TransactionCommandFactory`.
- [ ] Remove the six application/repository factory classes targeted by the roadmap.
- [ ] Confirm the domain `AccountFactory` remains available and intentionally preserved.
- [ ] Verify no captive dependency or incorrect lifetime is introduced.

### Acceptance criteria

- [ ] All production services resolve through the built-in DI container.
- [ ] Zero targeted factory classes remain in the production migration path.
- [ ] All R2 endpoint behavior remains unchanged.
- [ ] DI registration order matches the documented dependency order.

## Child Issue 7 - R3 DI Verification and Release

### Title

`[R3][Sprint 4] Verify DI behavior and release factory removal`

### Assignee and estimate

- Primary: Dev A
- Supporting: Dev B and Dev C
- Estimate: 24 hours of shared Sprint 4 verification, fixes, documentation, and release work
- Dependencies: Child Issues 5 and 6
- Labels: `migration`, `release:r3`, `sprint:4`, `area:testing`, `type:release`, `priority:p0`

### Atomic checklist

- [ ] Start the application from a clean build with no DI resolution errors.
- [ ] Re-run every R2 API test.
- [ ] Verify service and repository lifetimes.
- [ ] Verify connection disposal and repeated-request behavior.
- [ ] Verify there are no targeted factory files or references left.
- [ ] Update `DI-CONFIGURATION.md`, `CHANGELOG.md`, and migration decisions.
- [ ] Complete R3 release verification, merge, tag, and demo.

### Acceptance criteria

- [ ] R3 tests pass, including the no-factory and connection-disposal checks.
- [ ] All endpoints behave identically to R2.
- [ ] R3 release gate and release checklist are complete.

## Child Issue 8 - R4 Request DTO Validation

### Title

`[R4][Sprint 5] Add request DTOs and declarative validation rules`

### Assignee and estimate

- Primary: Dev B for request-model ownership
- Supporting: Dev A for integration; Dev C for negative testing
- Estimate: 24 hours of Dev B's R4 allocation
- Dependencies: Child Issue 7
- Labels: `migration`, `release:r4`, `sprint:5`, `area:validation`, `type:feature`, `priority:p1`

### Atomic checklist

- [ ] Add required-field validation to `CreateAccountRequestDto`.
- [ ] Enforce a 10-character account number.
- [ ] Enforce a 4-character PIN.
- [ ] Enforce age range 18-120.
- [ ] Enforce a positive balance.
- [ ] Validate account type, status, and privilege.
- [ ] Validate close-account account number.
- [ ] Validate transaction account number and positive amount.
- [ ] Create separate `DepositRequestDto`, `WithdrawRequestDto`, and `TransferRequestDto` models.
- [ ] Add PIN and from/to account validation to withdrawal and transfer requests.

### Acceptance criteria

- [ ] Invalid requests fail with field-level HTTP 400 validation errors.
- [ ] Valid R2 payloads continue to work.
- [ ] Validation rules are documented in `VALIDATION-RULES.md`.

## Child Issue 9 - R4 Response Contract and Validation Pipeline

### Title

`[R4][Sprint 5] Standardize API responses and automatic model validation`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev B for controller updates; Dev A for integration review
- Estimate: 24 hours of Dev C's R4 allocation
- Dependencies: Child Issue 8
- Labels: `migration`, `release:r4`, `sprint:5`, `area:api`, `area:validation`, `type:refactor`, `priority:p1`

### Atomic checklist

- [ ] Create `ApiResponse<T>` with `Success`, `Data`, and `Errors`.
- [ ] Update account and transaction controllers to use the wrapper consistently.
- [ ] Verify `[ApiController]` automatic 400 behavior.
- [ ] Customize `InvalidModelStateResponseFactory` for the agreed error format.
- [ ] Test missing fields, invalid lengths, age limits, negative balances, invalid amounts, and invalid PIN shape.
- [ ] Re-run all valid R2 requests.
- [ ] Update API examples and validation documentation.

### Acceptance criteria

- [ ] Every migrated endpoint has a consistent successful response shape.
- [ ] Every invalid request produces a predictable 400 response with useful field errors.
- [ ] All prior release tests pass for valid requests.
- [ ] R4 release verification is complete and `CHANGELOG.md` is updated.

## Child Issue 10 - R5 Exception Handling and ProblemDetails

### Title

`[R5][Sprint 6] Add global exception mapping, ProblemDetails, and structured logging`

### Assignee and estimate

- Primary: Dev A
- Supporting: Dev C for middleware pipeline; Dev B for API error tests
- Estimate: 24 hours of Dev A's R5 allocation
- Dependencies: Child Issue 9
- Labels: `migration`, `release:r5`, `sprint:6`, `area:middleware`, `area:testing`, `type:feature`, `priority:p1`

### Atomic checklist

- [ ] Create `GlobalExceptionHandler` implementing `IExceptionHandler`.
- [ ] Map `AccountException` to 400.
- [ ] Map `InactiveAccountException` to 409.
- [ ] Map `InsufficientBalanceException` and `MinimumBalanceViolationException` to 422.
- [ ] Map `InvalidAmountException` to 400.
- [ ] Map `InvalidPinException` to 401.
- [ ] Map `InvalidOperationException` to 409.
- [ ] Map unknown exceptions to a generic 500 response without a stack trace.
- [ ] Return `ProblemDetails` for handled errors.
- [ ] Add structured logging with correlation-friendly context.

### Acceptance criteria

- [ ] Every mapped domain exception returns the documented status and response shape.
- [ ] Unhandled errors do not expose internal stack traces.
- [ ] `ERROR-CODES.md` documents exception-to-status mappings.
- [ ] Negative and regression tests pass.

## Child Issue 11 - R5 CORS, Versioning, and Middleware Release

### Title

`[R5][Sprint 6] Enable CORS, URL-based API v1 versioning, and release verification`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev A for middleware ordering; Dev B for Swagger and endpoint verification
- Estimate: 48 hours of shared Sprint 6 capacity for CORS, versioning, integration tests, documentation, and release work
- Dependencies: Child Issue 10
- Labels: `migration`, `release:r5`, `sprint:6`, `area:middleware`, `area:api`, `type:release`, `priority:p1`

### Atomic checklist

- [ ] Add `Asp.Versioning.Http` and `Asp.Versioning.ApiExplorer` packages.
- [ ] Configure URL-based API versioning with v1 as the initial version.
- [ ] Add `[ApiVersion("1.0")]` to all controllers.
- [ ] Update routes to `api/v{version:apiVersion}/[controller]`.
- [ ] Configure Swagger to show version information.
- [ ] Add a named CORS policy with development and production origin settings.
- [ ] Configure allowed methods and headers.
- [ ] Place CORS, exception handling, authentication placeholders, and routing in the correct pipeline order.
- [ ] Test CORS preflight and cross-origin calls.
- [ ] Test versioned and default-version behavior.
- [ ] Update `ERROR-CODES.md`, endpoint docs, and changelog.

### Acceptance criteria

- [ ] `/api/v1/` endpoints work for all current API operations.
- [ ] CORS headers are present for allowed origins and denied for disallowed production origins.
- [ ] ProblemDetails is returned for documented failure cases.
- [ ] R5 release checks pass and the release is tagged.

## Child Issue 12 - R6 JWT Authentication

### Title

`[R6][Sprint 7] Implement JWT login, token validation, and authentication middleware`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev A for configuration/security review; Dev B for AuthController and API tests
- Estimate: 24 hours of Dev C's R6 allocation
- Dependencies: Child Issue 11
- Labels: `migration`, `release:r6`, `sprint:7`, `area:security`, `type:feature`, `risk-critical`, `priority:p0`

### Atomic checklist

- [ ] Add JWT bearer and token-generation packages.
- [ ] Add `Jwt:Key`, `Jwt:Issuer`, and `Jwt:Audience` configuration placeholders without committing production secrets.
- [ ] Configure token validation parameters and authentication middleware.
- [ ] Create `IAuthService` and `AuthService`.
- [ ] Create `LoginRequestDto` and `LoginResponseDto`.
- [ ] Create `POST /api/v1/auth/login`.
- [ ] Implement the roadmap's temporary hardcoded user store for this phase only.
- [ ] Include role claims and configure token expiration.
- [ ] Mark auth and health endpoints as anonymous.

### Acceptance criteria

- [ ] Valid credentials return a JWT.
- [ ] Invalid credentials return 401.
- [ ] Expired and tampered tokens are rejected with 401.
- [ ] Secrets are supplied through environment variables or an approved local secret mechanism.
- [ ] `AUTH-GUIDE.md` explains the temporary user store and its production limitation.

## Child Issue 13 - R6 Role Authorization and Security Release

### Title

`[R6][Sprint 7] Apply role-based authorization and verify 401/403 behavior`

### Assignee and estimate

- Primary: Dev A for policy and endpoint matrix
- Supporting: Dev B for endpoint attributes; Dev C for security testing and Swagger
- Estimate: 48 hours of shared Sprint 7 capacity for authorization, testing, documentation, and release
- Dependencies: Child Issue 12 and business confirmation of the role matrix
- Labels: `migration`, `release:r6`, `sprint:7`, `area:security`, `area:testing`, `type:release`, `needs-business-decision`, `priority:p0`

### Assumption requiring confirmation

The roadmap proposes User, Manager, Teller, and Admin roles. It assumes users can view their own account, Teller can view all accounts and create accounts, Manager can close accounts, and Admin can do everything. Business stakeholders must confirm this before final authorization attributes are merged.

### Atomic checklist

- [ ] Confirm and record the role-permission matrix.
- [ ] Add `[Authorize]` to protected endpoints.
- [ ] Add Admin-only and Teller/Admin policies where required.
- [ ] Add Manager/Admin protection to account-close operations.
- [ ] Ensure health and login remain anonymous.
- [ ] Add JWT support to Swagger's Authorize button.
- [ ] Test authorized access for each role.
- [ ] Test no-token 401 responses.
- [ ] Test insufficient-role 403 responses.
- [ ] Test expired and tampered tokens.
- [ ] Update `AUTH-GUIDE.md`, changelog, and release notes.

### Acceptance criteria

- [ ] Every protected endpoint has a documented authorization rule.
- [ ] R6 security test cases pass.
- [ ] No role can perform an operation outside the confirmed matrix.
- [ ] R6 security and release gates pass before tagging.

## Child Issue 14 - R7 Docker Image and Runtime Configuration

### Title

`[R7][Sprint 8] Containerize GDB.Api with a multi-stage Docker build`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev A for connection configuration; Dev B for API smoke testing
- Estimate: 24 hours of Dev C's R7 allocation
- Dependencies: Child Issue 13
- Labels: `migration`, `release:r7`, `sprint:8`, `area:docker`, `type:feature`, `priority:p1`

### Atomic checklist

- [ ] Create a multi-stage `Dockerfile` with a .NET 8 SDK build stage.
- [ ] Add the ASP.NET Core 8 runtime stage.
- [ ] Expose port 8080 and configure the application to listen on it.
- [ ] Create `.dockerignore`.
- [ ] Build and tag the image locally.
- [ ] Configure connection-string and other runtime environment variables.
- [ ] Create `docker-compose.yml` if needed for API plus database development.
- [ ] Document host-to-container SQL Server networking using the verified host address.
- [ ] Ensure the health endpoint is available to Docker health checks.

### Acceptance criteria

- [ ] `docker build -t gdb-api .` succeeds.
- [ ] `docker run` starts a healthy API container.
- [ ] Host requests can reach the API on port 8080.
- [ ] No secrets are baked into the image.
- [ ] `DOCKER-GUIDE.md` documents build, run, configuration, networking, and troubleshooting.

## Child Issue 15 - R7 Container Verification and Release

### Title

`[R7][Sprint 8] Verify containerized API, database access, restart behavior, and release`

### Assignee and estimate

- Primary: Dev A for release verification
- Supporting: Dev B for full API regression; Dev C for logs and networking
- Estimate: 48 hours of shared Sprint 8 capacity for container testing, fixes, documentation, and release
- Dependencies: Child Issue 14
- Labels: `migration`, `release:r7`, `sprint:8`, `area:docker`, `area:testing`, `type:release`, `priority:p1`

### Atomic checklist

- [ ] Verify `GET /api/v1/health` from the host.
- [ ] Run the full API test suite against the container.
- [ ] Verify account and transaction data persists in SQL Server.
- [ ] Analyze container logs for startup, configuration, and connection failures.
- [ ] Restart the container and verify API recovery.
- [ ] Verify Docker health status.
- [ ] Fix networking issues involving `host.docker.internal` or the documented host IP.
- [ ] Update release notes and Docker documentation.
- [ ] Complete R7 release verification, merge, tag, and demo.

### Acceptance criteria

- [ ] R7 smoke, integration, database, restart, and health checks pass.
- [ ] Container behavior matches the non-containerized API.
- [ ] R7 release gate is complete.

## Child Issue 16 - R8 Typed HTTP Client Infrastructure

### Title

`[R8][Sprint 9] Add typed HttpClient infrastructure and console authentication support`

### Assignee and estimate

- Primary: Dev A
- Supporting: Dev C for configuration and token handling; Dev B for client tests
- Estimate: 24 hours of Dev A's Sprint 9 allocation
- Dependencies: Child Issue 15
- Labels: `migration`, `release:r8`, `sprint:9`, `area:console-client`, `area:api`, `type:feature`, `priority:p0`

### Atomic checklist

- [ ] Add `Microsoft.Extensions.Http` and `System.Net.Http.Json` to the console project.
- [ ] Create `IGdbApiClient`.
- [ ] Create a typed `GdbApiClient`.
- [ ] Configure the API base URL through `ApiSettings:BaseUrl`.
- [ ] Add JSON serialization and response-envelope helpers.
- [ ] Implement login HTTP call.
- [ ] Store the JWT token in memory only for the current console session.
- [ ] Attach the token to subsequent requests.
- [ ] Configure a bounded HTTP timeout.
- [ ] Establish consistent HTTP status and API-error handling.

### Acceptance criteria

- [ ] The console can connect to the R7 API using the configured base URL.
- [ ] Login returns and stores a token.
- [ ] Authenticated requests include the bearer token.
- [ ] Timeout and non-success responses are available to the UI as structured client errors.

## Child Issue 17 - R8 Account and Transaction Client Methods

### Title

`[R8][Sprint 9] Implement all typed API client methods for banking operations`

### Assignee and estimate

- Primary: Dev B
- Supporting: Dev A for client contract review; Dev C for integration support
- Estimate: 24 hours of Dev B's Sprint 9 allocation
- Dependencies: Child Issue 16
- Labels: `migration`, `release:r8`, `sprint:9`, `area:console-client`, `type:feature`, `priority:p0`

### Atomic checklist

- [ ] Implement `GetAccountAsync` for `GET /api/v1/accounts/{accountNumber}`.
- [ ] Implement `GetAllAccountsAsync` for `GET /api/v1/accounts`.
- [ ] Implement `GetBalanceAsync` for `GET /api/v1/accounts/{accountNumber}/balance`.
- [ ] Implement `CreateAccountAsync` for `POST /api/v1/accounts`.
- [ ] Implement `CloseAccountAsync` for `DELETE /api/v1/accounts/{accountNumber}`.
- [ ] Implement `DepositAsync` for `POST /api/v1/transactions/deposit`.
- [ ] Implement `WithdrawAsync` for `POST /api/v1/transactions/withdraw`.
- [ ] Implement `TransferAsync` for `POST /api/v1/transactions/transfer`.
- [ ] Implement `GetRecentTransactionsAsync`.
- [ ] Add unit tests for request routes, serialization, success envelopes, and error envelopes.

### Acceptance criteria

- [ ] Every client method targets the documented versioned route.
- [ ] DTOs serialize to the expected API payloads.
- [ ] Success and API error responses are converted into usable client results.
- [ ] Client unit tests pass without requiring a live database.

## Child Issue 18 - R8 Console UI Replacement

### Title

`[R8][Sprint 10] Refactor console UI and startup to use only the HTTP client`

### Assignee and estimate

- Primary: Dev C
- Supporting: Dev A for composition-root review; Dev B for user-flow testing
- Estimate: 24 hours of Dev C's Sprint 10 allocation
- Dependencies: Child Issues 16 and 17
- Labels: `migration`, `release:r8`, `sprint:10`, `area:console-client`, `type:refactor`, `priority:p0`

### Atomic checklist

- [ ] Replace `AccountController` construction in `Home.cs` with `IGdbApiClient` calls.
- [ ] Replace `TransactionController` construction in `Home.cs` with `IGdbApiClient` calls.
- [ ] Add login at application start.
- [ ] Display clear messages for 4xx API errors and validation failures.
- [ ] Display a user-friendly service-unavailable message when the API is down.
- [ ] Handle timeout without freezing or crashing the console.
- [ ] Remove direct references from the console to service and repository layers.
- [ ] Update `Program.cs` to configure `IHttpClientFactory` and the typed client.
- [ ] Preserve the existing menu and banking workflows unless a route or auth change requires a documented adjustment.

### Acceptance criteria

- [ ] The console has no direct application-service or repository construction.
- [ ] Login is required before protected operations.
- [ ] All menu actions call the API client.
- [ ] API errors, timeouts, and network failures are understandable to the user.

## Child Issue 19 - R8 End-to-End Verification and Final Release

### Title

`[R8][Sprint 10] Verify console-to-API-to-database flows and complete migration release`

### Assignee and estimate

- Primary: Dev C for E2E and final demo ownership
- Supporting: Dev A for integration/regression; Dev B for functional and release documentation
- Estimate: 48 hours of shared Sprint 10 capacity for E2E testing, fixes, documentation, and final release
- Dependencies: Child Issue 18
- Labels: `migration`, `release:r8`, `sprint:10`, `area:testing`, `area:documentation`, `type:release`, `priority:p0`

### Atomic checklist

- [ ] Test console login and in-memory token storage.
- [ ] Test create account through console -> API -> database.
- [ ] Test view account, view all accounts, and view balance.
- [ ] Test deposit and verify the balance change.
- [ ] Test withdrawal and verify PIN and minimum-balance rules.
- [ ] Test transfer and verify both account balances.
- [ ] Test recent transaction history.
- [ ] Test close account and verify the Closed status.
- [ ] Test invalid credentials, invalid account, insufficient balance, and other documented failures.
- [ ] Stop the API and verify the console shows a service-unavailable message without crashing.
- [ ] Simulate timeout and verify the retry or timeout message.
- [ ] Run the complete regression suite for R1-R7.
- [ ] Update `CONSOLE-CLIENT-GUIDE.md`, deployment documentation, `CHANGELOG.md`, and final release notes.
- [ ] Prepare the stakeholder demo and migration completion summary.

### Acceptance criteria

- [ ] All 13 R8 test scenarios pass, including the API-down and timeout cases.
- [ ] All nine business operations work through the console HTTP path.
- [ ] No direct backend access remains in the console.
- [ ] All previous release tests pass.
- [ ] R8 is merged to `release/r8`, tagged, and demonstrated to stakeholders.

---

## Sprint Execution and Coordination

Each sprint follows the roadmap ceremony schedule:

- Day 1, first hour: one-hour sprint planning with all developers.
- Every day: 15-minute standup covering progress, dependencies, risks, and blocked hours.
- Day 4, end of day: one-hour sprint review/demo.
- Day 4, after the review: 30-minute retrospective.

Use GitHub issue checklists for atomic progress and pull requests for implementation. Every PR should link its child issue, identify tests run, and include screenshots or request/response examples when the work changes Swagger, the console UI, Docker, or documentation.

### Daily tracking fields

Track these fields on each child issue or in the sprint board:

- Planned hours
- Completed hours
- Remaining hours
- Blocked hours and blocking issue
- Test status
- Review status
- Release-gate status

### Recommended issue dependency chain

```text
R1 foundation
  -> R2 account + transaction API
  -> R2 stabilization
  -> R3 repository DI
  -> R3 service DI and factory removal
  -> R3 verification
  -> R4 validation and response contract
  -> R5 exception handling + CORS + versioning
  -> R6 JWT + authorization
  -> R7 Docker + container verification
  -> R8 HTTP client + console UI
  -> R8 end-to-end verification and final release
```

Parallel work is encouraged inside a release after the shared contract is agreed. For example, account and transaction controllers can proceed in parallel in R2, and request validation plus response-envelope work can proceed in parallel in R4. Release tagging remains sequential because each release is a verification baseline for the next one.

## Program Definition of Done

A child issue is Done when:

- [ ] Code is complete and builds without errors or warnings.
- [ ] New behavior has tests and existing tests still pass.
- [ ] Acceptance criteria are manually verified where applicable.
- [ ] At least one other developer approves the PR.
- [ ] Documentation is updated for user-visible or operational changes.
- [ ] The change is merged to `develop` through a PR.
- [ ] No known regression is left unexplained.

A release is Done when:

- [ ] All child issues for the release are Done or explicitly deferred.
- [ ] The full regression suite passes.
- [ ] Release-specific acceptance criteria and quality gates pass.
- [ ] API and operational documentation are current.
- [ ] `CHANGELOG.md` is updated.
- [ ] `develop` is merged to `release/rN`.
- [ ] The release is tagged in Git.
- [ ] A stakeholder demo is completed.

## Post-Migration Follow-Ups

Create separate backlog issues after R8 for work intentionally excluded from this migration:

- Replace the temporary hardcoded user store with a persistent identity provider.
- Hash and securely manage PINs instead of storing plaintext values.
- Evaluate fully asynchronous repository methods instead of short-term compatibility handling.
- Add production CI/CD and deployment automation if required.
- Review concurrent access behavior of the static InMemory `DataSet` store.
- Replace any remaining temporary compatibility code after production traffic is observed.
