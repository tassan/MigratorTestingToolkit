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
    /// Checks if a specified table contains the expected columns with correct types.
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
    /// Checks if a specified table contains a primary key on a given column.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The primary key column name.</param>
    /// <returns>True if the column is a primary key, otherwise false.</returns>
    public bool TableHasPrimaryKey(string tableName, string columnName)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName});";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string currentColumn = reader.GetString(1);
            int isPrimaryKey = reader.GetInt32(5); // PRAGMA table_info: Column 5 is PK flag

            if (currentColumn == columnName && isPrimaryKey == 1)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a specified column has a unique constraint.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The column name.</param>
    /// <returns>True if the column has a unique constraint, otherwise false.</returns>
    public bool TableHasUniqueConstraint(string tableName, string columnName)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"PRAGMA index_list({tableName});";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string indexName = reader.GetString(1);
            bool isUnique = reader.GetBoolean(2);

            if (indexName.Contains(columnName, StringComparison.OrdinalIgnoreCase) && isUnique)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a specified index exists in a table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="indexName">The index name.</param>
    /// <returns>True if the index exists, otherwise false.</returns>
    public bool IndexExists(string tableName, string indexName)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"PRAGMA index_list({tableName});";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            if (reader.GetString(1) == indexName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a specified foreign key exists in a table.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="columnName">The foreign key column name.</param>
    /// <param name="referencedTable">The referenced table.</param>
    /// <returns>True if the foreign key exists, otherwise false.</returns>
    public bool TableHasForeignKey(string tableName, string columnName, string referencedTable)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"PRAGMA foreign_key_list({tableName});";

        using SqliteDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            string fkColumn = reader.GetString(3);
            string referencedTableName = reader.GetString(2);

            if (fkColumn == columnName && referencedTableName == referencedTable)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if a table has at least the specified number of rows.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="minimumRowCount">The minimum expected row count.</param>
    /// <returns>True if the table has at least the specified rows, otherwise false.</returns>
    public bool TableHasMinimumRows(string tableName, int minimumRowCount)
    {
        using SqliteCommand command = Connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM {tableName};";

        long rowCount = (long)command.ExecuteScalar();
        return rowCount >= minimumRowCount;
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
