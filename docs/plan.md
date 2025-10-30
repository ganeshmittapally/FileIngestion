
# Code Structure Plan

_Refer to [design.md](./design.md) for architecture and technology context._

This document outlines the proposed code structure for the File Ingestion microservice, following .NET and hexagonal architecture best practices.

## Solution Structure

```
FileIngestion.sln
│
├── src/
│   ├── FileIngestion.Api/           # ASP.NET Core Web API (inbound adapters)
│   ├── FileIngestion.Application/   # Application layer (use cases, ports)
│   ├── FileIngestion.Domain/        # Domain models and business logic
│   ├── FileIngestion.Infrastructure/# Outbound adapters (Cosmos, Blob, etc.)
│   └── FileIngestion.Shared/        # Shared utilities, DTOs, contracts
│
├── tests/
│   ├── FileIngestion.Api.Tests/
│   ├── FileIngestion.Application.Tests/
│   ├── FileIngestion.Domain.Tests/
│   └── FileIngestion.Infrastructure.Tests/
│
├── deploy/
│   ├── terraform/                   # Terraform scripts for Azure resources
│   └── docker/                      # Dockerfiles and related assets
│
├── .github/
│   └── workflows/                   # GitHub Actions CI/CD pipelines
│
└── docs/                            # Documentation (design.md, plan.md, etc.)
```

## Key Project Areas

- **API Layer:**
  - REST endpoints, request validation, OpenAPI/Swagger
- **Application Layer:**
  - Use cases, service interfaces (ports), orchestration
- **Domain Layer:**
  - Core business logic, domain models, value objects
- **Infrastructure Layer:**
  - Implementations for Cosmos DB, Blob Storage, external integrations
- **Shared:**
  - DTOs, contracts, common utilities
- **Tests:**
  - Unit, integration, and contract tests for each layer
- **Deploy:**
  - Infrastructure as Code (Terraform), Docker assets
- **CI/CD:**
  - Automated build, test, and deployment workflows

## Next Steps
- Scaffold the solution and projects as per this structure
- Define initial interfaces and contracts
- Set up CI/CD and infrastructure scripts
