namespace NPS.Api.Application.Common.Nps;

// Fórmula NPS habitual: 9–10 promotores, 7–8 pasivos, ≤6 detractores; índice = %prom − %detr.
public static class NpsCalculator
{
    public static NpsComputationResult Compute(IReadOnlyCollection<int> scores)
    {
        var total = scores.Count;
        if (total == 0) return NpsComputationResult.Empty;

        var promoters = scores.Count(x => x >= 9);
        var detractors = scores.Count(x => x <= 6);
        var passives = scores.Count(x => x is 7 or 8);

        var nps = (double)(promoters - detractors) / total * 100;
        var pPerc = (double)promoters / total * 100;
        var dPerc = (double)detractors / total * 100;
        var passPerc = (double)passives / total * 100;

        return new NpsComputationResult(
            Math.Round(nps, 2),
            Math.Round(pPerc, 2),
            Math.Round(dPerc, 2),
            Math.Round(passPerc, 2),
            total);
    }
}
