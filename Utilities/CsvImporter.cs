using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace FantaScommesse.Utilities;

public class CsvImporter
{
    public async Task<List<T>> ImportCsvAsync<T>(Stream fileStream)
    {
        using var reader = new StreamReader(fileStream);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null
        });

        var records = new List<T>();
        await foreach (var record in csv.GetRecordsAsync<T>())
        {
            records.Add(record);
        }

        return records;
    }

    public async Task ExportCsvAsync<T>(IEnumerable<T> data, Stream outputStream)
    {
        await using var writer = new StreamWriter(outputStream, leaveOpen: true);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(data);
    }
}

public class MatchCsvRow
{
    public int RoundNo { get; set; }
    public int OrderNo { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime KickoffUtc { get; set; }
}
