# Design patterns used in this codebase: Command and Bridge

This document explains the Command pattern and the Bridge-like decoupling used in the repository. It is written for a junior developer and shows the flow, files to look at, why the patterns are used and how to extend them.

---

## 1) Command pattern — concept (short)

- Intent: Encapsulate a request (operation) as an object so you can parameterize callers with different requests, queue or log requests, and support undoable operations.
- Key parts: Command interface, concrete commands, invoker (caller), and optionally a factory to create commands.

Why it is used here: transaction operations (Deposit, Withdraw, Transfer) are modelled as commands. Each command implements the same interface and encapsulates the logic needed to execute that transaction.

Files to inspect (implementation in this repo):
- Interface: Application/Services/Contracts/ITransactionCommand.cs
- Concrete commands:
  - Application/Services/Implementations/DepositTransactionCommand.cs
  - Application/Services/Implementations/WithdrawTransactionCommand.cs
  - Application/Services/Implementations/TransferTransactionCommand.cs
- Factory: Application/Services/TransactionCommandFactory.cs
- Invoker/service: Application/Services/Implementations/TransactionService.cs

Flow (simple step-by-step):
1. A controller or caller prepares a TransactionDto describing the request (amount, account numbers, pin). See Application/Dtos/*.cs for the DTO.
2. TransactionService.ProcessTransactionAsync<TResponse>(transactionDto, transactionType) is called (this is the invoker).
3. TransactionService asks TransactionCommandFactory.Create<TResponse>(transactionType) for a command instance.
4. The factory returns a concrete ITransactionCommand<TResponse> (Deposit/Withdraw/Transfer) with required repositories injected.
5. TransactionService invokes command.ExecuteAsync(transactionDto).
6. The concrete command performs domain operations (calls domain model methods like account.Withdraw/Deposit), updates repositories and returns a response DTO (e.g., WithdrawResponseDto).

Why this is helpful (for a junior dev):
- Keeps transaction behavior isolated per operation (easy to read and test).
- Adding a new transaction type means adding a new command class and registering it in the factory — minimal change to existing code.
- The invoker (TransactionService) does not need to know details of each transaction type.

How to add a new transaction (example):
1. Create NewTransactionCommand : ITransactionCommand<NewResponseDto> and implement ExecuteAsync.
2. Add mapping in TransactionCommandFactory.Create to return the new command for a new TransactionType enum value.
3. Add any DTOs needed in Application/Dtos.

Notes and caveats:
- TransactionCommandFactory currently creates repositories using AccountRepositoryFactory.Create("DB") and TransactionRepositoryFactory.Create("DB"). If you need to change repository selection, update the factory code.

---

## 2) Bridge / decoupling between abstraction and implementation — concept (short)

- Intent (Bridge-like): separate an abstraction from its implementation so they can vary independently. In practice this repo uses interfaces + concrete implementations and provider factories to decouple the high-level services from the concrete storage and DB provider details.
- Key parts here: repository interfaces (abstractions) and multiple concrete repository implementations (implementations), plus a provider/connection layer that isolates DB-provider specifics.

Files to inspect (implementation in this repo):
- Repository interfaces:
  - Infrastructure/Repositories/Contracts/IAccountRepository.cs
  - Infrastructure/Repositories/Contracts/ITransactionRepository.cs
- Concrete repository implementations:
  - Infrastructure/Repositories/Implementations/AccountRepositoryDB.cs
  - Infrastructure/Repositories/Implementations/AccountRepositoryInMemory.cs
  - Infrastructure/Repositories/Implementations/TransactionRepositoryDB.cs
  - Infrastructure/Repositories/Implementations/TransactionRepositoryInMemory.cs
- Factories that select implementations:
  - Infrastructure/Repositories/AccountRepositoryFactory.cs
  - Infrastructure/Repositories/TransactionRepositoryFactory.cs
- DB provider / connection abstraction:
  - Infrastructure/Repositories/DataBaseProviderRegistration.cs
  - Infrastructure/Repositories/DataBaseConnectionManager.cs

Flow (how the pieces fit together at runtime):
1. High-level services (e.g., DepositTransactionCommand) depend on IAccountRepository and ITransactionRepository (they program to the interface).
2. A factory (TransactionCommandFactory or AccountRepositoryFactory) chooses a concrete implementation and returns an instance (DB or InMemory).
3. If the DB implementation is chosen, AccountRepositoryDB uses DataBaseConnectionManager.GetConnection() to obtain a DbConnection created from a provider-specific DbProviderFactory.
4. DataBaseProviderRegistration registers the provider factory if necessary (this lets the same repository code work with different database providers by changing configuration).

Why this is helpful (for a junior dev):
- The service logic (transaction handling) does not depend on how accounts/transactions are stored. You can switch implementations (e.g., replace DB storage with an in-memory store for tests) with minimal changes.
- The DB provider details (SqlClient, Oracle, etc.) are isolated in DataBaseConnectionManager and configured via app settings; repository code works with the generic DbConnection API.

How to add a new storage implementation or provider:
- In-memory storage: Add a new class implementing IAccountRepository or ITransactionRepository and register it in the corresponding factory (AccountRepositoryFactory or TransactionRepositoryFactory).
- New DB provider: supply a provider factory type in configuration and ensure DataBaseProviderRegistration.Register() is called at startup (the code uses DbProviderFactories and DbProviderFactory to abstract provider details).

Analogy for a junior dev:
- Think of the interface (IAccountRepository) as a remote control (abstraction) and concrete repositories as different TV models (implementations). The rest of the house uses the remote control interface and does not care which TV model is installed. The factory is the installer that gives you the correct TV model.

---

## Quick mapping table (pattern -> concrete elements)

- Command pattern:
  - Abstraction: ITransactionCommand<TResponse> (Application/Services/Contracts)
  - Concrete commands: DepositTransactionCommand, WithdrawTransactionCommand, TransferTransactionCommand (Application/Services/Implementations)
  - Invoker: TransactionService (Application/Services/Implementations/TransactionService.cs)
  - Factory: TransactionCommandFactory (Application/Services/TransactionCommandFactory.cs)

- Bridge / decoupling via interfaces + factories:
  - Abstractions: IAccountRepository, ITransactionRepository (Infrastructure/Repositories/Contracts)
  - Implementations: AccountRepositoryDB, AccountRepositoryInMemory, TransactionRepositoryDB, TransactionRepositoryInMemory (Infrastructure/Repositories/Implementations)
  - Factory selectors: AccountRepositoryFactory, TransactionRepositoryFactory (Infrastructure/Repositories)
  - DB provider abstraction: DataBaseConnectionManager, DataBaseProviderRegistration (Infrastructure/Repositories)

---

## Practical tips for a junior dev (do's and don'ts)

- Do: Add a new command class when you need a new transaction type; keep the domain rules (validation, PIN checks, balance enforcement) inside domain models (Account.* methods).
- Do: Keep repository interface changes minimal. When adding methods, update all implementations and factories.
- Don't: Put DB-specific SQL or connection code inside application service classes. Keep it inside repository implementations (this keeps the bridge/abstraction intact).
- Don't: Have services directly new-up concrete repository implementations throughout the code. Use factories or dependency injection so the choice of implementation is centralized.

---

If you want, I can also:
- Add a small diagram (ASCII or Mermaid) showing the sequence for a Transfer transaction.
- Create a template for adding a new command and repository implementation.
