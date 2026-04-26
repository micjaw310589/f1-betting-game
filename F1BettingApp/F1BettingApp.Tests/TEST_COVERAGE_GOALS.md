# F1 Betting Game - Test Coverage Goals and Strategy

## 🎯 Test Coverage Targets

### Overall Coverage Goals
- **Business Logic Layer**: 80%+ code coverage
- **Critical Paths**: 100% code coverage
- **Domain Entities**: 90%+ code coverage
- **Services**: 85%+ code coverage

### Critical Paths (100% Coverage Required)
- **BettingService.PlaceBetAsync()** - Core betting functionality
- **BettingService.CancelBetAsync()** - Bet cancellation and refund logic
- **BettingService.ProcessRaceResultsAsync()** - Race result processing
- **UserService.UpdateUserPointsAsync()** - Points management
- **RaceService.UpdateRaceStatusAsync()** - Race status updates
- **Transaction management** - All financial operations
- **Domain event publishing** - Event-driven architecture

### Business Logic Layer (80%+ Coverage)
- Service method implementations
- Domain event handlers
- Specification pattern usage
- Unit of work transactions
- Validation logic

### Test Types and Distribution
- **Unit Tests**: 70% of test suite
- **Integration Tests**: 20% of test suite
- **End-to-End Tests**: 10% of test suite

## 🧪 Test Framework Setup

### Technologies Used
- **xUnit**: Primary testing framework
- **Moq**: Mocking framework for dependencies
- **coverlet.collector**: Code coverage reporting
- **Builders Pattern**: Test data generation

### Test Project Structure
```
F1BettingApp.Tests/
├── Builders/              # Test data builders
│   ├── UserBuilder.cs     # User entity builder
│   ├── BetBuilder.cs      # Bet entity builder
│   ├── RaceBuilder.cs     # Race entity builder
│   └── ResultBuilder.cs   # Result entity builder
├── TEST_COVERAGE_GOALS.md # This documentation
├── *.cs                   # Test classes
└── F1BettingApp.Tests.csproj
```

### Test Data Builders
The builder pattern provides a fluent interface for creating test entities with sensible defaults:

```csharp
// Example usage
var user = new UserBuilder()
    .WithId(1)
    .WithUsername("testuser")
    .WithPoints(1000)
    .AsAdmin()
    .Build();

var bet = new BetBuilder()
    .WithUserId(1)
    .WithRaceId(1)
    .WithAmount(100)
    .AsWon()
    .Build();

var race = new RaceBuilder()
    .BuildUpcomingRace();

var results = new ResultBuilder()
    .BuildRaceResults();
```

## 📊 Coverage Reporting

### Configuration
The test project includes `coverlet.collector` for coverage reporting:

```xml
<PackageReference Include="coverlet.collector" Version="6.0.2" />
```

### Generating Coverage Reports
Run tests with coverage using dotnet CLI:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Viewing Reports
- **Visual Studio**: Built-in coverage visualization
- **VS Code**: Use Coverage Gutters extension
- **CI/CD**: Generate HTML/XML reports for artifacts

## 🚀 CI/CD Pipeline Configuration

### GitHub Actions Example
```yaml
name: Run Tests with Coverage

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --no-restore

    - name: Test with Coverage
      run: |
        dotnet test --no-build --verbosity normal \
          --collect:"XPlat Code Coverage" \
          --settings coverlet.runsettings

    - name: Upload Coverage Report
      uses: actions/upload-artifact@v4
      with:
        name: coverage-report
        path: TestResults/**/coverage.cobertura.xml
```

### Azure DevOps Example
```yaml
- task: DotNetCoreCLI@2
  displayName: 'dotnet test'
  inputs:
    command: test
    projects: '**/*Tests/*.csproj'
    arguments: '--configuration $(buildConfiguration) --collect:"XPlat Code Coverage"'
    publishTestResults: true

- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: Cobertura
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
    reportDirectory: '$(Agent.TempDirectory)/**/coverage.html'
```

## 📝 Testing Best Practices

### Test Naming Convention
```csharp
[Fact]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    // Act
    // Assert
}
```

### Test Structure
```csharp
[Fact]
public void PlaceBetAsync_WithSufficientBalance_ShouldCreateBet()
{
    // Arrange - Set up test data and mocks
    var user = new UserBuilder().WithPoints(1000).Build();
    var race = new RaceBuilder().BuildUpcomingRace();

    _mockUserRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(user);
    _mockRaceRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(race);

    // Act - Execute the method under test
    await _bettingService.PlaceBetAsync(1, 1, 1, 100);

    // Assert - Verify the expected behavior
    _mockBetRepository.Verify(repo => repo.AddAsync(It.IsAny<Bet>()), Times.Once);
    _mockUserRepository.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Once);
}
```

### Mocking Guidelines
- Mock external dependencies (repositories, APIs)
- Don't mock domain entities
- Use `It.IsAny<T>()` for parameters you don't care about
- Verify important interactions

## 🎓 Test Coverage Improvement Strategy

### Phase 1: Foundation (Current)
- ✅ Set up test framework (xUnit, Moq)
- ✅ Create test data builders
- ✅ Implement core service tests
- ✅ Document coverage goals

### Phase 2: Expansion
- Add domain event handler tests
- Implement specification pattern tests
- Add unit of work transaction tests
- Create integration tests for critical paths

### Phase 3: Optimization
- Identify and test edge cases
- Add performance tests for critical operations
- Implement property-based testing for complex logic
- Set up automated coverage monitoring

## 📈 Monitoring and Maintenance

### Coverage Monitoring
- Track coverage trends over time
- Set up alerts for coverage drops
- Review coverage in pull requests

### Test Maintenance
- Update tests when requirements change
- Remove obsolete tests
- Refactor tests along with production code
- Keep test data builders updated

## 🔧 Troubleshooting

### Common Issues
- **Mock not set up**: Ensure all required mocks are configured
- **Test data invalid**: Use builders to create valid entities
- **Async issues**: Use `await` properly in tests
- **Coverage gaps**: Identify untested branches and add cases

### Debugging Tips
- Use `Debug.WriteLine` for test output
- Check test output window for details
- Run tests in debug mode to step through
- Verify mock setups and invocations

This comprehensive test infrastructure ensures high-quality, maintainable tests that support the F1 Betting Game's development and evolution.