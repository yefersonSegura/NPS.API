namespace NPS.Api.Domain.Entities;

public class SurveyResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; }
    public DateTime CreatedAt { get; set; }
}
