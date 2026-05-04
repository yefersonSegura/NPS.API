using NPS.Api.Application.Common.Nps;

namespace NPS.Api.UnitTests;

// Regresión ligera: si alguien mueve buckets o redondeo, estos tres casos gritan rápido.
public sealed class NpsCalculatorTests
{
    [Fact]
    public void Compute_empty_returns_zeros()
    {
        var r = NpsCalculator.Compute(Array.Empty<int>());

        Assert.Equal(0, r.TotalResponses);
        Assert.Equal(0, r.NpsScore);
        Assert.Equal(0, r.PromotersPercentage);
        Assert.Equal(0, r.PassivesPercentage);
        Assert.Equal(0, r.DetractorsPercentage);
    }

    [Fact]
    public void Compute_one_detractor_nps_minus_100()
    {
        var r = NpsCalculator.Compute(new[] { 2 });

        Assert.Equal(1, r.TotalResponses);
        Assert.Equal(100, r.DetractorsPercentage);
        Assert.Equal(0, r.PromotersPercentage);
        Assert.Equal(0, r.PassivesPercentage);
        Assert.Equal(-100, r.NpsScore);
    }

    [Fact]
    public void Compute_mixed_matches_dev001_formula()
    {
        // 1 promotor (10), 1 pasivo (8), 1 detractor (4) → NPS = (33.33 - 33.33) ≈ 0
        var r = NpsCalculator.Compute(new[] { 10, 8, 4 });

        Assert.Equal(3, r.TotalResponses);
        Assert.Equal(33.33, r.PromotersPercentage, 2);
        Assert.Equal(33.33, r.PassivesPercentage, 2);
        Assert.Equal(33.33, r.DetractorsPercentage, 2);
        Assert.Equal(0, r.NpsScore, 2);
    }
}
