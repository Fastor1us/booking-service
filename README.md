# Booking API
REST API for booking events - create, manage and booking with JWT authentication and role-based authorization


## 📋 Requirements

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/)
- [Docker](https://www.docker.com/)
- CLI / IDE


## 📑 Navigation

- [🚀 Quick Start](#quick-start)
- [🗄️ Database Migrations](#️-database-migrations)
- [🔐 Authentication & Authorization](#-authentication--authorization)
  - [JWT Configuration](#jwt-configuration)
  - [Role Model](#role-model)
  - [Swagger JWT Setup](#swagger-jwt-setup)
- [🧪 Testing](#testing)
- [🌐 API Endpoints](#api-endpoints)
  - [🔑 Auth Controller](#auth-controller-apiauth)
  - [📅 Events Controller](#events-controller-apievents)
  - [📖 Booking Controller](#booking-controller-apibookings)
  - [📊 Query Parameters](#query-parameters)
- [📦 Models](#models)
  - [Auth Models](#auth-models)
  - [Event Models](#event-models)
  - [Booking Models](#booking-models)
  - [Error Models](#error-models)
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


## 🔐 Authentication & Authorization

The API uses **JWT (JSON Web Token)** authentication with role-based authorization to secure endpoints.

### JWT Configuration

The JWT settings are configured in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "BookingApi",
    "Audience": "BookingApiClient",
    "SigningKey": "your-secure-signing-key-minimum-32-characters",
    "ExpiryMinutes": 60
  }
}
```

### Role Model

The API implements a role-based access control (RBAC) model:

| Role    | Permissions |
|---------|-------------|
| **Admin** | Full access to all endpoints |
| **User** | Can book events, cancel own bookings |
| **Anonymous** | Can view events and booking details |

#### Detailed Permissions Matrix:

| Action | Anonymous | User | Admin |
|--------|-----------|------|-------|
| View events | ✅ | ✅ | ✅ |
| View event details | ✅ | ✅ | ✅ |
| View booking details | ✅ | ✅ | ✅ |
| Create event | ❌ | ❌ | ✅ |
| Update event | ❌ | ❌ | ✅ |
| Delete event | ❌ | ❌ | ✅ |
| Book event | ❌ | ✅ | ✅ |
| Cancel booking | ❌ | ✅ (own only) | ✅ (any) |

### Swagger JWT Setup

1. **Register** a new user:
   - Use the `POST /api/auth/register` endpoint
   - Provide email, username, and password
   - Default role assigned: `User`

2. **Login** to get JWT token:
   - Use the `POST /api/auth/login` endpoint
   - Receive token in response
   - Copy the token (without "Bearer" prefix)

3. **Authorize in Swagger**:
   - Click the **"Authorize"** button at the top of Swagger UI
   - Enter: `Bearer {your-token}` (replace {your-token} with the actual token)
   - Click **"Authorize"**
   - The padlock icon will appear locked on secured endpoints


## 🧪 Testing

The solution includes comprehensive testing with three separate test projects:

### 📂 Test Structure

```
tests/
├── BookingApi.Application.Tests/
├── BookingApi.Domain.Tests/
└── BookingApi.Infrastructure.Tests/
```

### BookingApi.Application.Tests ```Unit Tests```
- **Purpose:** Test individual components in isolation
- **Tools:** xUnit, Moq, In-Memory Database
- **Scope:** Services, business logic
- **Characteristics:** Fast execution, no external dependencies

### BookingApi.Domain.Tests ```Unit Tests```
- **Purpose:** Test individual components in isolation
- **Tools:** xUnit, Moq
- **Scope:** Domain logic, Validation
- **Characteristics:** Fast execution, no external dependencies

### BookingApi.Infrastructure.Tests ```Integration Tests```
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

### 🔑 Auth Controller (`/api/auth`)

| Method | Endpoint                | Description                       | Success Response | Authorization |
| ------ | ----------------------- | --------------------------------- | ---------------- | -------------- |
| POST   | `/api/auth/register`    | Register new user                 | 204 No Content   | ❌ Anonymous   |
| POST   | `/api/auth/login`       | Login and get JWT token           | 200 OK           | ❌ Anonymous   |

### 📅 Events Controller (`/api/events`)

| Method | Endpoint                | Description                       | Success Response | Authorization |
| ------ | ----------------------- | --------------------------------- | ---------------- | -------------- |
| GET    | `/api/events/{id}`      | Get event by ID                   | 200 OK           | ❌ Anonymous   |
| GET    | `/api/events`           | Get paginated events with filters | 200 OK           | ❌ Anonymous   |
| POST   | `/api/events`           | Create new event                  | 201 Created      | ✅ Admin       |
| PUT    | `/api/events/{id}`      | Update existing event             | 204 No Content   | ✅ Admin       |
| DELETE | `/api/events/{id}`      | Delete event                      | 204 No Content   | ✅ Admin       |
| POST   | `/api/events/{id}/book` | Book an event                     | 202 Accepted     | ✅ User/Admin  |

### 📖 Booking Controller (`/api/bookings`)

| Method | Endpoint                | Description       | Success Response | Authorization |
| ------ | ----------------------- | ----------------- | ---------------- | -------------- |
| GET    | `/api/bookings/{id}`    | Get booking by ID | 200 OK           | ❌ Anonymous   |
| POST   | `/api/bookings/{id}/cancel` | Cancel booking | 204 No Content   | ✅ User/Admin  |

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

### 🔑 POST `/api/auth/register`

Register a new user account.

**Request Body:**

| Name       | In   | Type          | Required | Description         |
| ---------- | ---- | ------------- | -------- | ------------------- |
| `dto`      | body | `CreateUserDto` | ✅ Yes  | User registration data |

**CreateUserDto:**

| Property   | Type     | Required | Description                  | Constraints          |
| ---------- | -------- | -------- | ---------------------------- | -------------------- |
| `username` | `string` | ✅ Yes   | Unique username              | 3-50 characters      |
| `email`    | `string` | ✅ Yes   | Valid email address          | Email format         |
| `password` | `string` | ✅ Yes   | User password                | Minimum 6 characters |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 204         | No Content (success)  | -                                       |
| 400         | Validation error      | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 🔑 POST `/api/auth/login`

Authenticate user and receive JWT token.

**Request Body:**

| Name       | In   | Type          | Required | Description         |
| ---------- | ---- | ------------- | -------- | ------------------- |
| `dto`      | body | `LoginUserDto` | ✅ Yes  | User login data |

**LoginUserDto:**

| Property   | Type     | Required | Description         |
| ---------- | -------- | -------- | ------------------- |
| `username` | `string` | ✅ Yes   | User's username     |
| `password` | `string` | ✅ Yes   | User's password     |

**Responses:**

| Status Code | Description           | Response Type                               |
| ----------- | --------------------- | ------------------------------------------- |
| 200         | Success               | [`LoginResponseDto`](#loginresponsedto)     |
| 400         | Invalid credentials   | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto)     |

**Example Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
}
```

---

### 🎯 GET `/api/events/{id}`

Get a single event by its unique identifier.

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Authorization:** ❌ Anonymous

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 200         | Success               | [`EventResponseDto`](#eventresponsedto) |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 📋 GET `/api/events`

Get paginated list of events with optional filtering.

**Authorization:** ❌ Anonymous

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

**Authorization:** ✅ Admin

**Request Body:**

| Name       | In   | Type                                  | Required | Description         |
| ---------- | ---- | ------------------------------------- | -------- | ------------------- |
| `dto`      | body | [`CreateEventDto`](#createeventdto)   | ✅ Yes  | Event creation data |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 201         | Created               | [`EventResponseDto`](#eventresponsedto) |
| 400         | Validation error      | [`ErrorResponseDto`](#errorresponsedto) |
| 401         | Unauthorized          | [`ErrorResponseDto`](#errorresponsedto) |
| 403         | Forbidden             | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### ✏️ PUT `/api/events/{id}`

Update an existing event.

**Authorization:** ✅ Admin

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
| 401         | Unauthorized          | [`ErrorResponseDto`](#errorresponsedto) |
| 403         | Forbidden             | [`ErrorResponseDto`](#errorresponsedto) |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 🗑️ DELETE `/api/events/{id}`

Delete an event.

**Authorization:** ✅ Admin

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 204         | No Content (success)  | -                                       |
| 401         | Unauthorized          | [`ErrorResponseDto`](#errorresponsedto) |
| 403         | Forbidden             | [`ErrorResponseDto`](#errorresponsedto) |
| 404         | Event not found       | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 🎫 POST `/api/events/{id}/book`

Book a ticket for an event. Creates a pending booking request.

**Authorization:** ✅ User or Admin

**Parameters:**

| Name | In   | Type   | Required | Description                    |
| ---- | ---- | ------ | -------- | ------------------------------ |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the event |

**Responses:**

| Status Code | Description                   | Response Type                               |
| ----------- | ----------------------------- | ------------------------------------------- |
| 202         | Accepted (booking created)    | [`BookingResponseDto`](#bookingresponsedto) |
| 401         | Unauthorized                  | [`ErrorResponseDto`](#errorresponsedto)     |
| 403         | Forbidden                     | [`ErrorResponseDto`](#errorresponsedto)     |
| 404         | Event not found               | [`ErrorResponseDto`](#errorresponsedto)     |
| 409         | Conflict (no available seats) | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error         | [`ErrorResponseDto`](#errorresponsedto)     |

---

### 🔍 GET `/api/bookings/{id}`

Get booking details by ID.

**Authorization:** ❌ Anonymous

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

### ❌ POST `/api/bookings/{id}/cancel`

Cancel a booking.

**Authorization:** ✅ User (own bookings) or Admin (any)

**Parameters:**

| Name | In   | Type   | Required | Description                      |
| ---- | ---- | ------ | -------- | -------------------------------- |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the booking |

**Responses:**

| Status Code | Description           | Response Type                               |
| ----------- | --------------------- | ------------------------------------------- |
| 204         | No Content (success)  | -                                           |
| 401         | Unauthorized          | [`ErrorResponseDto`](#errorresponsedto)     |
| 403         | Forbidden (not owner) | [`ErrorResponseDto`](#errorresponsedto)     |
| 404         | Booking not found     | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto)     |

---


## 📦 Models

### 🔑 Auth Models

#### `CreateUserDto`

Request model for user registration.

| Property   | Type     | Required | Description                  | Constraints          |
| ---------- | -------- | -------- | ---------------------------- | -------------------- |
| `username` | `string` | ✅ Yes   | Unique username              | 3-50 characters      |
| `email`    | `string` | ✅ Yes   | Valid email address          | Email format         |
| `password` | `string` | ✅ Yes   | User password                | Minimum 6 characters |

**Example:**

```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "SecurePassword123"
}
```

---

#### `LoginUserDto`

Request model for user login.

| Property   | Type     | Required | Description         |
| ---------- | -------- | -------- | ------------------- |
| `username` | `string` | ✅ Yes   | User's username     |
| `password` | `string` | ✅ Yes   | User's password     |

**Example:**

```json
{
  "username": "johndoe",
  "password": "SecurePassword123"
}
```

---

#### `LoginResponseDto`

Response model for successful login.

| Property | Type     | Description              |
| -------- | -------- | ------------------------ |
| `token`  | `string` | JWT access token         |

**Example:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJqb2huZG9lIiwibmFtZSI6IkpvaG4gRG9lIiwicm9sZSI6IlVzZXIiLCJleHAiOjE3MDQyMzQ1Njd9.abc123def456"
}
```

---

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

### ✨ `CreateEventDto`

Request model for creating a new event.

| Property      | Type             | Required | Description                 | Constraints                   |
| ------------- | ---------------- | -------- | --------------------------- | ----------------------------- |
| `title`       | `string`         | ✅ Yes   | Event title                 | Not empty, max 200 characters |
| `description` | `string?`        | ❌ No    | Optional event description  | Max 1000 characters           |
| `totalSeats`  | `int`            | ✅ Yes   | Max amount of event's seats | Must be greater than 0        |
| `startAt`     | `DateTimeOffset` | ✅ Yes   | Event start date and time   | Must be in the future         |
| `endAt`       | `DateTimeOffset` | ✅ Yes   | Event end date and time     | Must be later than `startAt`  |

---

### 🔄 `UpdateEventDto`

Request model for updating an existing event.

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
| `userId`      | `Guid`                                 | Unique user identifier        |
| `username`    | `string`                               | Username who made booking     |
| `status`      | [`BookingStatus`](#bookingstatus-enum) | Current status of the booking |
| `createdAt`   | `DateTimeOffset`                       | Booking creation time         |
| `processedAt` | `DateTimeOffset?`                      | Booking status changed time   |

**Example:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "userId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "username": "johndoe",
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
| `Cancelled` | Booking was cancelled by user                   |

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
| 400 Bad Request           | Validation errors (missing required fields, invalid dates)<br>Invalid filter parameters (`to` date before `from`)<br>Invalid pagination parameters (page < 1, pageSize outside 5-50)<br>Invalid credentials |
| 401 Unauthorized          | Missing or invalid JWT token<br>Token expired                                                                                                                                        |
| 403 Forbidden             | User role lacks required permissions<br>User tries to access resource they don't own (e.g., cancel another user's booking)                                                            |
| 404 Not Found             | Event or Booking with specified ID doesn't exist                                                                                                                                     |
| 409 Conflict              | Event does not have available seats for booking                                                                                                                                      |
| 500 Internal Server Error | Unhandled exceptions in the application                                                                                                                                              |

---


## 🧠 Background Processing

The API includes a background service that automatically processes pending bookings with a delay mechanism:

### Booking Processing Flow

1. **Client books an event** → Creates a pending booking
2. **Background service** → Checks for pending bookings
3. **Processing logic:** → Confirms bookings
4. **Result:** Bookings status updated

### Background Service Implementation
- Uses `BackgroundService` from .NET
- Runs continuously in the background
- Handles concurrency issues with optimistic locking (RowVersion)


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
- `IUserRepository` - Data access operations for users
- `IRepository<T>` - Generic repository interface with common operations
- Provides a clean abstraction over the data source

#### 🔄 Unit of Work Pattern
- `IUnitOfWork` - Coordinates multiple repositories in a single transaction
- Ensures atomic operations - all changes succeed or none are applied
- Lifecycle: Scoped per HTTP request in web applications


## 🛠️ Technology Stack

- .NET 10.0
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- JWT Authentication & Authorization
- Swagger/OpenAPI
- Docker & Testcontainers
- xUnit, Moq
- Background Services