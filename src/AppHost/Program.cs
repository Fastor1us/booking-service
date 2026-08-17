var builder = DistributedApplication.CreateBuilder(args);

var config = builder.Configuration;

var usersDbName = config["Database:Users:DbName"] ?? "usersdb";
var usersUser = config["Database:Users:User"] ?? "postgres";
var usersPassword = config["Database:Users:Password"] ?? "postgres";

var eventsDbName = config["Database:Events:DbName"] ?? "eventsdb";
var eventsUser = config["Database:Events:User"] ?? "postgres";
var eventsPassword = config["Database:Events:Password"] ?? "postgres";

var bookingsDbName = config["Database:Bookings:DbName"] ?? "bookingsdb";
var bookingsUser = config["Database:Bookings:User"] ?? "postgres";
var bookingsPassword = config["Database:Bookings:Password"] ?? "postgres";

// PostgreSQL
var usersDb = builder.AddPostgres("postgres-users")
    .WithImage("postgres:16-alpine")
    .WithEnvironment("POSTGRES_DB", usersDbName)
    .WithEnvironment("POSTGRES_USER", usersUser)
    .WithEnvironment("POSTGRES_PASSWORD", usersPassword)
    .WithDataVolume()
    .AddDatabase("usersdb");

var eventsDb = builder.AddPostgres("postgres-events")
    .WithImage("postgres:16-alpine")
    .WithEnvironment("POSTGRES_DB", eventsDbName)
    .WithEnvironment("POSTGRES_USER", eventsUser)
    .WithEnvironment("POSTGRES_PASSWORD", eventsPassword)
    .WithDataVolume()
    .AddDatabase("eventsdb");

var bookingsDb = builder.AddPostgres("postgres-bookings")
    .WithImage("postgres:16-alpine")
    .WithEnvironment("POSTGRES_DB", bookingsDbName)
    .WithEnvironment("POSTGRES_USER", bookingsUser)
    .WithEnvironment("POSTGRES_PASSWORD", bookingsPassword)
    .WithDataVolume()
    .AddDatabase("bookingsdb");

// Kafka
var kafka = builder.AddKafka("kafka")
    .WithImage("confluentinc/cp-kafka:latest")
    .WithEnvironment("KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR", "1")
    .WithEnvironment("KAFKA_TRANSACTION_STATE_LOG_MIN_ISR", "1")
    .WithEnvironment("KAFKA_CREATE_TOPICS", "booking-confirmed:1:1");

var jwtIssuer = config["Jwt:Issuer"] ?? "BookingPlatform";
var jwtAudience = config["Jwt:Audience"] ?? "BookingPlatformClient";
var jwtSigningKey = config["Jwt:SigningKey"] ?? "your-secure-signing-key-minimum-32-characters";
var jwtExpiryMinutes = config["Jwt:ExpiryMinutes"] ?? "60";

// User Service
var userService = builder.AddProject<Projects.UserService_Presentation>("userservice")
    .WithReference(usersDb)
    .WithEnvironment("ConnectionStrings__DefaultConnection", usersDb.Resource.ConnectionStringExpression)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Jwt__ExpiryMinutes", jwtExpiryMinutes)
    .WithHttpEndpoint(port: 5001, name: "http")
    .WithExternalHttpEndpoints();

// Event Service
var eventService = builder.AddProject<Projects.EventService_Presentation>("eventservice")
    .WithReference(eventsDb)
    .WithEnvironment("ConnectionStrings__DefaultConnection", eventsDb.Resource.ConnectionStringExpression)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Kafka__BootstrapServers", "localhost:9092")
    .WithEnvironment("Kafka__ConsumerGroup", "events-group")
    .WithHttpEndpoint(port: 5002, name: "http")
    .WithExternalHttpEndpoints()
    .WaitFor(kafka);

// Booking Service
var bookingService = builder.AddProject<Projects.BookingService_Presentation>("bookingservice")
    .WithReference(bookingsDb)
    .WithEnvironment("ConnectionStrings__DefaultConnection", bookingsDb.Resource.ConnectionStringExpression)
    .WithEnvironment("Jwt__Issuer", jwtIssuer)
    .WithEnvironment("Jwt__Audience", jwtAudience)
    .WithEnvironment("Jwt__SigningKey", jwtSigningKey)
    .WithEnvironment("Kafka__BootstrapServers", "localhost:9092")
    .WithEnvironment("Kafka__ConsumerGroup", "bookings-group")
    .WithHttpEndpoint(port: 5003, name: "http")
    .WithExternalHttpEndpoints()
    .WaitFor(kafka);

userService.WaitFor(usersDb);
eventService.WaitFor(eventsDb);
bookingService.WaitFor(bookingsDb);

// EventService зависит от Kafka, а BookingService от EventService
bookingService.WithReference(eventService);
eventService.WithReference(kafka);
bookingService.WithReference(kafka);

builder.Build().Run();
