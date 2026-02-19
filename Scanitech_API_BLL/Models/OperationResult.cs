namespace Scanitech_API_BLL.Models;

/// <summary>
/// Repræsenterer resultatet af en forretningsoperation (v5.0 standard).
/// </summary>
/// <param name="SuccessCount">Antal succesfuldt behandlede elementer.</param>
/// <param name="Errors">Liste over fejlbeskeder.</param>
/// <param name="Warnings">Liste over advarsler.</param>
public sealed record OperationResult(
    int SuccessCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    /// <summary>Angiver om operationen indeholder fejl.</summary>
    public bool HasErrors => Errors.Count > 0;

    /// <summary>Angiver om operationen var fuldstændig succesfuld uden fejl.</summary>
    public bool IsSuccess => Errors.Count == 0;
}