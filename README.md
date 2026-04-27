# Booking API

REST API to booking events

Solution includes following projects:
- ./api/BookingApi.csproj
- ./tests/BookingTests.csproj

## 📋 Requirements

- [.NET 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- CLI / IDE

## 🚀 Quick start

- Recovering dependencies
```bash
dotnet restore --project ./src/BookingApi.csproj 
```
- Build project
```bash
dotnet build --project ./src/BookingApi.csproj 
```
- Run project
```bash
dotnet run --project ./src/BookingApi.csproj 
```

## 🧪 Testing
Use [Swagger UI](http://localhost:5142/swagger/index.html) after ```dotnet run --project ./src/BookingApi.csproj```

- Run project with Tests
```bash
dotnet test
```
