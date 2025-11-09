using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository implementation for Round entity
/// </summary>
public class RoundRepository : BaseRepository, IRoundRepository
{
    public RoundRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Round?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT round_id, season_id, round_no, deadline_utc, is_published, is_closed,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_round
            WHERE round_id = @RoundId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<Round?> GetBySeasonAndRoundNoAsync(int seasonId, int roundNo)
    {
        const string sql = @"
            SELECT round_id, season_id, round_no, deadline_utc, is_published, is_closed,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_round
            WHERE season_id = @SeasonId AND round_no = @RoundNo";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);
        cmd.Parameters.AddWithValue("@RoundNo", roundNo);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Round>> GetBySeasonIdAsync(int seasonId)
    {
        const string sql = @"
            SELECT round_id, season_id, round_no, deadline_utc, is_published, is_closed,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_round
            WHERE season_id = @SeasonId
            ORDER BY round_no";

        var rounds = new List<Round>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rounds.Add(MapFromReader(reader));
        }

        return rounds;
    }

    public async Task<Round?> GetCurrentRoundAsync()
    {
        const string sql = @"
            SELECT TOP 1 round_id, season_id, round_no, deadline_utc, is_published, is_closed,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_round
            WHERE is_published = 1 AND is_closed = 0 AND deadline_utc > SYSUTCDATETIME()
            ORDER BY deadline_utc ASC";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Round>> GetAllAsync()
    {
        const string sql = @"
            SELECT round_id, season_id, round_no, deadline_utc, is_published, is_closed,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_round
            ORDER BY season_id DESC, round_no";

        var rounds = new List<Round>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rounds.Add(MapFromReader(reader));
        }

        return rounds;
    }

    public async Task<int> CreateAsync(Round entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_round (season_id, round_no, deadline_utc, is_published, is_closed,
                                  data_inserimento, utente_inserimento)
            VALUES (@SeasonId, @RoundNo, @DeadlineUtc, @IsPublished, @IsClosed,
                    @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@SeasonId", entity.SeasonId);
        cmd.Parameters.AddWithValue("@RoundNo", entity.RoundNo);
        cmd.Parameters.AddWithValue("@DeadlineUtc", entity.DeadlineUtc);
        cmd.Parameters.AddWithValue("@IsPublished", entity.IsPublished);
        cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        var id = (int)await cmd.ExecuteScalarAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(Round entity, string username)
    {
        const string sql = @"
            UPDATE fs_round
            SET deadline_utc = @DeadlineUtc,
                is_published = @IsPublished,
                is_closed = @IsClosed,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE round_id = @RoundId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@RoundId", entity.RoundId);
        cmd.Parameters.AddWithValue("@DeadlineUtc", entity.DeadlineUtc);
        cmd.Parameters.AddWithValue("@IsPublished", entity.IsPublished);
        cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM fs_round WHERE round_id = @RoundId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Round MapFromReader(SqlDataReader reader)
    {
        return new Round
        {
            RoundId = reader.GetInt32(reader.GetOrdinal("round_id")),
            SeasonId = reader.GetInt32(reader.GetOrdinal("season_id")),
            RoundNo = reader.GetInt32(reader.GetOrdinal("round_no")),
            DeadlineUtc = reader.GetDateTime(reader.GetOrdinal("deadline_utc")),
            IsPublished = reader.GetBoolean(reader.GetOrdinal("is_published")),
            IsClosed = reader.GetBoolean(reader.GetOrdinal("is_closed")),
            DataInserimento = reader.GetDateTime(reader.GetOrdinal("data_inserimento")),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
