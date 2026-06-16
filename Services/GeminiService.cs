using Microsoft.SemanticKernel;
using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Services;

public class GeminiService
{
    private readonly Kernel _kernel;

    public GeminiService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> AskAsync(string prompt)
    {
        var result = await _kernel.InvokePromptAsync(prompt);

        return result.ToString();
    }

    public async Task<string> AnalyzeResumeAsync(
    string resumeText)
{
    string prompt = $"""
Analyze this resume.

Provide:

1. Candidate Summary
2. Top Skills
3. Strengths
4. Weaknesses
5. Hiring Recommendation

Resume:

{resumeText}
""";

    return await AskAsync(prompt);
}

public async Task<List<string>>
    GenerateInterviewQuestionsAsync(
        Candidate candidate)
{
    string prompt = $"""
You are a senior HR recruiter.

Generate 5 interview questions for
this candidate.

Skills:
{string.Join(", ", candidate.Skills)}

Experience:
{candidate.Experience}

Education:
{candidate.Education}

Return ONLY the questions.
One question per line.
""";

    string result =
        await AskAsync(prompt);

    return result
        .Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries)
        .Select(q => q.Trim())
        .ToList();
}

public async Task<string> GenerateRecommendationAsync(
    Candidate candidate)
{
    string prompt = $"""
You are an HR recruiter.

Candidate Name:
{candidate.Name}

Skills:
{string.Join(", ", candidate.Skills)}

Experience:
{candidate.Experience}

Education:
{candidate.Education}

Score:
{candidate.Score}

Provide ONLY ONE of:

Strong Hire
Hire
Consider
Reject
""";

    return await AskAsync(prompt);
}

public async Task<Candidate> AnalyzeCandidateAsync(
    Candidate candidate)
{
    string prompt = $"""
You are an expert HR recruiter.

Analyze this candidate.

Name:
{candidate.Name}

Skills:
{string.Join(", ", candidate.Skills)}

Experience:
{candidate.Experience}

Education:
{candidate.Education}

Score:
{candidate.Score}

Provide EXACTLY in this format:

SUMMARY:
<summary>

STRENGTHS:
<strengths>

WEAKNESSES:
<weaknesses>

RECOMMENDATION:
<Strong Hire/Hire/Consider/Reject>
""";

    string result =
        await AskAsync(prompt);

    candidate.Summary =
        ExtractSection(
            result,
            "SUMMARY:",
            "STRENGTHS:");

    candidate.Strengths =
        ExtractSection(
            result,
            "STRENGTHS:",
            "WEAKNESSES:");

    candidate.Weaknesses =
        ExtractSection(
            result,
            "WEAKNESSES:",
            "RECOMMENDATION:");

    int recommendationIndex =
        result.IndexOf(
            "RECOMMENDATION:",
            StringComparison.OrdinalIgnoreCase);

    if (recommendationIndex >= 0)
    {
        candidate.Recommendation =
            result[(recommendationIndex +
                    "RECOMMENDATION:".Length)..]
                .Trim();
    }

    return candidate;
}

private string ExtractSection(
    string text,
    string start,
    string end)
{
    int startIndex =
        text.IndexOf(
            start,
            StringComparison.OrdinalIgnoreCase);

    int endIndex =
        text.IndexOf(
            end,
            StringComparison.OrdinalIgnoreCase);

    if (startIndex < 0 ||
        endIndex < 0)
    {
        return "";
    }

    startIndex += start.Length;

    return text
        .Substring(
            startIndex,
            endIndex - startIndex)
        .Trim();
}


}

