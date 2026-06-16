using UglyToad.PdfPig;
using Microsoft.SemanticKernel;
using System.Text.RegularExpressions;

namespace ResumeScreeningAgent.Plugins;

public class ResumePlugin
{
    [KernelFunction]
    public string ReadResume(string filePath)
    {
        using PdfDocument document =
            PdfDocument.Open(filePath);

        string text = "";

        foreach (var page in document.GetPages())
        {
            text += page.Text;
        }

        return text;
    }

    [KernelFunction]
public List<string> ExtractSkills(string resumeText)
{
    List<string> skills = new();

    string[] knownSkills =
    {
        "Excel",
        "Recruitment",
        "Screening",
        "LinkedIn Recruiter",
        "Naukri",
        "MS Word",
        "PowerPoint",
        "Interview Coordination"
    };

    foreach (var skill in knownSkills)
    {
        if (resumeText.Contains(skill,
            StringComparison.OrdinalIgnoreCase))
        {
            skills.Add(skill);
        }
    }

    return skills;
}

[KernelFunction]
public string ExtractEmail(string text)
{
    var match = Regex.Match(
        text,
        @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.(com|in|org|net)");

    return match.Success
        ? match.Value
        : "";
}

[KernelFunction]
public string ExtractPhone(string resumeText)
{
    var match = System.Text.RegularExpressions.Regex.Match(
        resumeText,
        @"(\+91\s?)?[6-9]\d{9}"
    );

    return match.Success ? match.Value : "";
}

[KernelFunction]
public string ExtractName(string text)
{
    var match = Regex.Match(
        text,
        @"^[A-Z\s]+");

    if (!match.Success)
    {
        return "";
    }

    string name = match.Value.Trim();

    if (name.EndsWith("N"))
    {
        name = name[..^1];
    }

    return name.Trim();
}

[KernelFunction]
public string ExtractEducation(string text)
{
    var match =
        System.Text.RegularExpressions.Regex.Match(
            text,
            @"EDUCATION(.*?)TOOLS",
            System.Text.RegularExpressions.RegexOptions.Singleline |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return match.Success
        ? match.Groups[1].Value.Trim()
        : "";
}

[KernelFunction]
public string ExtractExperience(string text)
{
    var match =
        System.Text.RegularExpressions.Regex.Match(
            text,
            @"PROFESSIONAL EXPERIENCE(.*?)EDUCATION",
            System.Text.RegularExpressions.RegexOptions.Singleline |
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

    return match.Success
        ? match.Groups[1].Value.Trim()
        : "";
}
    [KernelFunction]
    public double CalculateScore(
        List<string> candidateSkills,
        List<string> requiredSkills)
    {
        if (requiredSkills == null || requiredSkills.Count == 0)
        {
            return 0;
        }

        int matched =
            candidateSkills
            .Count(skill =>
                requiredSkills.Contains(
                    skill,
                    StringComparer.OrdinalIgnoreCase));

        double score = (double)matched / requiredSkills.Count * 100;
        if (double.IsNaN(score) || double.IsInfinity(score))
        {
            return 0;
        }
        return score;
    }
}