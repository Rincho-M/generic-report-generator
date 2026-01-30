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
1. **API Layer**: Receives create request, persists a "Pending" record to **PostgreSQL**, and publishes a message to **RabbitMQ**.
2. **Messaging**: **MassTransit** handles the reliable delivery of messages. It includes an exponential retry policy and Dead Letter Queues (DLQ) for fault tolerance.
3. **Worker Layer**: Consumes the message, fetches data from the **OpenMeteo API** (cached if possible), generates an Excel file using **ClosedXML**, and stores the result in the file system.

### Caching Strategy
Implements the **Decorator Pattern** for the data repository. It uses **Redis** to cache external API responses, significantly reducing latency and external API consumption for repeated requests.

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

`Add-Migration -Name #DescriptiveName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*Replace #DescriptiveName# with actual migration name.*

#### Remove
To remove not applied migration use this command:

`Remove-Migration -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*To remove already applied migration, you need to revert it in target database*

#### Revert
To revert already applied migration use this command:

`Update-Database #TargetMigrationName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*Replace #TargetMigrationName# with migration name you want to go to.*

#### Apply

Migrations are applied by running `GenericReportGenerator.Migrations` project. In `appsettings.json` the project must have `ConnectionStrings:Database` pointing to a desired database.