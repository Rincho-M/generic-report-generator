[![English](https://img.shields.io/badge/lang-en-red.svg)](README.md)
[![Russian](https://img.shields.io/badge/lang-ru-blue.svg)](README.ru.md)

# 🎫 Generic Report Generator 🎫
Распределенная система генерации отчетов, демонстрирующая архитектурные паттерны, асинхронную обработку и надежную оркестрацию инфраструктуры.

Я написал этот проект, чтобы освежить свои знания и изучить новые технологии. Основной упор был сделан на инфраструктуру, поэтому я намеренно использовал инструменты, которые могут показаться избыточными (отдельный Worker или RabbitMq), или местами выбирал упрощенный код и архитектуру.

#### Проект демонстрирует:
- Внешнее кэширование *(Redis)*
- Межсервисное взаимодействие через очередь *(RabbitMq)*
- Интеграция с внешним API *(OpenMeteo API)*
- Асинхронная обработка длительных запросов *(Генерация отчетов)*
- Хранение и доступ к бинарным данным (Blob) *(Файлы отчетов)*
- Контейнеризация многосервисного приложения *(Docker)*
- Оркестрация контейнеров *(Docker Compose)*
- Распределенная трассировка *(OpenTelemetry)*

## 🏭 Архитектура

Проект следует варианту **N-Layer Architecture** с **Feature-based** структурой папок. Система спроектирована для обработки длительных задач, ресурсоемких для CPU/IO, без блокировки API для пользователя.

### Структура проекта
- **src/apps**: Исполняемые точки входа (API, Worker, Migrations).
- **src/libs**: Общая бизнес-логика (Core), доступ к данным (Infrastructure) и общая конфигурация (Shared).
- **tests**: Интеграционные и модульные тесты.

### Пример потока данных
1. **API Layer**: Принимает запрос на создание и в одной транзакции сохраняет в **PostgreSQL** запись со статусом "Pending" **и** строку outbox. Фоновый dispatcher затем публикует сообщение из outbox в **RabbitMQ**.
2. **Messaging**: **MassTransit** обеспечивает надежную доставку сообщений. Система включает политику экспоненциальных повторов (retry policy) и очереди недоставленных сообщений (Dead Letter Queues, DLQ) для обеспечения отказоустойчивости.
3. **Worker Layer**: Обрабатывает сообщение, получает данные из **OpenMeteo API** (с использованием кэша, если возможно), генерирует Excel-файл с помощью **ClosedXML** и сохраняет результат в файловую систему.

### Стратегия кэширования
К репозиторию погодных данных из внешнего API применяется паттерн **Decorator**, который добавляет функционал кэширования, что значительно снижает задержки и нагрузку на внешнее API при повторных запросах. **Redis** используется в роли хранилища кэшируемых данных.

## 🚀 Деплой

### Предварительные требования
- Docker & Docker Compose
- .NET 10 SDK (для локальной разработки)

### Локальное окружение Docker
Чтобы запустить всю инфраструктуру (Postgres, RabbitMQ, Redis) вместе с API и Worker:

```bash
cd deploy
docker-compose up -d --build
```

После запуска будут доступны ресурсы:
- **Swagger UI**: `http://localhost:8080/swagger`
- **RabbitMQ Dashboard**: `http://localhost:15672` (логин: `admin`, пароль: `admin`)

### Готовность к Cloud & Kubernetes
Система построена с использованием принципов cloud-native:
- **Паттерн Init Container**: Миграции базы данных обрабатываются отдельным краткосрочным контейнером, который должен завершить работу перед запуском API/Worker.
- **Структурированное логирование**: Используется **Serilog** с форматом JSON, готовый для сбора системами Loki или ELK.
- **Observability (Наблюдаемость)**: Интегрирован **OpenTelemetry** для распределенной трассировки, что позволяет сопоставлять логи API и Worker через общий `TraceId`.

## 🧪 Тестирование

В проекте реализована стратегия "Testing Trophy", где приоритет отдается интеграционным тестам на реальной инфраструктуре.

- **Integration Tests**: Используют **Testcontainers** для запуска реальных экземпляров PostgreSQL и RabbitMQ в Docker. Это гарантирует проверку взаимодействия сервисов и базы данных в среде, максимально близкой к продакшну.
- **Unit Tests**: Сосредоточены на сложной логике, в частности на сборке Excel-файлов и маппинге в Global Exception Handler.

Запуск тестов:
```bash
dotnet test
```

## 💾 Рабочий процесс

### Стиль кода
Стиль кода определен в файле `.editorconfig` и контролируется pre-commit хуком через `husky.net`. При каждой попытке коммита измененные файлы проходят через команду `dotnet format`. Если будут обнаружены несоответствия стилю, коммит будет отклонен. В таком случае нужно выполнить команду `husky run --name format` в папке решения, которая применит правила стиля к файлам.

### Миграции

Миграции управляются через пакет `Microsoft.EntityFrameworkCore.Tools`.
Рекомендуемый способ — использование Package Manager Console в Visual Studio.

#### Создание (Create)

Для создания миграции используется команда:

```bash
Add-Migration -Name #DescriptiveName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Эквивалент .NET CLI (из корня репозитория):

```bash
dotnet ef migrations add #DescriptiveName# --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*Заменить #DescriptiveName# на фактическое название миграции.*

#### Удаление (Remove)
Чтобы удалить еще не примененную миграцию, используется:

```bash
Remove-Migration -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Эквивалент .NET CLI (из корня репозитория):

```bash
dotnet ef migrations remove --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*Чтобы удалить уже примененную миграцию, сначала нужно откатить её в целевой базе данных.*

#### Откат (Revert)
Чтобы откатить уже примененную миграцию, используется:

```bash
Update-Database #TargetMigrationName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context AppDbContext
```

Эквивалент .NET CLI (из корня репозитория).

```bash
dotnet ef database update #TargetMigrationName# --project src/GenericReportGenerator.Migrations --startup-project src/GenericReportGenerator.Migrations --context AppDbContext
```

*Заменить #TargetMigrationName# на название миграции, к которой нужно вернуться.*

#### Применение (Apply)

Миграции применяются путем запуска проекта `GenericReportGenerator.Migrations`. В файле `appsettings.json` этого проекта строка `ConnectionStrings:Database` должна указывать на нужную базу данных.