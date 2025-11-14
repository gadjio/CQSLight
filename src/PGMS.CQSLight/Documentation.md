# PGMS.CQSLight - NuGet Packages Documentation

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0

## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

---

## PGMS.CQSLight

**Core CQS Light framework with DirectBus implementation**

This is the main package that provides the core infrastructure for implementing the Command Query Separation pattern in your .NET applications.

### Key Features

- **Command Handling**: Base classes for command handlers (`BaseCommandHandler`, `BaseCommandHandlerAsync`) that support validation, event publishing, and transaction management
- **Query Processing**: Query processor (`IQueryProcessor`) with support for synchronous and asynchronous query handling, including caching capabilities
- **DirectBus**: Lightweight message bus (`IBus`) for publishing events and sending commands with automatic handler resolution using Autofac
- **Event Handling**: Event handler infrastructure (`IHandleEvent`) for processing domain events within transactions
- **Security**: Built-in role-based authorization support for commands with `AllowedRoles` attributes
- **Validation**: Integrated validation support using `ValidationResult` with automatic validation execution
- **Helpers**: `CommandHelper` and `QueryHelper` utilities to simplify command and query execution

### Core Interfaces

- `ICommand` / `IQuery<TResult>`: Marker interfaces for commands and queries
- `IHandleCommand<T>` / `IHandleQuery<TQuery, TResult>`: Handler interfaces
- `IBus`: Message bus for event publishing and command dispatching
- `IQueryProcessor`: Query execution with optional caching

This package is the foundation for all other PGMS.CQSLight packages and is required for implementing the CQS pattern in your application.

---

## PGMS.Data

**Data layer abstractions for repository pattern and unit of work**

This package contains all abstractions over a specific data provider, providing a clean separation between your business logic and data access layer.

### Key Features

- **Repository Pattern**: `IEntityRepository` and `IScopedEntityRepository` interfaces for data access operations
- **Unit of Work**: `IUnitOfWork` and `IUnitOfWorkProvider` for managing database transactions
- **Data Service**: `IDataService` interface for generating unique IDs using HiLo algorithm
- **Provider Agnostic**: Abstract interfaces that can be implemented by any data provider (EF Core, Dapper, etc.)

### Core Interfaces

- `IEntityRepository`: Main repository interface with CRUD operations, raw SQL support, and query building
- `IScopedEntityRepository`: Scoped operations within a unit of work context
- `IUnitOfWork`: Transaction management with commit/rollback support
- `IDataService`: ID generation service

This library helps to decouple the data access logic from the rest of the application, making it easy to use and test. It provides a consistent API regardless of the underlying data provider.

---

## PGMS.DataProvider.EFCore

**Entity Framework Core implementation of the repository pattern**

This package provides a complete implementation of the PGMS.Data abstractions using Entity Framework Core, enabling you to work with SQL Server databases.

### Key Features

- **EF Core Repository**: `BaseEntityRepository<TDbContext>` implementation with full CRUD operations
- **Unit of Work**: `UnitOfWork<T>` implementation with transaction support
- **Context Factory**: `ContextFactory<T>` for creating DbContext instances with connection string management
- **Data Service**: `DataService<TDbContext>` with HiLo ID generation using database sequences
- **Lazy Loading**: Support for lazy loading with `LazyLoadAttribute` on entities and properties
- **Raw SQL**: Execute raw SQL queries and commands with parameter support
- **Bulk Operations**: Bulk insert support for better performance
- **Query Builder**: LINQ-based query building with filtering, ordering, and pagination

### Installation Helper

Use `DataProviderLayerInstaller.RegisterContext<TDbContext>()` to register all services with Autofac dependency injection.

This library provides a robust, production-ready implementation of the repository pattern over EF Core, handling connection management, transactions, and optimized queries.

---

## PGMS.BlazorComponents

**Blazor components to facilitate the use of commands and queries in the UI (E2E DDD)**

This package focuses on components to facilitate the use of commands and queries in the UI, enabling true end-to-end Domain-Driven Design.

### End-to-end Domain-Driven Design (DDD)

End-to-end DDD involves applying the principles of DDD across the entire software development process, from requirements gathering to deployment. This means that the focus is not just on the code, but on the entire system, including the user interface, the database, and the infrastructure.

The end-to-end DDD approach also emphasizes the use of ubiquitous language, a common vocabulary used by both the development team and the domain experts to ensure that everyone is speaking the same language and referring to the same concepts. This helps to avoid misunderstandings and ensures that the software meets the needs of the business.

Overall, end-to-end DDD aims to create software systems that are not only technically sound but also closely aligned with the business domain they serve, which can lead to more successful and effective software solutions.

### Key Features

- **Base Action Components**: `BaseCqsActionComponent` for creating command-based UI actions
- **Modal Components**: `ActionModalComponent<TActionItem>` for displaying commands in modal dialogs
- **Side Panel Components**: `ActionSidePanelComponent<TActionItem>` for side panel command execution
- **Form Components**: `ActionFormCommandComponent<TActionItem, TCommand>` for command forms with validation
- **Security Integration**: `BaseSecureComponent` with role-based access control and session management
- **Error Handling**: Built-in error handling with `IErrorHandlerService` integration
- **Validation Display**: Automatic display of command validation results in the UI
- **Blazorise Integration**: Built on Blazorise component library for rich UI components

### What's New

Base classes for Blazor CQSLight integration. Includes modal and side panel host for Command Action Components, enabling seamless integration of CQS pattern in Blazor applications.

---

## PGMS.CQSLight.UnitTestUtilities

**Unit test utilities for PGMS.CQSLight - Fake implementations and in-memory repositories**

This package provides essential testing utilities to facilitate unit testing of applications built with PGMS.CQSLight.

### Key Features

- **Fake Implementations**: 
  - `FakeBus`: In-memory bus that tracks published events and commands for verification
  - `FakeQueryProcessor`: Stub query processor for testing
  - `FakeDataService`: Simple ID generator for testing
  - `FakeUnitOfWork` / `FakeTransaction`: No-op implementations for testing without database
  
- **In-Memory Repository**: `InMemoryEntityRepository` - fully functional in-memory implementation of `IEntityRepository`
  - Supports all CRUD operations
  - LINQ query support with filtering, ordering, and pagination
  - No database required - perfect for fast unit tests
  
- **In-Memory Reporting Repository**: `InMemoryReportingRepository<TContext>` - enhanced version with navigation property resolution
  - Automatically resolves entity relationships based on EF Core model metadata
  - Supports both one-to-many and many-to-one relationships
  - Perfect for testing queries with complex object graphs

### Usage

These utilities enable fast, isolated unit tests without the need for a database connection. Use `FakeBus` to verify that commands publish the expected events, and `InMemoryEntityRepository` to test query handlers and event handlers with in-memory data.

---

## PGMS.ScenarioTesting

**Scenario testing framework for integration and end-to-end testing**

This package provides a structured approach to writing scenario-based integration tests using the Given-When-Then pattern with NUnit.

### Key Features

- **Scenario Test Framework**: `BaseScenarioTest` base class for organizing tests with Given/When/Then structure
- **Test Actions**:
  - `ScenarioTestCommand<T>`: Execute commands in test scenarios
  - `ScenarioTestQuery<T, TResult>`: Execute queries with result validation
  - `ScenarioTestCommandWithValidationFailure<T>`: Test expected validation failures
  - `ScenarioTestDataValidation`: Validate data state using repository
  - `ScenarioTestDataManipulation`: Manipulate test data directly
  
- **Test Helpers**:
  - `IScenarioTestHelper`: Abstraction for test execution
  - `ScenarioTestHelperInMemoryImplementation<TContext>`: In-memory testing with Autofac container
  - `ScenarioTestHelperIntegratedImplementation<TContext>`: Integration testing against real database or API
  
- **API Testing**: `ICqsApiHelper` for testing CQS operations through HTTP APIs with RestSharp

### Usage

Scenario tests provide a clear, readable structure for integration tests:
1. **Givens**: Set up initial state (run once before all tests)
2. **Actions**: Execute commands and queries with validation
3. **Assertions**: Verify expected outcomes

This approach makes tests self-documenting and easier to maintain, while supporting both in-memory and integrated testing strategies.

---

## License

All packages are licensed under the MIT License.

## Repository

[https://github.com/gadjio/CQSLight](https://github.com/gadjio/CQSLight)

