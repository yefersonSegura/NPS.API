namespace NPS.Api.Application.Common.Nps;

// Números "de dominio" antes de cruzar a NpsResultsDto (perfil AutoMapper en Application).
public record NpsComputationResult(
    double NpsScore,
    double PromotersPercentage,
    double DetractorsPercentage,
    double PassivesPercentage,
    int TotalResponses)
{
    public static NpsComputationResult Empty { get; } = new(0, 0, 0, 0, 0);
}
