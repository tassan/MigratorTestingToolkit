using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;

namespace MigratorTestingToolkit.Utils;

/// <summary>
/// Provides utility methods for interacting with an in-memory SQLite database for testing.
/// Implements the .NET Disposable Pattern to ensure proper resource cleanup.
/// </summary>
public class SqliteTestHelper : IDisposable
{
    private bool _disposed;
    private readonly SqliteConnection _connection;

    /// <summary>
    /// Initializes a new instance of <see cref="SqliteTestHelper"/> using an in-memory SQLite database.
    /// </summary>
    public SqliteTestHelper()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    /// <summary>
    /// Executes a SQL command against the database.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    public void ExecuteNonQuery(string query)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a SQL command with parameters against the database.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <param name="parameters">A dictionary containing parameter names and values.</param>
    public void ExecuteNonQuery(string query, Dictionary<string, object> parameters)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;
        foreach (KeyValuePair<string, object> param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }
        command.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a SQL query and returns a single value.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <returns>The first column of the first row in the result set, or null if no rows exist.</returns>
    public object ExecuteScalar(string query)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;
        return command.ExecuteScalar();
    }

    /// <summary>
    /// Executes a SQL query and returns the result as a list of dictionaries.
    /// </summary>
    /// <param name="query">The SQL query to execute.</param>
    /// <returns>A list of dictionaries, where each dictionary represents a row.</returns>
    public List<Dictionary<string, object>> ExecuteQuery(string query)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = query;

        using SqliteDataReader reader = command.ExecuteReader();
        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

        while (reader.Read())
        {
            Dictionary<string, object> row = new Dictionary<string, object>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.GetValue(i);
            }
            result.Add(row);
        }

        return result;
    }

    /// <summary>
    /// Checks if a table exists in the database.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <returns>True if the table exists, otherwise false.</returns>
    public bool TableExists(string tableName)
    {
        using SqliteCommand command = _connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@tableName;";
        command.Parameters.AddWithValue("@tableName", tableName);

        using SqliteDataReader reader = command.ExecuteReader();
        return reader.Read();
    }

    /// <summary>
    /// Closes and disposes the SQLite connection.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose"/>, false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_connection != null)
                {
                    _connection.Close();
                    _connection.Dispose();
                }
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Releases all resources used by this instance.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer to ensure unmanaged resources are released.
    /// </summary>
    ~SqliteTestHelper()
    {
        Dispose(disposing: false);
    }
}
