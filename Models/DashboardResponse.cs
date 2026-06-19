namespace ResumeScreeningAgent.Models;

public class DashboardResponse
{
    public int TotalResumes { get; set; }

    public string TopCandidate { get; set; } = "";

    public double AverageScore { get; set; }

    public int StrongHireCount { get; set; }

    public int HireCount { get; set; }

    public int ConsiderCount { get; set; }

    public int RejectCount { get; set; }

    public string Message { get; set; } = "";
}