using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

public class PaymentRepository : BaseRepository, IPaymentRepository
{
    public PaymentRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Payment?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT payment_id, participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                   method, note, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_payment WHERE payment_id = @PaymentId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaymentId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapFromReader(reader) : null;
    }

    public async Task<IEnumerable<Payment>> GetByParticipationIdAsync(long participationId)
    {
        const string sql = @"
            SELECT payment_id, participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                   method, note, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_payment WHERE participation_id = @ParticipationId ORDER BY due_on_utc";

        var payments = new List<Payment>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", participationId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapFromReader(reader));
        }
        return payments;
    }

    public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
    {
        const string sql = @"
            SELECT payment_id, participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                   method, note, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_payment
            WHERE paid_eur < expected_eur
            ORDER BY due_on_utc";

        var payments = new List<Payment>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapFromReader(reader));
        }
        return payments;
    }

    public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync()
    {
        const string sql = @"
            SELECT payment_id, participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                   method, note, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_payment
            WHERE paid_eur < expected_eur AND due_on_utc < SYSUTCDATETIME()
            ORDER BY due_on_utc";

        var payments = new List<Payment>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapFromReader(reader));
        }
        return payments;
    }

    public async Task<decimal> GetTotalPaidByParticipationAsync(long participationId)
    {
        const string sql = "SELECT ISNULL(SUM(paid_eur), 0) FROM fs_payment WHERE participation_id = @ParticipationId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", participationId);

        return (decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<decimal> GetTotalDueByParticipationAsync(long participationId)
    {
        const string sql = "SELECT ISNULL(SUM(expected_eur - paid_eur), 0) FROM fs_payment WHERE participation_id = @ParticipationId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", participationId);

        return (decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        const string sql = @"
            SELECT payment_id, participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                   method, note, data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_payment ORDER BY due_on_utc DESC";

        var payments = new List<Payment>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            payments.Add(MapFromReader(reader));
        }
        return payments;
    }

    public async Task<long> CreateAsync(Payment entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_payment (participation_id, expected_eur, paid_eur, due_on_utc, paid_on_utc,
                                    method, note, data_inserimento, utente_inserimento)
            VALUES (@ParticipationId, @ExpectedEur, @PaidEur, @DueOnUtc, @PaidOnUtc,
                    @Method, @Note, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@ParticipationId", entity.ParticipationId);
        cmd.Parameters.AddWithValue("@ExpectedEur", entity.ExpectedEur);
        cmd.Parameters.AddWithValue("@PaidEur", entity.PaidEur);
        cmd.Parameters.AddWithValue("@DueOnUtc", entity.DueOnUtc);
        cmd.Parameters.AddWithValue("@PaidOnUtc", (object?)entity.PaidOnUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Method", (object?)entity.Method ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Note", (object?)entity.Note ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        return (long)(decimal)await cmd.ExecuteScalarAsync();
    }

    public async Task<bool> UpdateAsync(Payment entity, string username)
    {
        const string sql = @"
            UPDATE fs_payment
            SET paid_eur = @PaidEur, paid_on_utc = @PaidOnUtc, method = @Method, note = @Note,
                data_modifica = @DataModifica, utente_modifica = @UtenteModifica
            WHERE payment_id = @PaymentId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@PaymentId", entity.PaymentId);
        cmd.Parameters.AddWithValue("@PaidEur", entity.PaidEur);
        cmd.Parameters.AddWithValue("@PaidOnUtc", (object?)entity.PaidOnUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Method", (object?)entity.Method ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Note", (object?)entity.Note ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string sql = "DELETE FROM fs_payment WHERE payment_id = @PaymentId";
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PaymentId", id);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    private Payment MapFromReader(SqlDataReader reader)
    {
        return new Payment
        {
            PaymentId = reader.GetInt64(0),
            ParticipationId = reader.GetInt64(1),
            ExpectedEur = reader.GetDecimal(2),
            PaidEur = reader.GetDecimal(3),
            DueOnUtc = reader.GetDateTime(4),
            PaidOnUtc = GetValue<DateTime?>(reader, "paid_on_utc"),
            Method = GetValue<string>(reader, "method"),
            Note = GetValue<string>(reader, "note"),
            DataInserimento = reader.GetDateTime(8),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
