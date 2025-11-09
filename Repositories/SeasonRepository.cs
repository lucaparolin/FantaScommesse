using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public class SeasonRepository : BaseRepository, ISeasonRepository
{
    public SeasonRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Season?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT season_id, name, year_start, year_end, signup_deadline, base_fee_eur,
                   referral_discount, created_by, is_closed, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_season WHERE season_id = @SeasonId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<Season?> GetCurrentSeasonAsync()
    {
        const string sql = @"
            SELECT TOP 1 season_id, name, year_start, year_end, signup_deadline, base_fee_eur,
                   referral_discount, created_by, is_closed, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_season
            WHERE is_closed = 0 AND signup_deadline > SYSUTCDATETIME()
            ORDER BY year_start DESC";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<IEnumerable<Season>> GetActiveSeasons()
    {
        const string sql = @"
            SELECT season_id, name, year_start, year_end, signup_deadline, base_fee_eur,
                   referral_discount, created_by, is_closed, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_season WHERE is_closed = 0 ORDER BY year_start DESC";

        var seasons = new List<Season>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            seasons.Add(MapFromReader(reader));
        }
        return seasons;
    }

    public async Task<IEnumerable<Season>> GetAllAsync()
    {
        const string sql = @"
            SELECT season_id, name, year_start, year_end, signup_deadline, base_fee_eur,
                   referral_discount, created_by, is_closed, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_season ORDER BY year_start DESC";

        var seasons = new List<Season>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            seasons.Add(MapFromReader(reader));
        }
        return seasons;
    }

    public async Task<int> CreateAsync(Season entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_season (name, year_start, year_end, signup_deadline, base_fee_eur,
                                   referral_discount, created_by, is_closed, data_inserimento, utente_inserimento)
            VALUES (@Name, @YearStart, @YearEnd, @SignupDeadline, @BaseFeeEur,
                    @ReferralDiscount, @CreatedBy, @IsClosed, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@Name", entity.Name);
        cmd.Parameters.AddWithValue("@YearStart", entity.YearStart);
        cmd.Parameters.AddWithValue("@YearEnd", entity.YearEnd);
        cmd.Parameters.AddWithValue("@SignupDeadline", entity.SignupDeadline);
        cmd.Parameters.AddWithValue("@BaseFeeEur", entity.BaseFeeEur);
        cmd.Parameters.AddWithValue("@ReferralDiscount", entity.ReferralDiscount);
        cmd.Parameters.AddWithValue("@CreatedBy", entity.CreatedBy);
        cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(Season entity, string username)
    {
        const string sql = @"
            UPDATE fs_season
            SET name = @Name, signup_deadline = @SignupDeadline, base_fee_eur = @BaseFeeEur,
                referral_discount = @ReferralDiscount, is_closed = @IsClosed,
                data_modifica = @DataModifica, utente_modifica = @UtenteModifica
            WHERE season_id = @SeasonId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@SeasonId", entity.SeasonId);
        cmd.Parameters.AddWithValue("@Name", entity.Name);
        cmd.Parameters.AddWithValue("@SignupDeadline", entity.SignupDeadline);
        cmd.Parameters.AddWithValue("@BaseFeeEur", entity.BaseFeeEur);
        cmd.Parameters.AddWithValue("@ReferralDiscount", entity.ReferralDiscount);
        cmd.Parameters.AddWithValue("@IsClosed", entity.IsClosed);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM fs_season WHERE season_id = @SeasonId";
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@SeasonId", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private Season MapFromReader(SqlDataReader reader)
    {
        return new Season
        {
            SeasonId = reader.GetInt32(0),
            Name = reader.GetString(1),
            YearStart = reader.GetInt16(2),
            YearEnd = reader.GetInt16(3),
            SignupDeadline = reader.GetDateTime(4),
            BaseFeeEur = reader.GetDecimal(5),
            ReferralDiscount = reader.GetDecimal(6),
            CreatedBy = reader.GetInt64(7),
            IsClosed = reader.GetBoolean(8),
            DataInserimento = reader.GetDateTime(9),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
