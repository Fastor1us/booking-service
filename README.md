# Booking API
REST API for booking events - create, manage and booking


## 📋 Requirements

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/)
- CLI / IDE


## 📑 Navigation

- [🚀 Quick Start](#quick-start)
- [🗄️ Database Migrations](#️-database-migrations)
- [🧪 Testing](#testing)
- [🌐 API Endpoints](#api-endpoints)
  - [📅 Events Controller](#events-controller-apievents)
  - [📖 Booking Controller](#booking-controller-apibookings)
  - [📊 Query Parameters](#query-parameters)
- [📦 Models](#models)
  - [EventResponseDto](#eventresponsedto)
  - [PaginatedEventsResponseDto](#paginatedeventsresponsedto)
  - [CreateEventDto](#createeventdto)
  - [UpdateEventDto](#updateeventdto)
  - [BookingResponseDto](#bookingresponsedto)
  - [BookingStatus Enum](#bookingstatus-enum)
  - [ErrorResponseDto](#errorresponsedto)
- [📋 HTTP Status Codes](#http-status-codes)
- [🧠 Background Processing](#background-processing)
- [🏗️ Architecture](#️architecture)
- [🛠️ Technology Stack](#technology-stack)


## 🚀 Quick Start

### Restore dependencies

```bash
dotnet restore ./src/BookingApi.csproj
```

### Build project

```bash
dotnet build ./src/BookingApi.Presentation/BookingApi.Presentation.csproj
```

### Run project

```bash
dotnet run --project ./src/BookingApi.Presentation/BookingApi.Presentation.csproj
```

After running, access [Swagger UI](http://localhost:5142/swagger/index.html)


## 🗄️ Database Migrations
The project uses Entity Framework Core migrations to manage database schema changes.

### Prerequisites
Install the EF Core tools globally (if not already installed):

```bash
dotnet tool install --global dotnet-ef
```

### Create a new migration

```bash
dotnet ef migrations add <MigrationName> \
    --project ./src/BookingApi.Infrastructure/BookingApi.Infrastructure.csproj \
    --startup-project ./src/BookingApi.Presentation/BookingApi.Presentation.csproj
```

### Apply migrations

```bash
dotnet ef database update \
    --project ./src/BookingApi.Infrastructure/BookingApi.Infrastructure.csproj \
    --startup-project ./src/BookingApi.Presentation/BookingApi.Presentation.csproj
```

### Remove last migration

```bash
dotnet ef migrations remove \
    --project ./src/BookingApi.Infrastructure/BookingApi.Infrastructure.csproj \
    --startup-project ./src/BookingApi.Presentation/BookingApi.Presentation.csproj
```

### Automatic Migration
The application automatically applies pending migrations on startup:

```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}
```


## 🧪 Testing

The solution includes comprehensive testing with two separate test projects:

### 📂 Test Structure

```
tests/
├── BookingApi.Application.Tests/
└── BookingApi.Infrastructure.Tests/
```

### Unit Tests (BookingApi.Application.Tests)
- **Purpose:** Test individual components in isolation
- **Tools:** xUnit, Moq, In-Memory Database
- **Scope:** Services, Domain logic, Validation
- **Characteristics:** Fast execution, no external dependencies

### Integration Tests (BookingApi.Infrastructure.Tests)
- **Purpose:** Test real database interactions and component integration
- **Tools:** xUnit, Testcontainers.PostgreSql
- **Scope:** Repositories, UnitOfWork, Db Constraints and Transactions
- **Characteristics:** Uses real PostgreSQL container, tests data persistence

### Run tests

```bash
dotnet test
```

### Run only unit tests

```bash
dotnet test ./tests/BookingApi.Application.Tests/BookingApi.Application.Tests.csproj
```

### Run only integration tests

```bash
dotnet test ./tests/BookingApi.Infrastructure.Tests/BookingApi.Infrastructure.Tests.csproj
```

### Test Database Setup
Integration tests use **Testcontainers** to spin up a real PostgreSQL database in Docker:
- One container instance reused across all test classes
- Automatic cleanup between tests
- No manual database configuration required


## 🌐 API Endpoints

### 📅 Events Controller (`/api/events`)

| Method | Endpoint                | Description                       | Success Response |
| ------ | ----------------------- | --------------------------------- | ---------------- |
| GET    | `/api/events/{id}`      | Get event by ID                   | 200 OK           |
| GET    | `/api/events`           | Get paginated events with filters | 200 OK           |
| POST   | `/api/events`           | Create new event                  | 201 Created      |
| PUT    | `/api/events/{id}`      | Update existing event             | 204 No Content   |
| DELETE | `/api/events/{id}`      | Delete event                      | 204 No Content   |
| POST   | `/api/events/{id}/book` | Book an event                     | 202 Accepted     |

### 📖 Booking Controller (`/api/bookings`)

| Method | Endpoint             | Description       | Success Response |
| ------ | -------------------- | ----------------- | ---------------- |
| GET    | `/api/bookings/{id}` | Get booking by ID | 200 OK           |

### 📊 Query Parameters

#### GET `/api/events` - Query Parameters

| Parameter  | Type     | Description                                      | Constraints          | Default |
| ---------- | -------- | ------------------------------------------------ | -------------------- | ------- |
| `title`    | string   | Filter by title (case-insensitive partial match) | -                    | null    |
| `from`     | DateTime | Filter events starting after this date           | Must be before `to`  | null    |
| `to`       | DateTime | Filter events ending before this date            | Must be after `from` | null    |
| `page`     | int      | Page number                                      | ≥ 1                  | 1       |
| `pageSize` | int      | Items per page                                   | 5-50                 | 10      |

**Example Request:**

```
GET /api/events?title=workshop&from=2024-01-01&to=2024-12-31&page=2&pageSize=20
```

### 📦 Detailed endpoints

### 🎯 GET `/api/events/{id}`

Get a single event by its unique identifier.

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 200         | Success               | [`EventResponseDto`](#eventresponsedto) |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 📋 GET `/api/events`

Get paginated list of events with optional filtering.

**Parameters:**

| Name       | In    | Type       | Required | Description                                      |
| ---------- | ----- | ---------- | -------- | ------------------------------------------------ |
| `title`    | query | `string`   | ❌ No    | Filter by title (case-insensitive partial match) |
| `from`     | query | `DateTime` | ❌ No    | Filter events starting after this date           |
| `to`       | query | `DateTime` | ❌ No    | Filter events ending before this date            |
| `page`     | query | `int`      | ❌ No    | Page number (≥ 1)                                |
| `pageSize` | query | `int`      | ❌ No    | Items per page (5-50)                            |

**Validation Rules:**

- `to` date must be later than `from` date
- `page` must be at least 1
- `pageSize` must be between 5 and 50

**Responses:**

| Status Code | Description                                   | Response Type                                               |
| ----------- | --------------------------------------------- | ----------------------------------------------------------- |
| 200         | Success                                       | [`PaginatedEventsResponseDto`](#paginatedeventsresponsedto) |
| 400         | Validation error or invalid filter/pagination | [`ErrorResponseDto`](#errorresponsedto)                     |
| 500         | Internal server error                         | [`ErrorResponseDto`](#errorresponsedto)                     |

---

### ➕ POST `/api/events`

Create a new event.

**Request Body:**

| Name       | In   | Type                                  | Required | Description         |
| ---------- | ---- | ------------------------------------- | -------- | ------------------- |
| `dto`      | body | [`CreateEventDto`](#createeventdto)   | ✅ Yes  | Event creation data |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 201         | Created               | [`EventResponseDto`](#eventresponsedto) |
| 400         | Validation error      | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### ✏️ PUT `/api/events/{id}`

Update an existing event.

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Request Body:**

| Name       | In   | Type                                   | Required | Description        |
| ---------- | ---- | -------------------------------------- | -------- | ------------------ |
| `dto`      | body | [`UpdateEventDto`](#updateeventrequest)| ✅ Yes  | Updated event data |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 204         | No Content (success)  | -                                       |
| 400         | Validation error      | [`ErrorResponseDto`](#errorresponsedto) |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 🗑️ DELETE `/api/events/{id}`

Delete an event.

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 204         | No Content (success)  | -                                       |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 🎫 POST `/api/events/{id}/book`

Book a ticket for an event. Creates a pending booking request.

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Responses:**

| Status Code | Description                   | Response Type                               |
| ----------- | ----------------------------- | ------------------------------------------- |
| 202         | Accepted (booking created)    | [`BookingResponseDto`](#bookingresponsedto) |
| 404         | Event not found               | [`ErrorResponseDto`](#errorresponsedto)     |
| 409         | Conflict (no available seats) | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error         | [`ErrorResponseDto`](#errorresponsedto)     |

---

### 🔍 GET `/api/bookings/{id}`

Get booking details by ID.

**Parameters:**

| Name | In   | Type   | Required | Description                      |
| ---- | ---- | ------ | -------- | -------------------------------- |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the booking |

**Responses:**

| Status Code | Description           | Response Type                               |
| ----------- | --------------------- | ------------------------------------------- |
| 200         | Success               | [`BookingResponseDto`](#bookingresponsedto) |
| 404         | Booking not found     | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto)     |

---


## 📦 Models

### 🎫 `EventResponseDto`

Response model for event data.

| Property         | Type             | Description                          |
| ---------------- | ---------------- | ------------------------------------ |
| `id`             | `Guid`           | Unique event identifier              |
| `title`          | `string`         | Event title                          |
| `description`    | `string?`        | Optional event description           |
| `totalSeats`     | `int`            | Max amount of event's seats          |
| `availableSeats` | `int`            | Current available seats for booking  |
| `startAt`        | `DateTimeOffset` | Event start date and time            |
| `endAt`          | `DateTimeOffset` | Event end date and time              |

**Example:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Tech Conference 2024",
  "description": "Annual technology conference",
  "totalSeats": 20,
  "availableSeats": 5,
  "startAt": "2024-06-15T09:00:00+00:00",
  "endAt": "2024-06-17T18:00:00+00:00"
}
```

---

### 📄 `PaginatedEventsResponseDto`

Response model for paginated event list.

| Property     | Type                            | Description                     |
| ------------ | ------------------------------- | ------------------------------- |
| `items`      | `IEnumerable<EventResponseDto>` | List of events for current page |
| `totalCount` | `int`                           | Total number of events          |

**Example:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Tech Conference 2024",
      "description": "Annual technology conference",
      "totalSeats": 20,
      "availableSeats": 5,
      "startAt": "2024-06-15T09:00:00+00:00",
      "endAt": "2024-06-17T18:00:00+00:00"
    }
  ],
  "totalCount": 1,
  "pageIndex": 1,
  "itemsCount": 1
}
```

---

### ✨ `CreateEventRequest`

Request model for creating a new event (HTTP contract in Presentation layer).

| Property      | Type             | Required | Description                 | Constraints                   |
| ------------- | ---------------- | -------- | --------------------------- | ----------------------------- |
| `title`       | `string`         | ✅ Yes   | Event title                 | Not empty, max 200 characters |
| `description` | `string?`        | ❌ No    | Optional event description  | Max 1000 characters           |
| `totalSeats`  | `int`            | ✅ Yes   | Max amount of event's seats | Must be greater than 0        |
| `startAt`     | `DateTimeOffset` | ✅ Yes   | Event start date and time   | Must be in the future         |
| `endAt`       | `DateTimeOffset` | ✅ Yes   | Event end date and time     | Must be later than `startAt`  |

---

### 🔄 `UpdateEventRequest`

Request model for updating an existing event (HTTP contract in Presentation layer).

| Property      | Type             | Required | Description                | Constraints                   |
| ------------- | ---------------- | -------- | -------------------------- | ----------------------------- |
| `title`       | `string`         | ✅ Yes   | Event title                | Not empty, max 200 characters |
| `description` | `string?`        | ❌ No    | Optional event description | Max 1000 characters           |
| `startAt`     | `DateTimeOffset` | ✅ Yes   | Event start date and time  | Must be in the future         |
| `endAt`       | `DateTimeOffset` | ✅ Yes   | Event end date and time    | Must be later than `startAt`  |

---

### 🎟️ `BookingResponseDto`

Response model for booking data.

| Property      | Type                                   | Description                   |
| ------------- | -------------------------------------- | ----------------------------- |
| `id`          | `Guid`                                 | Unique booking identifier     |
| `eventId`     | `Guid`                                 | Unique event identifier       |
| `status`      | [`BookingStatus`](#bookingstatus-enum) | Current status of the booking |
| `createdAt`   | `DateTimeOffset`                       | Booking creation time         |
| `processedAt` | `DateTimeOffset?`                      | Booking status changed time   |

**Example:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00+00:00",
  "processedAt": null
}
```

---

### 🏷️ `BookingStatus` Enum

| Value       | Description                                     |
| ----------- | ----------------------------------------------- |
| `Pending`   | Booking request created, waiting for processing |
| `Confirmed` | Booking has been confirmed successfully         |
| `Rejected`  | Booking request was rejected (e.g., event full) |

---

### ❌ `ErrorResponseDto`

Response model for error responses returned by the global exception handling middleware.

| Property  | Type                  | Required | Description                                    |
| --------- | --------------------- | -------- | ---------------------------------------------- |
| `title`   | `string`              | ✅ Yes   | Short error description or exception message   |
| `details` | `IEnumerable<string>` | ✅ Yes   | List of detailed error messages (can be empty) |

**Example (Validation Error - 400 Bad Request):**

```json
{
  "title": "Invalid filter parameters",
  "details": ["'To' date must be later than 'From' date"]
}
```

**Example (Not Found - 404):**

```json
{
  "title": "Event with Id '3fa85f64-5717-4562-b3fc-2c963f66afa6' is not found",
  "details": []
}
```

---

### 📋 HTTP Status Codes

| Status Code               | When Occurs                                                                                                                                                                          |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 400 Bad Request           | Validation errors (missing required fields, invalid dates)<br>Invalid filter parameters (`to` date before `from`)<br>Invalid pagination parameters (page < 1, pageSize outside 5-50) |
| 404 Not Found             | Event or Booking with specified ID doesn't exist                                                                                                                                     |
| 409 Conflict              | Event does not have available seats for booking                                                                                                                                      |
| 500 Internal Server Error | Unhandled exceptions in the application                                                                                                                                              |

---


## 🏗️ Architecture

The solution follows a **Clean Architecture** with clear separation of concerns

### Layer Dependencies

```
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure                           │
│        (Repositories, DbContext, Background Services)       │
└─────────────────────────┬───────────────────────────────────┘
                          │ depends on
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Application                              │
│              (Services, DTOs, Interfaces)                   │
└─────────────────────────┬───────────────────────────────────┘
                          │ depends on
                          ▼
┌─────────────────────────────────────────────────────────────┐
│                    Domain                                   │
│              (Models, Exceptions, Constants)                │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    Presentation                             │
│           (Controllers, Middlewares, HTTP DTOs)             │
└────────┬────────────────────────────────────┬───────────────┘
         │ depends on                         │ depends on
         ▼                                    ▼
      Application                         Infrastructure
```

### 🏗️ Data Access Pattern
The application uses the Repository + Unit of Work pattern for data access

#### 📦 Repository Pattern
- `IEventRepository` - Data access operations for events
- `IBookingRepository` - Data access operations for bookings
- `IRepository<T>` - Generic repository interface with common operations
- Provides a clean abstraction over the data source

#### 🔄 Unit of Work Pattern
- `IUnitOfWork` - Coordinates multiple repositories in a single transaction
- Ensures atomic operations - all changes succeed or none are applied
- Lifecycle: Scoped per HTTP request in web applications

## 🧠 Background Processing

The API includes a background service that automatically processes pending bookings with a delay mechanism:

### Booking Processing Flow

1. **Client books an event** → Creates a pending booking
2. **Background service** (runs every 5 seconds) checks for pending bookings
3. **Processing logic:**
   - Checks if event has available seats
   - If available → Confirms booking, decrements available seats
   - If full → Rejects booking
4. **Result:** Booking status updated (Confirmed/Rejected)

### Background Service Implementation
- Uses `BackgroundService` from .NET
- Runs continuously in the background
- Implements retry logic with exponential backoff
- Handles concurrency issues with optimistic locking (RowVersion)

## 🛠️ Technology Stack

- .NET 10.0
- ASP.NET Core Web API
- EFCore, PostgreSQL
- Swagger/OpenAPI
- PostgreSQL
- Docer
- xUnit
