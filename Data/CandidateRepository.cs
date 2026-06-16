using Microsoft.EntityFrameworkCore;
using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Data;

public class CandidateRepository : ICandidateRepository
{
    private readonly ResumeScreeningDbContext _context;

    public CandidateRepository(ResumeScreeningDbContext context)
    {
        _context = context;
    }

    public async Task AddCandidateAsync(Candidate candidate)
    {
        await _context.Candidates.AddAsync(candidate);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Candidate>> GetAllCandidatesAsync()
    {
        return await _context.Candidates.ToListAsync();
    }

    public async Task<Candidate?> GetCandidateByIdAsync(int id)
    {
        return await _context.Candidates.FindAsync(id);
    }

    public async Task DeleteCandidateAsync(int id)
    {
        await _context.Candidates
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task DeleteAllCandidatesAsync()
    {
        await _context.Candidates.ExecuteDeleteAsync();
    }
}
