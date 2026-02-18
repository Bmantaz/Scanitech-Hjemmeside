namespace Scanitech_Logic.Models;

/// <summary>
/// Standardiseret resultat for alle forretningsoperationer (5.0 Standard).
/// </summary>
public sealed record OperationResult(
    int SuccessCount,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings)
{
    public bool HasErrors => Errors.Count > 0;
    public bool IsSuccess => Errors.Count == 0;
}