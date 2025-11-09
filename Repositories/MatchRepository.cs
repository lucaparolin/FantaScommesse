using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository implementation for Match entity
/// </summary>
public class MatchRepository : BaseRepository, IMatchRepository
{
    public MatchRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Match?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT match_id, round_id, order_no, home_team_id, away_team_id, kickoff_utc,
                   result_code, goals_home, goals_away, ggng, ou, api_match_id,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_match
            WHERE match_id = @MatchId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MatchId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Match>> GetByRoundIdAsync(int roundId)
    {
        const string sql = @"
            SELECT match_id, round_id, order_no, home_team_id, away_team_id, kickoff_utc,
                   result_code, goals_home, goals_away, ggng, ou, api_match_id,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_match
            WHERE round_id = @RoundId
            ORDER BY order_no";

        var matches = new List<Match>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            matches.Add(MapFromReader(reader));
        }

        return matches;
    }

    public async Task<IEnumerable<Match>> GetAllAsync()
    {
        const string sql = @"
            SELECT match_id, round_id, order_no, home_team_id, away_team_id, kickoff_utc,
                   result_code, goals_home, goals_away, ggng, ou, api_match_id,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_match
            ORDER BY kickoff_utc DESC";

        var matches = new List<Match>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            matches.Add(MapFromReader(reader));
        }

        return matches;
    }

    public async Task<long> CreateAsync(Match entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_match (round_id, order_no, home_team_id, away_team_id, kickoff_utc,
                                  result_code, goals_home, goals_away, ggng, ou, api_match_id,
                                  data_inserimento, utente_inserimento)
            VALUES (@RoundId, @OrderNo, @HomeTeamId, @AwayTeamId, @KickoffUtc,
                    @ResultCode, @GoalsHome, @GoalsAway, @GgNg, @Ou, @ApiMatchId,
                    @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@RoundId", entity.RoundId);
        cmd.Parameters.AddWithValue("@OrderNo", entity.OrderNo);
        cmd.Parameters.AddWithValue("@HomeTeamId", entity.HomeTeamId);
        cmd.Parameters.AddWithValue("@AwayTeamId", entity.AwayTeamId);
        cmd.Parameters.AddWithValue("@KickoffUtc", entity.KickoffUtc);
        cmd.Parameters.AddWithValue("@ResultCode", (object?)entity.ResultCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoalsHome", (object?)entity.GoalsHome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoalsAway", (object?)entity.GoalsAway ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GgNg", (object?)entity.GgNg ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Ou", (object?)entity.Ou ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ApiMatchId", (object?)entity.ApiMatchId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        var id = (long)(decimal)await cmd.ExecuteScalarAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(Match entity, string username)
    {
        const string sql = @"
            UPDATE fs_match
            SET kickoff_utc = @KickoffUtc,
                result_code = @ResultCode,
                goals_home = @GoalsHome,
                goals_away = @GoalsAway,
                ggng = @GgNg,
                ou = @Ou,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE match_id = @MatchId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@MatchId", entity.MatchId);
        cmd.Parameters.AddWithValue("@KickoffUtc", entity.KickoffUtc);
        cmd.Parameters.AddWithValue("@ResultCode", (object?)entity.ResultCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoalsHome", (object?)entity.GoalsHome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoalsAway", (object?)entity.GoalsAway ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GgNg", (object?)entity.GgNg ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Ou", (object?)entity.Ou ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> UpdateResultsAsync(long matchId, string resultCode, byte? goalsHome, byte? goalsAway, string? ggng, string? ou, string username)
    {
        const string sql = @"
            UPDATE fs_match
            SET result_code = @ResultCode,
                goals_home = @GoalsHome,
                goals_away = @GoalsAway,
                ggng = @GgNg,
                ou = @Ou,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE match_id = @MatchId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@MatchId", matchId);
        cmd.Parameters.AddWithValue("@ResultCode", resultCode);
        cmd.Parameters.AddWithValue("@GoalsHome", (object?)goalsHome ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GoalsAway", (object?)goalsAway ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GgNg", (object?)ggng ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Ou", (object?)ou ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataModifica", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("@UtenteModifica", username);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM fs_match WHERE match_id = @MatchId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@MatchId", id);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Match MapFromReader(SqlDataReader reader)
    {
        return new Match
        {
            MatchId = reader.GetInt64(reader.GetOrdinal("match_id")),
            RoundId = reader.GetInt32(reader.GetOrdinal("round_id")),
            OrderNo = reader.GetByte(reader.GetOrdinal("order_no")),
            HomeTeamId = reader.GetInt32(reader.GetOrdinal("home_team_id")),
            AwayTeamId = reader.GetInt32(reader.GetOrdinal("away_team_id")),
            KickoffUtc = reader.GetDateTime(reader.GetOrdinal("kickoff_utc")),
            ResultCode = GetValue<string>(reader, "result_code"),
            GoalsHome = GetValue<byte?>(reader, "goals_home"),
            GoalsAway = GetValue<byte?>(reader, "goals_away"),
            GgNg = GetValue<string>(reader, "ggng"),
            Ou = GetValue<string>(reader, "ou"),
            ApiMatchId = GetValue<string>(reader, "api_match_id"),
            DataInserimento = reader.GetDateTime(reader.GetOrdinal("data_inserimento")),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
