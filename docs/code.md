# Coding Guidelines

_Refer to [plan.md](./plan.md) for the recommended code structure._

## General Principles
- Write clean, readable, and maintainable code.
- Adhere to SOLID principles and separation of concerns.
- Favor composition over inheritance.
- Keep business logic in the domain layer; avoid leaking infrastructure details.
- Use dependency injection for all external dependencies.
- Write unit and integration tests for all business logic and adapters.

## .NET & C# Standards
- Follow the official [.NET Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Use `PascalCase` for class, method, and property names.
- Use `camelCase` for local variables and parameters.
- Use explicit access modifiers (public, private, etc.).
- Prefer `async`/`await` for I/O-bound operations.
- Use nullable reference types and avoid nulls where possible.
- Keep methods short and focused (single responsibility).
- Use meaningful names for all identifiers.

## Project Structure
- Place API controllers in `FileIngestion.Api`.
- Place use cases and service interfaces in `FileIngestion.Application`.
- Place domain models and business logic in `FileIngestion.Domain`.
- Place infrastructure implementations (Cosmos, Blob, etc.) in `FileIngestion.Infrastructure`.
- Place shared DTOs, contracts, and utilities in `FileIngestion.Shared`.
- Place all tests in the `tests` folder, mirroring the structure of `src`.

## What To Do
- Write XML or Markdown documentation for public APIs and complex logic.
- Use exception handling and meaningful error messages.
- Validate all external inputs (API, storage, etc.).
- Use configuration and secrets from environment or Azure Key Vault.
- Use logging and tracing (OpenTelemetry) for all critical operations.
- Review and test all code before merging.
- Use code reviews and automated CI/CD checks.

## What Not To Do
- Do not place business logic in controllers or infrastructure.
- Do not use magic strings or hard-coded values; use constants or configuration.
- Do not catch general exceptions without handling or logging.
- Do not expose sensitive information in logs or error messages.
- Do not commit secrets or credentials to source control.
- Do not ignore failing tests or code analysis warnings.

## Code Style Profile

### Editor Configuration
- `.editorconfig` file has been created at the root of the project to enforce consistent coding styles.
- Configure your IDE to format on save for consistent code formatting.

### Required Tools & Extensions
1. **IDE Extensions:**
   - .NET Core Test Explorer
   - C# Dev Kit
   - C# Extensions
   - EditorConfig for VS Code

2. **Code Analysis:**
   ```xml
   <!-- Add to .csproj files -->
   <ItemGroup>
     <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.507">
       <PrivateAssets>all</PrivateAssets>
       <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
     </PackageReference>
     <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0">
       <PrivateAssets>all</PrivateAssets>
       <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
     </PackageReference>
   </ItemGroup>
   ```

3. **Pre-commit Hooks:**
   - Install `dotnet-format` globally: `dotnet tool install -g dotnet-format`
   - Set up Git hooks to run `dotnet format` before commits

### Build Pipeline Requirements
```yaml
# Add to GitHub Actions workflow
- name: Code Analysis
  run: |
    dotnet restore
    dotnet build --configuration Release /warnaserror
    dotnet format --verify-no-changes
```

### Code Style Rules
1. **Formatting:**
   - Use spaces for indentation (4 spaces)
   - Keep lines under 120 characters
   - Use blank lines to group related code

2. **Naming:**
   - Use PascalCase for types and members
   - Use camelCase for local variables and parameters
   - Prefix interfaces with 'I'
   - Suffix exceptions with 'Exception'

3. **Code Organization:**
   - One type per file
   - Group members by type (fields, properties, constructors, methods)
   - Keep files under 500 lines
   - Keep methods under 30 lines

4. **Language Features:**
   - Use C# latest features appropriately
   - Prefer pattern matching over type checking
   - Use records for DTOs and immutable types
   - Use init-only properties where appropriate

## References
- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
