# Booking API
REST API for booking events - create, manage and booking

## 📋 Requirements

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- CLI / IDE

## 📑 Navigation

- [🚀 Quick Start](#quick-start)
- [🧪 Testing](#testing)
- [🌐 API Endpoints](#api-endpoints)
  - [📅 Events Controller](#events-controller-apievents)
  - [📖 Booking Controller](#booking-controller-apibookings)
  - [📊 Query Parameters](#query-parameters)
- [📦 Models](#models)
  - [EventResponseDto](#eventresponsedto)
  - [PaginatedEventsResponseDto](#paginatedeventsresponsedto)
  - [PostEventDto](#posteventdto)
  - [PutEventDto](#puteventdto)
  - [BookingResponseDto](#bookingresponsedto)
  - [BookingStatus Enum](#bookingstatus-enum)
  - [ErrorResponseDto](#errorresponsedto)
- [📋 HTTP Status Codes](#http-status-codes)
- [🏗️ Architecture](#architecture)
- [🔒 Concurrency & Synchronization Primitives](#concurrency-&-synchronization-primitives)
- [🧠 Background Processing](#background-processing)
- [🛠️ Technology Stack](#technology-stack)
- [📝 Notes](#notes)

## 🚀 Quick Start

### Restore dependencies

```bash
dotnet restore ./src/BookingApi.csproj
```

### Build project

```bash
dotnet build ./src/BookingApi.csproj
```

### Run project

```bash
dotnet run --project ./src/BookingApi.csproj
```

After running, access [Swagger UI](http://localhost:5142/swagger/index.html)

## 🧪 Testing

### Run tests

```bash
dotnet test
```

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

| Status Code | Description               | Response Type                                               |
| ----------- | ------------------------- | ----------------------------------------------------------- |
| 200         | Success                   | [`PaginatedEventsResponseDto`](#paginatedeventsresponsedto) |
| 400         | Invalid filter/pagination | [`ErrorResponseDto`](#errorresponsedto)                     |
| 500         | Internal server error     | [`ErrorResponseDto`](#errorresponsedto)                     |

---

### ➕ POST `/api/events`

Create a new event.

**Request Body:**

| Name       | In   | Type                            | Required | Description         |
| ---------- | ---- | ------------------------------- | -------- | ------------------- |
| `eventDto` | body | [`PostEventDto`](#posteventdto) | ✅ Yes   | Event creation data |

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

| Name       | In   | Type                          | Required | Description        |
| ---------- | ---- | ----------------------------- | -------- | ------------------ |
| `eventDto` | body | [`PutEventDto`](#puteventdto) | ✅ Yes   | Updated event data |

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

| Status Code | Description                  | Response Type                               |
| ----------- | ---------------------------- | ------------------------------------------- |
| 202         | Accepted (booking created)   | [`BookingResponseDto`](#bookingresponsedto) |
| 404         | Event not found              | [`ErrorResponseDto`](#errorresponsedto)     |
| 500         | Internal server error        | [`ErrorResponseDto`](#errorresponsedto)     |

---

### 🔍 GET `/api/bookings/{id}`

Get booking details by ID.

**Parameters:**

| Name | In   | Type   | Required | Description                      |
| ---- | ---- | ------ | -------- | -------------------------------- |
| `id` | path | `guid` | ✅ Yes   | Unique identifier of the booking |

**Responses:**

| Status Code | Description           | Response Type                           |
| ----------- | --------------------- | --------------------------------------- |
| 200         | Success               | [`Booking`](#booking-model)             |
| 404         | Booking not found     | [`ErrorResponseDto`](#errorresponsedto) |
| 500         | Internal server error | [`ErrorResponseDto`](#errorresponsedto) |

---

### 📦 Models

### 🎫 `EventResponseDto`

Response model for event data.

| Property         | Type       |Description                          |
| ---------------- | ---------- | ----------------------------------- |
| `id`             | `Guid`     | Unique event identifier             |
| `title`          | `string`   | Event title                         |
| `description`    | `string`   | Optional event description          |
| `totalSeats`     | `int`      | Max amount of event's seats         |
| `availableSeats` | `int`      | Current available seats to booking  |
| `startAt`        | `DateTime` | Event start date and time           |
| `endAt`          | `DateTime` | Event end date and time             |

**Example:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "Tech Conference 2024",
  "totalSeats": 20,
  "availableSeats": 5,
  "description": "Annual technology conference",
  "startAt": "2024-06-15T09:00:00Z",
  "endAt": "2024-06-17T18:00:00Z"
}
```

---

### 📄 `PaginatedEventsResponseDto`

Response model for paginated event list.

| Property     | Type                            | Description                     |
| ------------ | ------------------------------- | ------------------------------- |
| `items`      | `IEnumerable<EventResponseDto>` | List of events for current page |
| `totalCount` | `int`                           | Total number of events          |
| `pageIndex`  | `int`                           | Current page number (1-based)   |
| `itemsCount` | `int`                           | Number of items on this page    |

**Example:**

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "Tech Conference 2024",
      "description": "Annual technology conference",
      "startAt": "2024-06-15T09:00:00Z",
      "endAt": "2024-06-17T18:00:00Z"
    }
  ],
  "totalCount": 25,
  "pageIndex": 2,
  "itemsCount": 10
}
```

---

### ✨ `PostEventDto`

Request model for creating a new event.

| Property      | Type       | Required | Description                 | Constraints                   |
| ------------- | ---------- | -------- | --------------------------- | ----------------------------- |
| `title`       | `string`   | ✅ Yes   | Event title                 | Not empty, max 200 characters |
| `description` | `string`   | ❌ No    | Optional event description  | Max 1000 characters           |
| `totalSeats`  | `int`      | ✅ Yes   | Max amount of event's seats | Must be more than 0           |
| `startAt`     | `DateTime` | ✅ Yes   | Event start date and time   | Must be in the future         |
| `endAt`       | `DateTime` | ✅ Yes   | Event end date and time     | Must be later than `startAt`  |

---

### 🔄 `PutEventDto`

Request model for updating an existing event.

| Property      | Type       | Required | Description                | Constraints                   |
| ------------- | ---------- | -------- | -------------------------- | ----------------------------- |
| `title`       | `string`   | ✅ Yes   | Event title                | Not empty, max 200 characters |
| `description` | `string`   | ❌ No    | Optional event description | Max 1000 characters           |
| `startAt`     | `DateTime` | ✅ Yes   | Event start date and time  | Must be in the future         |
| `endAt`       | `DateTime` | ✅ Yes   | Event end date and time    | Must be later than `startAt`  |

---

### 🎟️ `BookingResponseDto`

Response model for booking creation (returned from `POST /api/events/{id}/book`).

| Property      | Type                                   | Description                   |
| ------------- | -------------------------------------- | ----------------------------- |
| `Id`          | `Guid`                                 | Unique booking identifier     |
| `eventId`     | `Guid`                                 | Unique event identifier       |
| `status`      | [`BookingStatus`](#bookingstatus-enum) | Current status of the booking |
| `createdAt`   | `DateTime`                             | Booking creation time         |
| `processedAt` | `DateTime`                             | Booking status changed time   |

**Example:**

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
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

Response model for error responses returned by the global exception handling middleware

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

---

### HTTP Status Codes

| Status Code               | When Occurs                                                                                                                                                                          |
| ------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| 400 Bad Request           | Validation errors (missing required fields, invalid dates)<br>Invalid filter parameters (`to` date before `from`)<br>Invalid pagination parameters (page < 1, pageSize outside 5-50) |
| 404 Not Found             | Event or Booking with specified ID doesn't exist                                                                                                                                     |
| 409 Conflict              | Event does not have available seats to booking                                                                                                                                       |
| 500 Internal Server Error | Unhandled exceptions in the application                                                                                                                                              |

### Example Error Response (400 Bad Request)

```json
{
  "title": "Invalid filter parameters",
  "details": ["'To' date must be later than 'From' date"]
}
```

```json
{
  "title": "One or more validation errors occurred.",
  "details": ["EndAt must be later than StartAt", "Title is required"]
}
```

### Example Error Response (404 Not Found)

```json
{
  "title": "Event with id '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found",
  "details": []
}
```

## 🏗️ Architecture

The solution follows a layered architecture:

- **Presentation Layer** (`BookingApi.Presentation`) - Controllers, DTOs, Filters, Middlewares
- **Application Layer** (`BookingApi.Application`) - Services, Interfaces
- **Domain Layer** (`BookingApi.Domain`) - Models, Exceptions
- **Infrastructure Layer** (`BookingApi.Infrastructure`) - Repositories (In-memory implementation)

## 🔒 Concurrency & Synchronization Primitives

The application uses synchronization primitives and concurrent collections to prevent race conditions and data inconsistency

## 🧠 Background Processing

The API includes a background service that automatically processes pending bookings with a delay mechanism

## 🛠️ Technology Stack

- .NET 10.0
- ASP.NET Core Web API
- Swagger/OpenAPI
- xUnit (testing)
- In-memory repository (development)

## 📝 Notes

- The API uses an in-memory database by default. All data is lost when the application stops.
- Uncomment the test data population block in `EventInMemoryRepository.cs` to pre-populate events for testing.
- Comprehensive validation is implemented both at the DTO level and service layer.
