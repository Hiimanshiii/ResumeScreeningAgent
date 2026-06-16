using ResumeScreeningAgent.Plugins;
using ResumeScreeningAgent.Models;
using ResumeScreeningAgent.Data;

namespace ResumeScreeningAgent.Services;

public class ResumeService
{
    private readonly ResumePlugin _resumePlugin;
    private readonly ICandidateRepository _candidateRepository;

    public ResumeService(ICandidateRepository candidateRepository)
    {
        _resumePlugin = new ResumePlugin();
        _candidateRepository = candidateRepository;
    }

    private async Task<Candidate> PersistOrUpdateCandidateAsync(Candidate candidate)
    {
        if (!string.IsNullOrWhiteSpace(candidate.Email))
        {
            var existingCandidates = await _candidateRepository.GetAllCandidatesAsync();
            var existing = existingCandidates.FirstOrDefault(c => c.Email.Equals(candidate.Email, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                await _candidateRepository.DeleteCandidateAsync(existing.Id);
            }
        }

        candidate.Id = 0; // Ensure EF Core treats this as a new insert
        await _candidateRepository.AddCandidateAsync(candidate);
        return candidate;
    }

    public string ReadResume(string path)
    {
        return _resumePlugin.ReadResume(path);
    }

    public Candidate ParseResume(string path)
    {
        string text = _resumePlugin.ReadResume(path);

        Candidate candidate = new();

        candidate.Name =
            _resumePlugin.ExtractName(text);

        candidate.Email =
            _resumePlugin.ExtractEmail(text);

        candidate.Phone =
            _resumePlugin.ExtractPhone(text);

        candidate.Skills =
            _resumePlugin.ExtractSkills(text);

        candidate.Experience =
            _resumePlugin.ExtractExperience(text);

        candidate.Education =
            _resumePlugin.ExtractEducation(text);

        return PersistOrUpdateCandidateAsync(candidate).GetAwaiter().GetResult();
    }

    public double CalculateSkillMatch(
        Candidate candidate,
        List<string> requiredSkills)
    {
        if (requiredSkills == null || requiredSkills.Count == 0)
        {
            return 0;
        }

        int matchedSkills = candidate.Skills
            .Count(skill =>
                requiredSkills.Any(req =>
                    req.Equals(
                        skill,
                        StringComparison.OrdinalIgnoreCase)));

        double match = (double)matchedSkills / requiredSkills.Count * 100;
        if (double.IsNaN(match) || double.IsInfinity(match))
        {
            return 0;
        }
        return match;
    }

  public Candidate ScoreCandidate(
    Candidate candidate,
    List<string> requiredSkills)
{
    candidate.Score =
        _resumePlugin.CalculateScore(
            candidate.Skills,
            requiredSkills);

    candidate.MatchedSkills =
        candidate.Skills
            .Where(skill =>
                requiredSkills.Any(req =>
                    req.Equals(
                        skill,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();

    candidate.MissingSkills =
        requiredSkills
            .Where(req =>
                !candidate.Skills.Any(skill =>
                    skill.Equals(
                        req,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();

    return PersistOrUpdateCandidateAsync(candidate).GetAwaiter().GetResult();
}

    public List<Candidate> ParseAllResumes(
        string folderPath)
    {
        List<Candidate> candidates = new();

        string[] files =
            Directory.GetFiles(
                folderPath,
                "*.pdf");

        foreach (string file in files)
        {
            Candidate candidate =
                ParseResume(file);

            candidates.Add(candidate);
        }

        return candidates;
    }

  public List<Candidate> ScoreAllCandidates(
    List<Candidate> candidates,
    List<string> requiredSkills)
{
    foreach (Candidate candidate in candidates)
    {
        ScoreCandidate(
            candidate,
            requiredSkills);
    }

    return candidates;
}

   public List<Candidate> RankCandidates(
    List<Candidate> candidates)
{
    List<Candidate> ranked =
        candidates
            .OrderByDescending(c => c.Score)
            .ToList();

    for (int i = 0; i < ranked.Count; i++)
    {
        ranked[i].Rank = i + 1;
        ranked[i] = PersistOrUpdateCandidateAsync(ranked[i]).GetAwaiter().GetResult();
    }

    return ranked;
}
}