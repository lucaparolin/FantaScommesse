using System.Data;
using System.Data.SqlClient;
using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository implementation for Prediction entity
/// </summary>
public class PredictionRepository : BaseRepository, IPredictionRepository
{
    public PredictionRepository(IConfiguration configuration) : base(configuration) { }

    public async Task<Prediction?> GetByIdAsync(long id)
    {
        const string sql = @"
            SELECT prediction_id, participation_id, round_id, created_utc, submitted_utc,
                   is_valid, is_late, errors_cnt, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_prediction
            WHERE prediction_id = @PredictionId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PredictionId", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<Prediction?> GetByParticipationAndRoundAsync(long participationId, int roundId)
    {
        const string sql = @"
            SELECT prediction_id, participation_id, round_id, created_utc, submitted_utc,
                   is_valid, is_late, errors_cnt, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_prediction
            WHERE participation_id = @ParticipationId AND round_id = @RoundId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ParticipationId", participationId);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapFromReader(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Prediction>> GetByRoundIdAsync(int roundId)
    {
        const string sql = @"
            SELECT prediction_id, participation_id, round_id, created_utc, submitted_utc,
                   is_valid, is_late, errors_cnt, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_prediction
            WHERE round_id = @RoundId AND submitted_utc IS NOT NULL";

        var predictions = new List<Prediction>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@RoundId", roundId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            predictions.Add(MapFromReader(reader));
        }

        return predictions;
    }

    public async Task<IEnumerable<Prediction>> GetAllAsync()
    {
        const string sql = @"
            SELECT prediction_id, participation_id, round_id, created_utc, submitted_utc,
                   is_valid, is_late, errors_cnt, data_inserimento, data_modifica,
                   utente_inserimento, utente_modifica
            FROM fs_prediction
            ORDER BY created_utc DESC";

        var predictions = new List<Prediction>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            predictions.Add(MapFromReader(reader));
        }

        return predictions;
    }

    public async Task<long> CreateAsync(Prediction entity, string username)
    {
        const string sql = @"
            INSERT INTO fs_prediction (participation_id, round_id, created_utc, submitted_utc,
                                       is_valid, is_late, errors_cnt, data_inserimento, utente_inserimento)
            VALUES (@ParticipationId, @RoundId, @CreatedUtc, @SubmittedUtc,
                    @IsValid, @IsLate, @ErrorsCnt, @DataInserimento, @UtenteInserimento);
            SELECT CAST(SCOPE_IDENTITY() AS BIGINT);";

        SetAuditFieldsForInsert(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@ParticipationId", entity.ParticipationId);
        cmd.Parameters.AddWithValue("@RoundId", entity.RoundId);
        cmd.Parameters.AddWithValue("@CreatedUtc", entity.CreatedUtc);
        cmd.Parameters.AddWithValue("@SubmittedUtc", (object?)entity.SubmittedUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsValid", entity.IsValid);
        cmd.Parameters.AddWithValue("@IsLate", entity.IsLate);
        cmd.Parameters.AddWithValue("@ErrorsCnt", entity.ErrorsCnt);
        cmd.Parameters.AddWithValue("@DataInserimento", entity.DataInserimento);
        cmd.Parameters.AddWithValue("@UtenteInserimento", entity.UtenteInserimento);

        var id = (long)(decimal)await cmd.ExecuteScalarAsync();
        return id;
    }

    public async Task<bool> UpdateAsync(Prediction entity, string username)
    {
        const string sql = @"
            UPDATE fs_prediction
            SET submitted_utc = @SubmittedUtc,
                is_valid = @IsValid,
                is_late = @IsLate,
                errors_cnt = @ErrorsCnt,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE prediction_id = @PredictionId";

        SetAuditFieldsForUpdate(entity, username);

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@PredictionId", entity.PredictionId);
        cmd.Parameters.AddWithValue("@SubmittedUtc", (object?)entity.SubmittedUtc ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsValid", entity.IsValid);
        cmd.Parameters.AddWithValue("@IsLate", entity.IsLate);
        cmd.Parameters.AddWithValue("@ErrorsCnt", entity.ErrorsCnt);
        cmd.Parameters.AddWithValue("@DataModifica", entity.DataModifica!);
        cmd.Parameters.AddWithValue("@UtenteModifica", entity.UtenteModifica!);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        // Hard delete for predictions
        const string sqlItems = "DELETE FROM fs_prediction_item WHERE prediction_id = @PredictionId";
        const string sqlPrediction = "DELETE FROM fs_prediction WHERE prediction_id = @PredictionId";

        using var conn = await GetConnectionAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            using (var cmd = new SqlCommand(sqlItems, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PredictionId", id);
                await cmd.ExecuteNonQueryAsync();
            }

            using (var cmd = new SqlCommand(sqlPrediction, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PredictionId", id);
                await cmd.ExecuteNonQueryAsync();
            }

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            return false;
        }
    }

    public async Task<IEnumerable<PredictionItem>> GetPredictionItemsAsync(long predictionId)
    {
        const string sql = @"
            SELECT item_id, prediction_id, match_id, selection,
                   data_inserimento, data_modifica, utente_inserimento, utente_modifica
            FROM fs_prediction_item
            WHERE prediction_id = @PredictionId
            ORDER BY item_id";

        var items = new List<PredictionItem>();
        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@PredictionId", predictionId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(MapItemFromReader(reader));
        }

        return items;
    }

    public async Task SavePredictionItemsAsync(long predictionId, IEnumerable<PredictionItem> items, string username)
    {
        const string sqlDelete = "DELETE FROM fs_prediction_item WHERE prediction_id = @PredictionId";
        const string sqlInsert = @"
            INSERT INTO fs_prediction_item (prediction_id, match_id, selection, data_inserimento, utente_inserimento)
            VALUES (@PredictionId, @MatchId, @Selection, @DataInserimento, @UtenteInserimento)";

        using var conn = await GetConnectionAsync();
        using var transaction = conn.BeginTransaction();

        try
        {
            // Delete existing items
            using (var cmd = new SqlCommand(sqlDelete, conn, transaction))
            {
                cmd.Parameters.AddWithValue("@PredictionId", predictionId);
                await cmd.ExecuteNonQueryAsync();
            }

            // Insert new items
            foreach (var item in items)
            {
                SetAuditFieldsForInsert(item, username);

                using var cmd = new SqlCommand(sqlInsert, conn, transaction);
                cmd.Parameters.AddWithValue("@PredictionId", predictionId);
                cmd.Parameters.AddWithValue("@MatchId", item.MatchId);
                cmd.Parameters.AddWithValue("@Selection", item.Selection);
                cmd.Parameters.AddWithValue("@DataInserimento", item.DataInserimento);
                cmd.Parameters.AddWithValue("@UtenteInserimento", item.UtenteInserimento);

                await cmd.ExecuteNonQueryAsync();
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public async Task<bool> SubmitPredictionAsync(long predictionId, DateTime deadline, string username)
    {
        var now = DateTime.UtcNow;
        var isLate = now > deadline;

        const string sql = @"
            UPDATE fs_prediction
            SET submitted_utc = @SubmittedUtc,
                is_late = @IsLate,
                data_modifica = @DataModifica,
                utente_modifica = @UtenteModifica
            WHERE prediction_id = @PredictionId";

        using var conn = await GetConnectionAsync();
        using var cmd = new SqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@PredictionId", predictionId);
        cmd.Parameters.AddWithValue("@SubmittedUtc", now);
        cmd.Parameters.AddWithValue("@IsLate", isLate);
        cmd.Parameters.AddWithValue("@DataModifica", now);
        cmd.Parameters.AddWithValue("@UtenteModifica", username);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    private Prediction MapFromReader(SqlDataReader reader)
    {
        return new Prediction
        {
            PredictionId = reader.GetInt64(reader.GetOrdinal("prediction_id")),
            ParticipationId = reader.GetInt64(reader.GetOrdinal("participation_id")),
            RoundId = reader.GetInt32(reader.GetOrdinal("round_id")),
            CreatedUtc = reader.GetDateTime(reader.GetOrdinal("created_utc")),
            SubmittedUtc = GetValue<DateTime?>(reader, "submitted_utc"),
            IsValid = reader.GetBoolean(reader.GetOrdinal("is_valid")),
            IsLate = reader.GetBoolean(reader.GetOrdinal("is_late")),
            ErrorsCnt = reader.GetByte(reader.GetOrdinal("errors_cnt")),
            DataInserimento = reader.GetDateTime(reader.GetOrdinal("data_inserimento")),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }

    private PredictionItem MapItemFromReader(SqlDataReader reader)
    {
        return new PredictionItem
        {
            ItemId = reader.GetInt64(reader.GetOrdinal("item_id")),
            PredictionId = reader.GetInt64(reader.GetOrdinal("prediction_id")),
            MatchId = reader.GetInt64(reader.GetOrdinal("match_id")),
            Selection = GetString(reader, "selection"),
            DataInserimento = reader.GetDateTime(reader.GetOrdinal("data_inserimento")),
            DataModifica = GetValue<DateTime?>(reader, "data_modifica"),
            UtenteInserimento = GetString(reader, "utente_inserimento"),
            UtenteModifica = GetValue<string>(reader, "utente_modifica")
        };
    }
}
