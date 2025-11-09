using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository implementation for User entity
/// </summary>
public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<User?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT user_id, email, password_hash, display_name, phone, is_admin, is_organizer,
                   created_utc, status, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_user
            WHERE user_id = @UserId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = @"
            SELECT user_id, email, password_hash, display_name, phone, is_admin, is_organizer,
                   created_utc, status, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_user
            WHERE email = @Email";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        const string sql = "SELECT COUNT(1) FROM fs_user WHERE email = @Email";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Email", email);

        var count = (int)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = @"
            SELECT user_id, email, password_hash, display_name, phone, is_admin, is_organizer,
                   created_utc, status, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_user
            WHERE status = 1
            ORDER BY display_name";

        var users = new List<User>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            users.Add(MapFromReader(reader));
        }

        return users;
    }

    public async Task<long> CreateAsync(User entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_user (email, password_hash, display_name, phone, is_admin, is_organizer,
                                 created_utc, status, data_inserimento, utente_inserimento)
            VALUES (@Email, @PasswordHash, @DisplayName, @Phone, @IsAdmin, @IsOrganizer,
                    @CreatedUtc, @Status, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Email", entity.Email);
        cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash);
        cmd.Parameters.AddWithValue("@DisplayName", entity.DisplayName);
        cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsAdmin", entity.IsAdmin);
        cmd.Parameters.AddWithValue("@IsOrganizer", entity.IsOrganizer);
        cmd.Parameters.AddWithValue("@CreatedUtc", entity.CreatedUtc);
        cmd.Parameters.AddWithValue("@Status", entity.Status);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        var id = (long)(decimal)await cmd.ExecuteScalarAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(User entity, string username)
    {
        const string sql = @"
            UPDATE fs_user
            SET display_name = @DisplayName,
                phone = @Phone,
                is_admin = @IsAdmin,
                is_organizer = @IsOrganizer,
                status = @Status,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE user_id = @UserId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@UserId", entity.UserId);
        cmd.Parameters.AddWithValue("@DisplayName", entity.DisplayName);
        cmd.Parameters.AddWithValue("@Phone", (object?)entity.Phone ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsAdmin", entity.IsAdmin);
        cmd.Parameters.AddWithValue("@IsOrganizer", entity.IsOrganizer);
        cmd.Parameters.AddWithValue("@Status", entity.Status);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // Soft delete - set status to 0
        const string sql = "UPDATE fs_user SET status = 0 WHERE user_id = @UserId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private User MapFromReader(SqlDataReader reader)
    {
        return new User
        {
            UserId = reader.GetInt64(reader.GetOrdinal("user_id")),
            Email = GetString(reader, "email"),
            PasswordHash = GetString(reader, "password_hash"),
            DisplayName = GetString(reader, "display_name"),
            Phone = GetValue<string>(reader, "phone"),
            IsAdmin = reader.GetBoolean(reader.GetOrdinal("is_admin")),
            IsOrganizer = reader.GetBoolean(reader.GetOrdinal("is_organizer")),
            CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),
            Status = reader.GetByte(reader.GetOrdinal("status")),
            DataInserimento = reader.GetDateTime(reader.GetOrdinal("data_inserimento")),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
