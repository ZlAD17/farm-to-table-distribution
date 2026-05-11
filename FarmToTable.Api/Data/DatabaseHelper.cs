using Microsoft.Data.SqlClient;

namespace FarmToTable.Api.Data;

/// <summary>
/// Lightweight database helper. Provides a factory method for opening
/// SqlConnections so that repositories do not need to reference
/// IConfiguration directly.
/// </summary>
public sealed class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    /// <summary>
    /// Creates and opens a new SqlConnection.
    /// Callers are responsible for disposing it (using or await using).
    /// </summary>
    public async Task<SqlConnection> OpenConnectionAsync()
    {
        var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();
        return conn;
    }
}
