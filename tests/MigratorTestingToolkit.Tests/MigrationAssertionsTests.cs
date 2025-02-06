using System;
using System.Collections.Generic;
using Xunit;
using MigratorTestingToolkit.Assertions;
using MigratorTestingToolkit.Utils;

namespace MigratorTestingToolkit.Tests;

/// <summary>
/// Unit tests for <see cref="MigrationAssertions"/>.
/// Ensures assertions work correctly against database schema.
/// </summary>
public class MigrationAssertionsTests : IDisposable
{
    private readonly SqliteTestHelper _sqliteTestHelper;
    private readonly MigrationAssertions _assertions;

    /// <summary>
    /// Initializes a new test instance with an in-memory database.
    /// </summary>
    public MigrationAssertionsTests()
    {
        _sqliteTestHelper = new SqliteTestHelper();
        _assertions = new MigrationAssertions();
        _assertions.Setup();
    }

    /// <summary>
    /// Ensures resources are cleaned up after tests.
    /// </summary>
    public void Dispose()
    {
        _assertions.Dispose();
    }

    [Fact]
    public void TableExists_ShouldReturnTrue_WhenTableExists()
    {
        _sqliteTestHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT)");
        bool exists = _assertions.TableExists("Users");
        Assert.True(exists);
    }

    [Fact]
    public void TableExists_ShouldReturnFalse_WhenTableDoesNotExist()
    {
        bool exists = _assertions.TableExists("NonExistentTable");
        Assert.False(exists);
    }

    [Fact]
    public void TableHasColumns_ShouldReturnTrue_WhenSchemaMatches()
    {
        _sqliteTestHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");

        bool matches = _assertions.TableHasColumns("Users", new Dictionary<string, string>
        {
            { "Id", "INTEGER" },
            { "Name", "TEXT" }
        });

        Assert.True(matches);
    }

    [Fact]
    public void TableHasColumns_ShouldReturnFalse_WhenSchemaDoesNotMatch()
    {
        _sqliteTestHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");

        bool matches = _assertions.TableHasColumns("Users", new Dictionary<string, string>
        {
            { "Id", "INTEGER" },
            { "Age", "INTEGER" }
        });

        Assert.False(matches);
    }

    [Fact]
    public void TableHasUniqueConstraint_ShouldReturnTrue_WhenUniqueIndexExists()
    {
        _sqliteTestHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Email TEXT UNIQUE)");

        bool hasUnique = _assertions.TableHasUniqueConstraint("Users", "Email");
        Assert.True(hasUnique);
    }

    [Fact]
    public void TableHasUniqueConstraint_ShouldReturnFalse_WhenNoUniqueIndexExists()
    {
        _sqliteTestHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Email TEXT)");

        bool hasUnique = _assertions.TableHasUniqueConstraint("Users", "Email");
        Assert.False(hasUnique);
    }
}
