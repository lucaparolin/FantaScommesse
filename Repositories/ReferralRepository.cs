using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public class ReferralRepository : BaseRepository, IReferralRepository
{
    public ReferralRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Referral?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT referral_id, referrer_participation_id, referred_participation_id, discount_eur,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_referral WHERE referral_id = @ReferralId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ReferralId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<IEnumerable<Referral>> GetByReferrerIdAsync(long referrerParticipationId)
    {
        const string sql = @"
            SELECT referral_id, referrer_participation_id, referred_participation_id, discount_eur,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_referral WHERE referrer_participation_id = @ReferrerId";

        var referrals = new List<Referral>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ReferrerId", referrerParticipationId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            referrals.Add(MapFromReader(reader));
        }
        return referrals;
    }

    public async Task<int> GetReferralCountAsync(long referrerParticipationId)
    {
        const string sql = "SELECT COUNT(*) FROM fs_referral WHERE referrer_participation_id = @ReferrerId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ReferrerId", referrerParticipationId);

        return (int)await cmd.ExecuteScalarAsync();
    }

    public async Task<decimal> GetTotalDiscountsEarnedAsync(long referrerParticipationId)
    {
        const string sql = "SELECT ISNULL(SUM(discount_eur), 0) FROM fs_referral WHERE referrer_participation_id = @ReferrerId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ReferrerId", referrerParticipationId);

        return (decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<IEnumerable<Referral>> GetAllAsync()
    {
        const string sql = @"
            SELECT referral_id, referrer_participation_id, referred_participation_id, discount_eur,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_referral ORDER BY data_inserimento DESC";

        var referrals = new List<Referral>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            referrals.Add(MapFromReader(reader));
        }
        return referrals;
    }

    public async Task<long> CreateAsync(Referral entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_referral (referrer_participation_id, referred_participation_id, discount_eur,
                                     data_inserimento, utente_inserimento)
            VALUES (@ReferrerId, @ReferredId, @DiscountEur, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@ReferrerId", entity.ReferrerParticipationId);
        cmd.Parameters.AddWithValue("@ReferredId", entity.ReferredParticipationId);
        cmd.Parameters.AddWithValue("@DiscountEur", entity.DiscountEur);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        return (long)(decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(Referral entity, string username)
    {
        const string sql = @"
            UPDATE fs_referral
            SET discount_eur = @DiscountEur, data_modifica = @DataModifica, utente_modifica = @UtenteModifica
            WHERE referral_id = @ReferralId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@ReferralId", entity.ReferralId);
        cmd.Parameters.AddWithValue("@DiscountEur", entity.DiscountEur);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM fs_referral WHERE referral_id = @ReferralId";
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ReferralId", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private Referral MapFromReader(SqlDataReader reader)
    {
        return new Referral
        {
            ReferralId = reader.GetInt64(0),
            ReferrerParticipationId = reader.GetInt64(1),
            ReferredParticipationId = reader.GetInt64(2),
            DiscountEur = reader.GetDecimal(3),
            DataInserimento = reader.GetDateTime(4),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
