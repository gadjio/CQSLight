## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
