using System;
using Xunit;
using MigratorTestingToolkit.Base;
using Microsoft.Data.Sqlite;
using FluentMigrator;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace MigratorTestingToolkit.Tests;

/// <summary>
/// Unit tests for <see cref="MigrationTestBase"/>.
/// Ensures migrations setup and cleanup correctly.
/// </summary>
public class MigrationTestBaseTests : IDisposable
{
    private readonly MigrationTestBase _migrationBase;
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new in-memory SQLite database for testing.
    /// </summary>
    public MigrationTestBaseTests()
    {
        _migrationBase = new TestMigrationBase();
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Ensures the database is cleaned up after tests.
    /// </summary>
    public void Dispose()
    {
        _migrationBase.Dispose();
        _connection.Close();
    }

    [Fact]
    public void Setup_ShouldRunMigrations()
    {
        _migrationBase.Setup();

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TestTable'";
        using SqliteDataReader reader = command.ExecuteReader();
        Assert.True(reader.Read(), "Migration should have created the TestTable.");
    }

    [Fact]
    public void Cleanup_ShouldRollbackMigrations()
    {
        _migrationBase.Setup();
        _migrationBase.Cleanup();

        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TestTable'";
        using SqliteDataReader reader = command.ExecuteReader();
        Assert.False(reader.Read(), "Migration should have rolled back TestTable.");
    }
}

/// <summary>
/// Test implementation of <see cref="MigrationTestBase"/> with a sample migration.
/// </summary>
public class TestMigrationBase : MigrationTestBase
{
    public TestMigrationBase() : base(useInMemory: true) { }

    [Migration(202502060001)]
    public class TestMigration : Migration
    {
        public override void Up()
        {
            Create.Table("TestTable")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString().NotNullable();
        }

        public override void Down()
        {
            Delete.Table("TestTable");
        }
    }
}
