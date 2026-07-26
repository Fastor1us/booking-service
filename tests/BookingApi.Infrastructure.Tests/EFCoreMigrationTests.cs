using BookingApi.Domain.Constants;
using BookingApi.Infrastructure.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace BookingApi.Infrastructure.Tests;

public class MigrationTests : PostgreSqlBase
{
    [Fact]
    public async Task Migrations_ShouldApplySuccessfully()
    {
        // Arrange
        var context = CreateContext();
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();

        // Act
        var tableNames = await context.Database
            .SqlQueryRaw<string>(@"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' 
                AND table_type = 'BASE TABLE';")
            .ToListAsync();

        // Assert
        Assert.Empty(pendingMigrations);
        Assert.Contains("events", tableNames);
        Assert.Contains("bookings", tableNames);
    }

    [Fact]
    public async Task Migrations_ShouldCreateAllConstraints()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var constraints = await context.Database
            .SqlQueryRaw<ConstraintInfo>(@"
                SELECT 
                    tc.table_name,
                    tc.constraint_name,
                    tc.constraint_type,
                    cc.check_clause
                FROM information_schema.table_constraints tc
                LEFT JOIN information_schema.check_constraints cc 
                    ON tc.constraint_name = cc.constraint_name
                WHERE tc.table_schema = 'public'
                AND tc.table_name IN ('events', 'bookings');")
            .ToListAsync();
        var eventConstraints = constraints.Where(c => c.table_name == "events").ToList();
        var bookingConstraints = constraints.Where(c => c.table_name == "bookings").ToList();

        // Assert
        Assert.Contains(eventConstraints, c => c.constraint_type == "PRIMARY KEY");
        Assert.Contains(eventConstraints, c => c.constraint_type == "CHECK");
        Assert.Contains(bookingConstraints, c => c.constraint_type == "PRIMARY KEY");
        Assert.Contains(bookingConstraints, c => c.constraint_type == "FOREIGN KEY");
        Assert.Contains(bookingConstraints, c => c.constraint_type == "CHECK");
    }

    [Fact]
    public async Task Migrations_ShouldHaveCorrectTableStructure()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var columns = await context.Database
            .SqlQueryRaw<ColumnInfo>(@"
                SELECT 
                    table_name,
                    column_name,
                    data_type,
                    character_maximum_length,
                    is_nullable
                FROM information_schema.columns
                WHERE table_schema = 'public'
                AND table_name IN ('events', 'bookings')
                ORDER BY table_name, ordinal_position;")
            .ToListAsync();
        var eventColumns = columns.Where(c => c.table_name == "events").ToList();
        var bookingColumns = columns.Where(c => c.table_name == "bookings").ToList();

        // Assert
        Assert.Contains(eventColumns, c =>
            c.column_name == "id" && c.data_type == "uuid" && c.is_nullable == "NO");
        Assert.Contains(eventColumns, c =>
            c.column_name == "title" && c.data_type == "character varying" &&
            c.character_maximum_length == EventConstants.TitleMaxLength && c.is_nullable == "NO");
        Assert.Contains(eventColumns, c =>
            c.column_name == "total_seats" && c.data_type == "integer" && c.is_nullable == "NO");
        Assert.Contains(eventColumns, c =>
            c.column_name == "available_seats" && c.data_type == "integer" && c.is_nullable == "NO");
        Assert.Contains(bookingColumns, c =>
            c.column_name == "id" && c.data_type == "uuid" && c.is_nullable == "NO");
        Assert.Contains(bookingColumns, c =>
            c.column_name == "event_id" && c.data_type == "uuid" && c.is_nullable == "NO");
        Assert.Contains(bookingColumns, c =>
            c.column_name == "status" && c.data_type == "character varying" &&
            c.character_maximum_length == 20 && c.is_nullable == "NO");
        Assert.Contains(bookingColumns, c =>
            c.column_name == "created_at" && c.data_type == "timestamp with time zone" && c.is_nullable == "NO");
    }

    [Fact]
    public async Task Migrations_ShouldCreateAllIndexes()
    {
        // Arrange
        var context = CreateContext();

        // Act
        var indexes = await context.Database
            .SqlQueryRaw<IndexInfo>(@"
                SELECT
                    tablename,
                    indexname
                FROM pg_indexes
                WHERE schemaname = 'public'
                AND tablename IN ('events', 'bookings');")
            .ToListAsync();
        var eventIndexes = indexes.Where(i => i.tablename == "events").ToList();
        var bookingIndexes = indexes.Where(i => i.tablename == "bookings").ToList();

        // Assert
        Assert.Contains(eventIndexes, i => i.indexname == "ix_events_title");
        Assert.Contains(eventIndexes, i => i.indexname == "ix_events_start_at_end_at");
        Assert.Contains(eventIndexes, i => i.indexname == "ix_events_available_seats");
        Assert.Contains(bookingIndexes, i => i.indexname == "ix_bookings_event_id");
        Assert.Contains(bookingIndexes, i => i.indexname == "ix_bookings_status");
        Assert.Contains(bookingIndexes, i => i.indexname == "ix_bookings_event_id_status");
        Assert.Contains(bookingIndexes, i => i.indexname == "ix_bookings_created_at");
    }
}

public class ConstraintInfo
{
    public string table_name { get; set; } = string.Empty;
    public string constraint_name { get; set; } = string.Empty;
    public string constraint_type { get; set; } = string.Empty;
    public string? check_clause { get; set; }
}

public class ColumnInfo
{
    public string table_name { get; set; } = string.Empty;
    public string column_name { get; set; } = string.Empty;
    public string data_type { get; set; } = string.Empty;
    public int? character_maximum_length { get; set; }
    public string is_nullable { get; set; } = string.Empty;
}

public class IndexInfo
{
    public string tablename { get; set; } = string.Empty;
    public string indexname { get; set; } = string.Empty;
}
