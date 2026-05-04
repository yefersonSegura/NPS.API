using NPS.Api.Application.Services.Survey;

namespace NPS.Api.UnitTests;

public sealed class CreateSurveyResponseValidatorTests
{
    private readonly CreateSurveyResponseValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void Score_in_range_valid(int score)
    {
        var r = _validator.Validate(new CreateSurveyResponseCommand(score));

        Assert.True(r.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void Score_out_of_range_invalid(int score)
    {
        var r = _validator.Validate(new CreateSurveyResponseCommand(score));

        Assert.False(r.IsValid);
        Assert.Contains(r.Errors, e => e.PropertyName == nameof(CreateSurveyResponseCommand.Score));
    }
}
