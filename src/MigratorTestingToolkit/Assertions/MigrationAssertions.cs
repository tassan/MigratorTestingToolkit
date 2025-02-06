using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using MigratorTestingToolkit.Base;

namespace MigratorTestingToolkit.Assertions;

/// <summary>
/// Provides database schema assertions for migration testing.
/// </summary>
public class MigrationAssertions : MigrationTestBase
{
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationAssertions"/> with an in-memory SQLite database.
    /// </summary>
    public MigrationAssertions() : base(useInMemory: true) { }

    /// <summary>
    /// Checks if a specified table exists in the database.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>True if the table exists, otherwise false.</returns>
    public bool TableExists(string tableName)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
        command.Parameters.AddWithValue("@tableName", tableName);

        using SqliteDataReader reader = command.ExecuteReader();
        return reader.Read();
    }

    /// <summary>
    /// Checks if a specified table contains the expected columns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="expectedColumns">A dictionary mapping column names to their expected data types.</param>
    /// <returns>True if all expected columns exist with the correct data types, otherwise false.</returns>
    public bool TableHasColumns(string tableName, Dictionary<string, string> expectedColumns)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        using SqliteDataReader reader = command.ExecuteReader();
        Dictionary<string, string> actualColumns = new Dictionary<string, string>();

        while (reader.Read())
        {
            string columnName = reader.GetString(1);
            string columnType = reader.GetString(2);
            actualColumns[columnName] = columnType;
        }

        foreach (KeyValuePair<string, string> expected in expectedColumns)
        {
            if (!actualColumns.ContainsKey(expected.Key) || actualColumns[expected.Key] != expected.Value)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose"/>, false if called from finalizer.</param>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                base.Dispose(disposing);
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by this instance.
    /// </summary>
    public new void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~MigrationAssertions()
    {
        Dispose(disposing: false);
    }
}
