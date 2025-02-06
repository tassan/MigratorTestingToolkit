using System;
using System.Collections.Generic;
using Xunit;
using MigratorTestingToolkit.Utils;

namespace MigratorTestingToolkit.Tests;

/// <summary>
/// Unit tests for <see cref="SqliteTestHelper"/>.
/// </summary>
public class SqliteTestHelperTests : IDisposable
{
    private readonly SqliteTestHelper _sqliteHelper;

    /// <summary>
    /// Initializes a new test instance with a fresh SQLite in-memory database.
    /// </summary>
    public SqliteTestHelperTests()
    {
        _sqliteHelper = new SqliteTestHelper();
    }

    /// <summary>
    /// Ensures that the database connection is properly closed after tests.
    /// </summary>
    public void Dispose()
    {
        _sqliteHelper.Dispose();
    }

    [Fact]
    public void TableExists_ShouldReturnFalse_WhenTableDoesNotExist()
    {
        bool exists = _sqliteHelper.TableExists("NonExistentTable");
        Assert.False(exists);
    }

    [Fact]
    public void ExecuteNonQuery_ShouldCreateTable()
    {
        _sqliteHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT)");

        bool exists = _sqliteHelper.TableExists("Users");
        Assert.True(exists);
    }

    [Fact]
    public void ExecuteScalar_ShouldReturnInsertedValue()
    {
        _sqliteHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT)");
        _sqliteHelper.ExecuteNonQuery("INSERT INTO Users (Name) VALUES ('John Doe')");

        object name = _sqliteHelper.ExecuteScalar("SELECT Name FROM Users WHERE Id = 1");
        Assert.NotNull(name);
        Assert.Equal("John Doe", name);
    }

    [Fact]
    public void ExecuteQuery_ShouldReturnInsertedRows()
    {
        _sqliteHelper.ExecuteNonQuery("CREATE TABLE Users (Id INTEGER PRIMARY KEY, Name TEXT)");
        _sqliteHelper.ExecuteNonQuery("INSERT INTO Users (Name) VALUES ('Alice')");
        _sqliteHelper.ExecuteNonQuery("INSERT INTO Users (Name) VALUES ('Bob')");

        List<Dictionary<string, object>> users = _sqliteHelper.ExecuteQuery("SELECT * FROM Users");

        Assert.Equal(2, users.Count);
        Assert.Equal("Alice", users[0]["Name"]);
        Assert.Equal("Bob", users[1]["Name"]);
    }
}
