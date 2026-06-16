using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Data;

public interface IJobDescriptionRepository
{
    Task SaveJobDescriptionAsync(JobDescription jobDescription);
    Task<JobDescription?> GetJobDescriptionAsync();
    Task DeleteJobDescriptionAsync();
}
