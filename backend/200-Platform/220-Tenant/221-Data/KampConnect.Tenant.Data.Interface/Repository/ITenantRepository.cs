namespace KampConnect.Tenant.Data.Interface.Repository;

using Tenant = KampConnect.Tenant.Data.Entity.Core.Tenant;

/// <summary>
/// Definieert de bewerkingen voor gegevenstoegang van <see cref="Tenant"/>-entiteiten.
/// </summary>
public interface ITenantRepository
{
    /// <summary>
    /// Haalt een tenant op aan de hand van zijn unieke id.
    /// </summary>
    /// <param name="tenantId">Het unieke id van de tenant.</param>
    /// <param name="ct">Een token om de bewerking te annuleren.</param>
    /// <returns>De gevonden <see cref="Tenant"/>, of <see langword="null"/> als er geen gevonden werd.</returns>
    Task<Tenant?> GetByIdAsync(int tenantId, CancellationToken ct = default);

    /// <summary>
    /// Haalt een tenant op aan de hand van zijn unieke slug.
    /// </summary>
    /// <param name="tenantSlug">De URL-vriendelijke slug van de tenant.</param>
    /// <param name="ct">Een token om de bewerking te annuleren.</param>
    /// <returns>De gevonden <see cref="Tenant"/>, of <see langword="null"/> als er geen gevonden werd.</returns>
    Task<Tenant?> GetBySlugAsync(string tenantSlug, CancellationToken ct = default);

    /// <summary>
    /// Bepaalt of er al een tenant met de opgegeven slug bestaat.
    /// </summary>
    /// <param name="tenantSlug">De slug die gecontroleerd moet worden.</param>
    /// <param name="ct">Een token om de bewerking te annuleren.</param>
    /// <returns><see langword="true"/> als de slug al in gebruik is; anders <see langword="false"/>.</returns>
    Task<bool> SlugExistsAsync(string tenantSlug, CancellationToken ct = default);

    /// <summary>
    /// Markeert een nieuwe tenant om toe te voegen. De tenant wordt opgeslagen wanneer <see cref="SaveChangesAsync"/> wordt aangeroepen.
    /// </summary>
    /// <param name="tenant">De tenant die toegevoegd moet worden.</param>
    void Add(Tenant tenant);

    /// <summary>
    /// Slaat alle openstaande wijzigingen op in de onderliggende gegevensopslag.
    /// </summary>
    /// <param name="ct">Een token om de bewerking te annuleren.</param>
    /// <returns>Een taak die de asynchrone opslagbewerking voorstelt.</returns>
    Task SaveChangesAsync(CancellationToken ct = default);
}
