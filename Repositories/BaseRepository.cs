using System.Data;
using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Base repository implementation with common ADO.NET functionality
/// </summary>
public abstract class BaseRepository
{
    protected readonly string _connectionString;

    protected BaseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not configured");
    }

    /// <summary>
    /// Creates and opens a SQL connection
    /// </summary>
    protected async Task<SqlConnection> GetConnectionAsync()
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    /// <summary>
    /// Sets audit fields on entity for insert
    /// </summary>
    protected void SetAuditFieldsForInsert(BaseAuditEntity entity, string username)
    {
        entity.DataInserimento = DateTime.UtcNow;
        entity.UtenteInserimento = username;
        entity.DataModifica = null;
        entity.UtenteModifica = null;
    }

    /// <summary>
    /// Sets audit fields on entity for update
    /// </summary>
    protected void SetAuditFieldsForUpdate(BaseAuditEntity entity, string username)
    {
        entity.DataModifica = DateTime.UtcNow;
        entity.UtenteModifica = username;
    }

    /// <summary>
    /// Safely gets value from SqlDataReader, handling DBNull
    /// </summary>
    protected T? GetValue<T>(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? default : (T)reader.GetValue(ordinal);
    }

    /// <summary>
    /// Safely gets string value from SqlDataReader
    /// </summary>
    protected string GetString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
    }
}
