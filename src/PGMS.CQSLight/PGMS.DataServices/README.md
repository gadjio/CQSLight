## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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
## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
