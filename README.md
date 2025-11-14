# CQSLight

[![Package Version](https://img.shields.io/nuget/v/PGMS.CQSLight.svg)](https://www.nuget.org/packages/PGMS.CQSLight)
[![NuGet Downloads](https://img.shields.io/nuget/dt/PGMS.CQSLight.svg)](https://www.nuget.org/packages/PGMS.CQSLight)
[![License](https://img.shields.io/github/license/gadjio/CQSLight.svg)](https://github.com/gadjio/CQSLight/blob/master/LICENSE)

Version 5.X => DotNet 10.0

Version 4.X => DotNet 8.0

Version 3.X => DotNet 6.0

Version 2.X => DotNet 5.0


# Introduction 
PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable.
It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

It's important to note that PGMS.CQSLight is not a complete solution, it's a library that helps to implement the CQS pattern and it's up to you to use it properly and to implement other patterns and practices to make your application robust and secure.

PGMS.Data contains all abstractions over a specific Data Provider

PGMS.DataProvider is an implementation over EFCore
This library helps to decouple the data access logic from the rest of the application, and it's easy to use and test.


# Getting Started
Have a look at our sample app based on PGMS.CQSLight


# What's new
Version 3.1 -> async function for entityrepository, bus, EventHandlers, CommandHandlers and QueryHandlers
Version 3.1.0.5 -> EntityRepository - FindAllOperation (return all entries - not fetchsize / offset)

Version 3.2 -> Role Validation
				Breaking changes :
					- IBus.Send -> add ContextInfo
					- IHandleCommand<T>.Execute -> add ContextInfo
					- ICommand -> Remove ByUsername, ByUserId
					- BaseCommand -> Remove ByUsername, ByUserId
