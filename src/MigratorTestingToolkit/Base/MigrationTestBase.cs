using System;
using Microsoft.Data.Sqlite;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;

namespace MigratorTestingToolkit.Base;

/// <summary>
/// Base class for migration testing, providing setup, teardown, and FluentMigrator integration.
/// Implements IDisposable to ensure proper resource cleanup.
/// </summary>
public abstract class MigrationTestBase : IDisposable
{
    protected IMigrationRunner MigrationRunner { get; }
    protected readonly SqliteConnection Connection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationTestBase"/> with a specified database connection.
    /// </summary>
    /// <param name="connection">Database connection string.</param>
    /// <param name="useInMemory">Specifies whether to use an in-memory SQLite database.</param>
    protected MigrationTestBase(string connection = "Data Source=migrations.db", bool useInMemory = false)
    {
        string connectionString = useInMemory ? "Data Source=:memory:" : connection;
        Connection = new SqliteConnection(connectionString);
        Connection.Open();

        ServiceProvider serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(rb => rb
                .AddSQLite()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(MigrationTestBase).Assembly).For.Migrations())
            .BuildServiceProvider(false);

        MigrationRunner = serviceProvider.GetRequiredService<IMigrationRunner>();
    }

    /// <summary>
    /// Runs all database migrations up to the latest version.
    /// </summary>
    public void Setup()
    {
        MigrationRunner.MigrateUp();
    }

    /// <summary>
    /// Rolls back all applied database migrations.
    /// </summary>
    public void Cleanup()
    {
        MigrationRunner.MigrateDown(0);
    }

    /// <summary>
    /// Releases resources used by this instance.
    /// </summary>
    /// <param name="disposing">True if called from <see cref="Dispose"/>, false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
                if (MigrationRunner?.Processor != null)
                {
                    MigrationRunner.Processor.Dispose();
                }
                if (Connection != null)
                {
                    Connection.Close();
                    Connection.Dispose();
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
    ~MigrationTestBase()
    {
        Dispose(disposing: false);
    }
}
