using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Data;

public interface ICandidateRepository
{
    Task AddCandidateAsync(Candidate candidate);
    Task<List<Candidate>> GetAllCandidatesAsync();
    Task<Candidate?> GetCandidateByIdAsync(int id);
    Task DeleteCandidateAsync(int id);
    Task DeleteAllCandidatesAsync();
}
