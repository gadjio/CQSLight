## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
