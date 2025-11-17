## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
