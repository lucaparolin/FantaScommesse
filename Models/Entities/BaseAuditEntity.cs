namespace FantaScommesse.Models.Entities;

/// <summary>
/// Base class for all entities requiring audit trail
/// Implements standard audit fields as per specifications
/// </summary>
public abstract class BaseAuditEntity
{
    /// <summary>
    /// Data e ora di inserimento del record (UTC)
    /// </summary>
    public DateTime DataInserimento { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data e ora dell'ultima modifica (UTC)
    /// </summary>
    public DateTime? DataModifica { get; set; }

    /// <summary>
    /// Utente che ha inserito il record
    /// </summary>
    public string UtenteInserimento { get; set; } = "SYSTEM";

    /// <summary>
    /// Utente che ha effettuato l'ultima modifica
    /// </summary>
    public string? UtenteModifica { get; set; }
}
