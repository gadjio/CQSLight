## Introduction

PGMS.CQSLight is a small, open-source library that provides a simple and easy-to-use interface for implementing the Command Query Separation (CQS) pattern in .NET Core applications. The library is designed to be lightweight, flexible, and easy to test.

The CQS pattern is a software design pattern that separates the responsibility of executing a command from the responsibility of querying data. The pattern is based on the idea that a method or function should either change the state of the system or retrieve data, but not both.

In PGMS.CQSLight, commands and queries are separated into different interfaces, and each command and query is handled by a specific handler. The library also provides a simple interface for dispatching commands and handling queries.

PGMS.CQSLight is built with SOLID principles, which makes the code maintainable, scalable, and testable. It's an easy to use library, and it's a good fit for small and medium-sized projects that need to implement the CQS pattern.

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

## Getting Started

Have a look at our sample app based on PGMS.CQSLight: [GitHub Repository](https://github.com/gadjio/CQSLight)

Version 5.X => DotNet 10.0  
Version 4.X => DotNet 8.0  
Version 3.X => DotNet 6.0  
Version 2.X => DotNet 5.0
