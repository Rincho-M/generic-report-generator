[![English](https://img.shields.io/badge/lang-en-red.svg)](README.md)
[![Russian](https://img.shields.io/badge/lang-ru-blue.svg)](README.ru.md)

# 🎫 Generic Report Generator 🎫
A distributed report generation system demonstrating architectural patterns, asynchronous processing, and robust infrastructure orchestration.

I built this to brush up on what I already know and to learn some new things. I wanted to make it more infrastructure oriented, so I intentionally used tools that might be overkill (separate worker or message bus) or chose oversimplified code and architecture here and there.

#### The project showcases:
- External caching *(Redis)*
- Service-To-Service message bus communication *(RabbitMq)*
- External API integration *(OpenMeteo API)*
- Asynchronous long-running request processing *(Report generation)*
- Blob data storage and access *(Report files)*
- Multi-service app containerization *(Docker)*
- Container orchestration *(Docker Compose)*
- Distributed tracing *(OpenTelemetry)*

## 🏭 Architecture

The project follows a variant of **N-Layer Architecture** with **Feature-based folder structure**. It designed to handle long-running CPU/IO-intensive tasks without blocking the user-facing API.

### Project Structure
- **src/apps**: Executable entry points (API, Worker, Migrations).
- **src/libs**: Shared business logic (Core), data access (Infrastructure), and shared configuration (Shared).
- **tests**: Integration and Unit tests.

### Data flow example
1. **API Layer**: Receives create request and persists a "Pending" record **and** an outbox row to **PostgreSQL** in one transaction. A background dispatcher later publishes the outbox message to **RabbitMQ**.
2. **Messaging**: **MassTransit** handles the reliable delivery of messages. It includes an exponential retry policy and Dead Letter Queues (DLQ) for fault tolerance.
3. **Worker Layer**: Consumes the message, fetches data from the **OpenMeteo API** (cached if possible), generates an Excel file using **ClosedXML**, and stores the result in the file system.

### Caching Strategy
Implements the **Decorator Pattern** for the data repository. It uses **Redis** to cache external API responses, significantly reducing latency and external API consumption for repeated requests.

### Exception handling and logging
Exceptions bubble. Services do not catch them to log or translate them. A catch belongs only at a boundary that has no handler above it.

- **Domain exceptions** extend `DomainException`. They mean the user did something that would break a business rule, such as an unknown city or a file requested before the report is ready.
- **System exceptions** are everything else: infrastructure, the weather provider, the cache, or file generation.
- **Validation** (`FluentValidation`) is separate from both and is not a domain exception.

**API.** `GlobalExceptionHandler` is the only HTTP log site. Validation returns 400 and is logged at information. A domain exception returns 422 and is logged at warning with the exception. A system exception returns 500, is logged at error with the exception, and the client gets a generic message. The body includes `TraceId`.

**Worker.** `GlobalExceptionFilter` runs on each consume attempt, inside the retry pipe. It logs a domain exception at warning and a system exception at error, then rethrows. Cancellation is not logged. A system exception is logged on every attempt, and MassTransit retries those with exponential backoff. `DomainException` is ignored, so it is logged once and faults immediately. The fault consumer does not log again; it marks the report `Failed`.

**Other boundaries.** The outbox dispatcher logs its own message and keeps polling. The migrations process logs the exception at critical and rethrows. The API and worker host catch logs a fatal event if the process terminates.

**Structured logging.** Serilog writes JSON. Templates use named properties, and the exception is passed into the log call so the stack trace is on the event. `TraceId` ties the API and worker together.

## 🚀 Deployment

### Prerequisites
- Docker & Docker Compose
- .NET 10 SDK (for local development)

### Local Docker Environment
To spin up the entire infrastructure (Postgres, RabbitMQ, Redis) along with the API and Worker:

```bash
cd deploy
docker-compose up -d --build
```

Once running, you can access:
- **Swagger UI**: `http://localhost:8080/swagger`
- **RabbitMQ Dashboard**: `http://localhost:15672` (admin/admin)

### Cloud & Kubernetes Readiness
The system is built with cloud-native principles:
- **Init Container Pattern**: Database migrations are handled by a dedicated short-lived container that must complete before the API/Worker start.
- **Structured Logging**: Uses **Serilog** with JSON formatting, ready for ingestion by Loki or ELK stacks.
- **Observability**: **OpenTelemetry** is integrated for distributed tracing, allowing correlation of logs across the API and Worker via a shared `TraceId`.

## 🧪 Testing

The project implements a "Testing Trophy" strategy, prioritizing integration tests over real infrastructure.

- **Integration Tests**: Utilizes **Testcontainers** to spin up real PostgreSQL and RabbitMQ instances in Docker. This ensures the communication between services and the database is verified in a production-like environment.
- **Unit Tests**: Focuses on complex logic, specifically the Excel file building and the Global Exception Handler mapping.

To run the tests:
```bash
dotnet test
```

## 💾 Workflow

### Code style
Code style is defined in `.editorconfig` file and enforced by pre-commit hook executed by `husky.net`. On every commit attempt, staged files will be going through `dotnet format` command and if it will find inconsistencies with defined code style, the commit will be rejected. In such cases you would need to execute `husky run --name format` command in the solution folder, which would apply code style rules to your staged files.

### Migrations

Migrations are managed through `Microsoft.EntityFrameworkCore.Tools` package.
The recommended way is to use Package Manager Console in Visual Studio.

#### Create

To create a migration use this command:

```bash
Add-Migration -Name #DescriptiveName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Equivalent .NET CLI (from repo root):

```bash
dotnet ef migrations add #DescriptiveName# --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*Replace #DescriptiveName# with actual migration name.*

#### Remove
To remove not applied migration use this command:

```bash
Remove-Migration -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Equivalent .NET CLI (from repo root):

```bash
dotnet ef migrations remove --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*To remove already applied migration, you need to revert it in target database*

#### Revert
To revert already applied migration use this command:

```bash
Update-Database #TargetMigrationName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Equivalent .NET CLI (from repo root).

```bash
dotnet ef database update #TargetMigrationName# --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*Replace #TargetMigrationName# with migration name you want to go to.*

#### Apply

Migrations are applied by running `GenericReportGenerator.Migrations` project. In `appsettings.json` the project must have `ConnectionStrings:Database` pointing to a desired database.