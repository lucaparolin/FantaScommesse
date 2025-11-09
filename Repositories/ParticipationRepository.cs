using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public class ParticipationRepository : BaseRepository, IParticipationRepository
{
    public ParticipationRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Participation?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT participation_id, user_id, season_id, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_participation WHERE participation_id = @ParticipationId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<Participation?> GetByUserAndSeasonAsync(long userId, int seasonId)
    {
        const string sql = @"
            SELECT participation_id, user_id, season_id, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_participation WHERE user_id = @UserId AND season_id = @SeasonId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<IEnumerable<Participation>> GetBySeasonIdAsync(int seasonId)
    {
        const string sql = @"
            SELECT participation_id, user_id, season_id, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_participation WHERE season_id = @SeasonId";

        var participations = new List<Participation>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            participations.Add(MapFromReader(reader));
        }
        return participations;
    }

    public async Task<IEnumerable<Participation>> GetByUserIdAsync(long userId)
    {
        const string sql = @"
            SELECT participation_id, user_id, season_id, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_participation WHERE user_id = @UserId";

        var participations = new List<Participation>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            participations.Add(MapFromReader(reader));
        }
        return participations;
    }

    public async Task<int> GetParticipantCountAsync(int seasonId)
    {
        const string sql = "SELECT COUNT(*) FROM fs_participation WHERE season_id = @SeasonId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", seasonId);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<IEnumerable<Participation>> GetAllAsync()
    {
        const string sql = @"
            SELECT participation_id, user_id, season_id, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_participation ORDER BY data_inserimento DESC";

        var participations = new List<Participation>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            participations.Add(MapFromReader(reader));
        }
        return participations;
    }

    public async Task<long> CreateAsync(Participation entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_participation (user_id, season_id, data_inserimento, utente_inserimento)
            VALUES (@UserId, @SeasonId, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@UserId", entity.UserId);
        cmd.Parameters.AddWithValue("@SeasonId", entity.SeasonId);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        return (long)(decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(Participation entity, string username)
    {
        const string sql = @"
            UPDATE fs_participation
            SET data_modifica = @DataModifica, utente_modifica = @UtenteModifica
            WHERE participation_id = @ParticipationId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@ParticipationId", entity.ParticipationId);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM fs_participation WHERE participation_id = @ParticipationId";
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private Participation MapFromReader(SqlDataReader reader)
    {
        return new Participation
        {
            ParticipationId = reader.GetInt64(0),
            UserId = reader.GetInt64(1),
            SeasonId = reader.GetInt32(2),
            DataInserimento = reader.GetDateTime(3),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
