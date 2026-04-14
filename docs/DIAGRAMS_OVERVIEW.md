# F1 Betting Application - Diagrams Overview

## 📊 Complete Set of PlantUML Diagrams

This document provides an overview of all PlantUML diagrams created for the F1 Betting Application, explaining their purpose and what they illustrate.

---

## 1. **Class Diagram** (`class_diagram.puml`)

**Purpose**: Shows the domain layer entity relationships and data structures

**Contains**:
- Domain entities: `User`, `Bet`, `Race`, `Driver`, `Team`
- Enumerations: `BetStatus`, `RaceStatus`
- Entity relationships and properties
- Method signatures

**Use Case**: Understanding the core business models and their relationships

**Key Relationships**:
- User places Bet
- Bet relates to Race and Driver
- Driver belongs to Team

---

## 2. **Database Schema** (`database_schema_domain.puml`)

**Purpose**: Visualizes the SQL database structure aligned with domain models

**Contains**:
- 5 main tables: Users, Bets, Races, Drivers, Teams
- Column definitions with SQL data types
- Primary and foreign keys
- Constraints and indexes
- Table relationships

**Use Case**: Database design and understanding data persistence

**Key Features**:
- CHECK constraints for enum values
- UNIQUE constraints for user identifiers
- Foreign key relationships with CASCADE rules
- Performance indexes recommendations

---

## 3. **Architecture Component Diagram** (`architecture_component_diagram.puml`)

**Purpose**: Shows the complete application architecture with all components and their interactions

**Contains**:
- Frontend: Angular SPA with modules (Auth, Betting, Dashboard, F1 Data)
- API Layer: Controllers and authentication
- Application Layer: Services, DTOs, Validators, Mapping
- Domain Layer: Entities and Enums
- Infrastructure Layer: Repositories, DbContext, External APIs, Background Jobs
- Database: SQL Server tables
- External: OpenF1 API

**Use Case**: High-level understanding of system architecture and component dependencies

**Layers**:
1. Client (Angular)
2. API (REST endpoints)
3. Application (Business logic)
4. Domain (Models)
5. Infrastructure (Data access, External services)
6. Data (SQL Server)

---

## 4. **Layered Architecture Diagram** (`architecture_layers_diagram.puml`)

**Purpose**: Detailed view of the 5-layer clean architecture

**Contains**:
- Presentation Layer (Angular UI)
- API Layer (REST endpoints)
- Application Layer (Services & business logic)
- Domain Layer (Models & enums)
- Infrastructure Layer (Data access & integrations)
- External Services (OpenF1 API)

**Use Case**: Understanding architectural principles and layer responsibilities

**Key Principles**:
- Clean Architecture
- Dependency Rule (inner layers don't depend on outer)
- Separation of Concerns
- SOLID Principles

---

## 5. **Project Structure Diagram** (`project_structure_diagram.puml`)

**Purpose**: Visual representation of the file and folder hierarchy

**Contains**:
- Angular Frontend (src/app with feature modules)
- .NET Solution structure
  - F1BettingApp.API
  - F1BettingApp.Application
  - F1BettingApp.Domain
  - F1BettingApp.Infrastructure
  - F1BettingApp.Tests
- Documentation folder (docs)
- Configuration files

**Use Case**: Navigating the codebase and understanding project organization

**Project Folders**:
- `src/` - Angular application
- `F1BettingApp/` - .NET solution
  - Each project organized by responsibility
  - Tests for quality assurance
  - Comprehensive documentation

---

## 6. **Sequence Diagram: Place Bet** (`sequence_diagram_place_bet.puml`)

**Purpose**: Shows the step-by-step flow of placing a bet through the entire application

**Flow**:
1. User fills in bet form (Frontend)
2. POST request sent to API with JWT token
3. API validates JWT and routes to controller
4. Controller calls BettingService
5. Service validates bet (balance, race status, etc.)
6. Service persists bet to database via Repository
7. Response returned to user

**Interactions**:
- User ↔ Angular
- Angular ↔ HTTP Interceptor
- Interceptor ↔ API
- API ↔ Services
- Services ↔ Validation
- Services ↔ Repository
- Repository ↔ Database

**Use Case**: Understanding user workflows and system interactions

**Key Points**:
- JWT authentication on every request
- Multi-layer validation
- Error handling at each stage
- Transactional integrity

---

## 7. **Sequence Diagram: Race Result Processing** (`sequence_diagram_race_result_processing.puml`)

**Purpose**: Shows how background jobs process race results automatically

**Flow**:
1. Scheduler triggers RaceResultWorker
2. Worker fetches unprocessed races from database
3. For each finished race:
   - Fetch results from OpenF1 API
   - Load all pending bets for that race
   - Determine win/loss status
   - Update bet records
   - Update race status
4. Automatic point calculation and distribution

**Interactions**:
- Scheduler ↔ Background Worker
- Worker ↔ Repository
- Repository ↔ Database
- Worker ↔ OpenF1 API
- Service ↔ Business Logic

**Use Case**: Understanding automated background processing

**Benefits**:
- Non-blocking to user interactions
- Automatic result settlement
- Reliable transaction handling
- Auditable processing

---

## 8. **Deployment & Infrastructure Diagram** (`deployment_infrastructure_diagram.puml`)

**Purpose**: Shows production deployment architecture and infrastructure components

**Contains**:
- Client Tier: User browsers with Angular SPA
- CDN: Static assets distribution (optional)
- API Tier: Multiple ASP.NET Core instances with load balancer
- Cache Tier: Redis for caching (optional)
- Data Tier: SQL Server primary + backup
- External Services: OpenF1 API
- Monitoring: Application Insights, Logging, Performance monitoring

**Deployment Tiers**:
1. **Client**: Browser-based Angular application
2. **CDN**: JavaScript, CSS, images (optional)
3. **API**: Load-balanced ASP.NET Core servers
4. **Cache**: Redis for performance (optional)
5. **Database**: SQL Server with replication
6. **External**: OpenF1 API integration
7. **Monitoring**: Observability and logging

**Use Case**: Understanding production infrastructure and deployment options

**Characteristics**:
- Scalable: Horizontal scaling of API servers
- Available: Redundancy and backup
- Secure: HTTPS, JWT, parameterized queries
- Performant: Async operations, caching, indexes

---

## 📚 Additional Documentation

### SQL Scripts

#### 1. **Database Creation** (`01_create_database.sql`)
- Creates all 5 core tables
- Defines relationships and constraints
- Establishes indexes for performance
- Includes table descriptions

#### 2. **Sample Data** (`02_seed_sample_data.sql`)
- Inserts test data
- 5 sample users
- 7 teams with 14 drivers
- 8 races with various statuses
- 12 sample bets for testing

---

## 🎯 How to Use These Diagrams

### For Development
- **Class & Database Diagrams**: Reference for schema understanding
- **Architecture Diagrams**: Understanding how code is organized
- **Project Structure**: Navigating the codebase

### For Communication
- **Component Architecture**: Explain system to stakeholders
- **Layered Architecture**: Discuss design decisions
- **Sequence Diagrams**: Walkthrough workflows

### For Troubleshooting
- **Sequence Diagrams**: Trace request flow to find issues
- **Deployment Diagram**: Identify infrastructure bottlenecks

### For Learning
- **All Diagrams**: Understanding the complete system
- **Sequence Diagrams**: Learning how features work
- **Architecture**: Understanding design patterns

---

## 🔄 Diagram Relationships

```
┌─────────────────────────────────────────────────────┐
│         User interacts with Application              │
└──────────────┬──────────────────────────────────────┘
               │
        ┌──────▼──────────┐
        │  Sequence Flow  │ (Place Bet / Process Results)
        └──────┬──────────┘
               │
      ┌────────▼────────────┐
      │ Component & Layers  │ (Architecture)
      └────────┬────────────┘
               │
      ┌────────▼──────────────┐
      │ Project Structure     │ (File organization)
      └────────┬──────────────┘
               │
      ┌────────▼──────────────┐
      │ Class & Database      │ (Data models)
      └────────┬──────────────┘
               │
      ┌────────▼──────────────┐
      │ Deployment            │ (Production setup)
      └───────────────────────┘
```

---

## 📋 Viewing the Diagrams

### Online Tools
- **PlantText**: https://www.planttext.com/
- **PlantUML Editor**: https://editor.plantuml.com/
- **Kroki.io**: https://kroki.io/

### VS Code Extensions
- **PlantUML**: Extension for VS Code by jebbs
- **Graphviz Preview**: For visualization

### Command Line
```bash
# Generate PNG from .puml file
plantuml -tpng diagram.puml

# Generate SVG
plantuml -tsvg diagram.puml
```

---

## 🚀 Next Steps

These diagrams can be used to:
1. **Onboard new developers** - Complete visual reference
2. **Document decisions** - Architecture rationale
3. **Plan features** - Understanding integration points
4. **Monitor performance** - Identifying optimization opportunities
5. **Scale infrastructure** - Based on deployment diagram

---

## 📝 Notes

All diagrams are:
- ✅ PlantUML format (text-based)
- ✅ Version control friendly (no binary files)
- ✅ Easy to update and maintain
- ✅ Automatically rendereable in most tools
- ✅ Properly documented with annotations

For questions or updates to diagrams, refer to the source `.puml` files in the `docs/` folder.
