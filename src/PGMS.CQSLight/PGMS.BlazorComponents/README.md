## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
