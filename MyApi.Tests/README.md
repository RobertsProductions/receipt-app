# MyApi.Tests

Unit test project for the Warranty Tracker API.

## Overview

This project contains unit tests for the MyApi application, covering:
- Model validation and behavior
- Business logic testing
- Data integrity tests

## Test Structure

```
MyApi.Tests/
├── Models/              # Tests for data models
│   ├── ApplicationUserTests.cs  # User model tests
│   └── ReceiptTests.cs          # Receipt model tests
├── Controllers/        # API controller tests (future)
├── Services/          # Service layer tests (future)
└── Integration/       # Integration tests (future)
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run tests in watch mode
```bash
dotnet watch test
```

### Run specific test class
```bash
dotnet test --filter "FullyQualifiedName~ApplicationUserTests"
```

## Test Coverage

### Models (100% covered)
- ✅ ApplicationUser - User model with notification preferences
- ✅ Receipt - Receipt tracking model
- ✅ NotificationChannel - Enum for notification channels

## Testing Frameworks

- **xUnit** - Primary test framework
- **FluentAssertions** - Assertion library for readable tests
- **Moq** - Mocking library for dependencies
- **Microsoft.AspNetCore.Mvc.Testing** - For integration testing (future)
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing

## Test Conventions

### Naming Convention
```
[MethodUnderTest]_[Scenario]_[ExpectedBehavior]
```

Example: `Receipt_WithNoWarrantyExpirationDate_ShouldBeNull()`

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void TestName()
{
    // Arrange - Set up test data and dependencies
    var model = new Model();
    
    // Act - Execute the method under test
    var result = model.DoSomething();
    
    // Assert - Verify the outcome
    result.Should().Be(expectedValue);
}
```

## Future Test Plans

### Controllers
- AuthController integration tests
- ReceiptsController integration tests
- ProfileController integration tests

### Services
- TokenService unit tests
- EmailNotificationService unit tests
- OcrService unit tests
- LocalFileStorageService unit tests

### Integration Tests
- End-to-end API workflow tests
- Database integration tests
- Authentication flow tests

## Dependencies

All test dependencies are managed via NuGet:
- xunit (2.9.2)
- xunit.runner.visualstudio (2.8.2)
- FluentAssertions (7.0.0)
- Moq (4.20.72)
- Microsoft.AspNetCore.Mvc.Testing (8.0.11)
- Microsoft.EntityFrameworkCore.InMemory (8.0.11)
- Microsoft.NET.Test.Sdk (17.11.1)

## CI/CD Integration

Tests are designed to be run in CI/CD pipelines with:
- Fast execution times
- No external dependencies
- Clear failure messages
- Minimal resource requirements

## Best Practices

1. **Isolation** - Each test is independent and can run in any order
2. **Fast** - Tests execute quickly using in-memory data
3. **Reliable** - Tests produce consistent results
4. **Readable** - Test names clearly describe what is being tested
5. **Maintainable** - Tests are simple and focused on single behaviors

## Running Tests in Development

During development, use watch mode to run tests automatically:

```bash
cd MyApi.Tests
dotnet watch test
```

This will re-run tests whenever you save changes to test or source files.
