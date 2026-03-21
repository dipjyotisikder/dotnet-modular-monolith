# .NET Modular Monolith

A production-ready template demonstrating **Modular Monolith architecture**, **Domain-Driven Design**, and **Clean Architecture** principles using **ASP.NET Core**, **C# 14**, and **.NET 10**. Build scalable applications with responsibility segregation, enterprise patterns, and battle-tested best practices.

## 🎯 Core Principles

- **Modular Monolith** - Feature modules with clear boundaries and independent business logic
- **Domain-Driven Design** - Ubiquitous language, aggregates, and bounded contexts per module
- **Clean Architecture** - Clear separation: Domain → Application → Infrastructure layers
- **Responsibility Segregation** - SOLID principles with distinct concerns (Features, Domain, Infrastructure)
- **Database Best Practices** - EF Core migrations, proper indexing, and schema management
- **EF Core Optimization** - Lazy loading prevention, query projection, and efficient data access patterns

## 🚀 Key Features

- **Independently Deployable Modules** - Users and Bookings modules with encapsulated logic
- **Outbox Pattern** - Reliable event publishing with eventual consistency
- **Distributed Locks** - Safe concurrent operations across instances
- **Permission-Based Authorization** - Role and claim-based security model
- **Comprehensive Testing** - Unit, integration, and architecture layer tests
- **Automated Migrations** - Database schema versioning and deployment safety
- **.NET 10 & C# 14** - Latest framework capabilities and language features

## 📦 Architecture & Project Structure

### Layered Design (Per Module)

```
Module/
├── Domain/           # Entities, aggregates, value objects, domain events
├── Application/      # Use cases, commands, queries, DTOs, business logic orchestration
└── Infrastructure/   # Data access, repositories, external services, EF Core configuration
```

### Project Organization

```
src/
├── AppHost/                  # Bootstrapper: service registration, middleware pipeline
├── Modules/
│   ├── Users/               # User bounded context (Domain-Driven Design)
│   │   ├── Domain/          # Core business rules, value objects, aggregates
│   │   ├── Features/        # Application commands and queries
│   │   └── Infrastructure/  # Repository implementations, EF configuration
│   └── Bookings/            # Booking bounded context
│       ├── Domain/
│       ├── Features/
│       └── Infrastructure/
└── Shared/                   # Cross-cutting infrastructure (auth, database, events)
tests/
├── ModulesTests/           # Unit and integration tests per module
└── ArchitectureTests/      # Layer validation and architecture rules
```

## 🏗️ Architecture Highlights

**Clean Architecture Implementation:**
- ✅ Domain layer isolation - zero external dependencies
- ✅ Application layer orchestration - use case handling
- ✅ Infrastructure abstraction - repository patterns, EF Core encapsulation
- ✅ Responsibility segregation - each class has a single reason to change

**DDD Patterns:**
- Bounded Contexts per module (Users, Bookings)
- Aggregates as transaction boundaries
- Domain events for cross-module communication
- Value objects for type safety

**EF Core Best Practices:**
- Explicit query loading (no lazy loading enabled)
- Projection to DTOs at query level
- Optimized migrations with proper indexing
- DbContext pooling for performance

**Database Best Practices:**
- Schema versioning via migrations
- Outbox pattern for transactional consistency
- Proper foreign key constraints
- Audit trail support (seedable entities)

## 🚀 Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (or configure your preferred database)

### Installation & Running

```bash
# Clone repository
git clone https://github.com/dipjyotisikder/dotnet-modular-monolith.git
cd dotnet-modular-monolith

# Restore dependencies
dotnet restore

# Apply migrations and seed data
dotnet run --project src/AppHost

# Run test suite
dotnet test

# Run specific module tests
dotnet test --filter "Users or Bookings"
```

## 🧪 Testing Strategy

- **Unit Tests** - Domain logic and business rule validation
- **Integration Tests** - Feature layer with real dependencies
- **Architecture Tests** - Enforce layer separation and design rules
- **Domain Layer Tests** - Aggregate behavior and invariants

## 📚 Key Concepts

| Concept | Purpose |
|---------|---------|
| **Aggregate** | Boundary of consistency, enforced in Domain layer |
| **Repository** | Data access abstraction (Infrastructure layer) |
| **Value Object** | Immutable, identity-less business concepts |
| **Domain Event** | Significant business events published from aggregates |
| **Outbox Pattern** | Reliable transactional event publishing |
| **Bounded Context** | Module isolation with clear contracts |

## 📄 License

This project is open source and available under the MIT License.

---

**A blueprint for building enterprise applications with proven architectural patterns and responsibility segregation.**
