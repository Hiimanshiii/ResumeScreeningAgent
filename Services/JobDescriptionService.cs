using ResumeScreeningAgent.Data;
using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Services;

public class JobDescriptionService
{
    private readonly IJobDescriptionRepository _repository;

    public JobDescriptionService(IJobDescriptionRepository repository)
    {
        _repository = repository;
    }

    public JobDescription ParseJobDescription(
        string title,
        string description)
    {
        JobDescription jd = new();

        jd.Title = title;
        jd.Description = description;

        List<string> knownSkills =
        [
            "Recruitment",
            "Screening",
            "Interview Coordination",
            "Excel",
            "Naukri",
            "LinkedIn Recruiter",
            "MS Word",
            "PowerPoint"
        ];

        foreach (string skill in knownSkills)
        {
            if (description.Contains(
                skill,
                StringComparison.OrdinalIgnoreCase))
            {
                jd.RequiredSkills.Add(skill);
            }
        }

        return jd;
    }

    public void Save(JobDescription jobDescription)
    {
        _repository.SaveJobDescriptionAsync(jobDescription).GetAwaiter().GetResult();
    }

    public JobDescription? Get()
    {
        return _repository.GetJobDescriptionAsync().GetAwaiter().GetResult();
    }

    public void Delete()
    {
        _repository.DeleteJobDescriptionAsync().GetAwaiter().GetResult();
    }
}