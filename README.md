# Booking API
REST API for managing event bookings

## 📋 Requirements
- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- CLI / IDE

## 🚀 Quick Start
### Restore dependencies
```bash
dotnet restore --project ./src/BookingApi.csproj
```
### Build project
```bash
dotnet build --project ./src/BookingApi.csproj
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

## 🌑 API Endpoints
### Events Controller (`/api/events`)
| Method | Endpoint | Description | Success Response |
|--------|----------|-------------|------------------|
| GET | `/api/events/{id}` | Get event by ID | 200 OK |
| GET | `/api/events` | Get paginated events with filters | 200 OK |
| POST | `/api/events` | Create new event | 201 Created |
| PUT | `/api/events/{id}` | Update existing event | 204 No Content |
| DELETE | `/api/events/{id}` | Delete event | 204 No Content |

### GET `/api/events` - Query Parameters

| Parameter | Type | Description | Constraints | Default |
|-----------|------|-------------|-------------|---------|
| `title` | string | Filter by title (case-insensitive partial match) | - | null |
| `from` | DateTime | Filter events starting after this date | Must be before `to` | null |
| `to` | DateTime | Filter events ending before this date | Must be after `from` | null |
| `page` | int | Page number | ≥ 1 | 1 |
| `pageSize` | int | Items per page | 5-50 | 10 |

**Example Request:**
```
GET /api/events?title=workshop&from=2024-01-01&to=2024-12-31&page=2&pageSize=20
```

### Request/Response Models

#### POST `/api/events` & PUT `/api/events/{id}` Body

```json
{
  "title": "string (required)",
  "description": "string (optional)",
  "startAt": "2024-01-01T10:00:00Z (required)",
  "endAt": "2024-01-01T12:00:00Z (required)"
}
```

**Validation Rules:**
- `endAt` must be later than `startAt`
- All required fields must be provided and non-empty

#### GET `/api/events/{id}` & POST `/api/events` Response

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "title": "string",
  "description": "string",
  "startAt": "2024-01-01T10:00:00Z",
  "endAt": "2024-01-01T12:00:00Z"
}
```

#### GET `/api/events` Response (Paginated)

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "title": "string",
      "description": "string",
      "startAt": "2024-01-01T10:00:00Z",
      "endAt": "2024-01-01T12:00:00Z"
    }
  ],
  "totalCount": 1,
  "pageIndex": 1,
  "itemsCount": 1
}
```

## ❌ Error Handling

All errors return a consistent JSON format with appropriate HTTP status codes.

### Error Response Format

```json
{
  "title": "Error description",
  "details": ["Additional error details"]
}
```

### HTTP Status Codes

| Status Code | Description | When Occurs |
|-------------|-------------|-------------|
| 400 Bad Request | Invalid request data | - Validation errors (missing required fields, invalid dates)<br>- Invalid filter parameters (`to` date before `from`)<br>- Invalid pagination parameters (page < 1, pageSize outside 5-50) |
| 404 Not Found | Resource not found | Event with specified ID doesn't exist |
| 409 Conflict | Resource conflict | ID conflict |
| 500 Internal Server Error | Server error | Unhandled exceptions in the application |

### Example Error Response (400 Bad Request)

```json
{
  "title": "Invalid filter parameters",
  "details": [
    "'To' date must be later than 'From' date"
  ]
}
```

```json
{
  "title": "One or more validation errors occurred.",
  "details": [
    "EndAt must be later than StartAt",
    "Title is required"
  ]
}
```

### Example Error Response (404 Not Found)

```json
{
  "title": "Event with id '3fa85f64-5717-4562-b3fc-2c963f66afa6' was not found",
  "details": []
}
```

### Example Error Response (409 Conflict)

```json
{
  "title": "Event with id '3fa85f64-5717-4562-b3fc-2c963f66afa6' already exist",
  "details": []
}
```

## 🏗️ Architecture

The solution follows a layered architecture:

- **Presentation Layer** (`BookingApi.Presentation`) - Controllers, DTOs, Filters, Middlewares
- **Application Layer** (`BookingApi.Application`) - Services, Interfaces
- **Domain Layer** (`BookingApi.Domain`) - Models, Exceptions
- **Infrastructure Layer** (`BookingApi.Infrastructure`) - Repositories (In-memory implementation)

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
