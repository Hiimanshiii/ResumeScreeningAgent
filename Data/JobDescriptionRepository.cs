using Microsoft.EntityFrameworkCore;
using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Data;

public class JobDescriptionRepository : IJobDescriptionRepository
{
    private readonly ResumeScreeningDbContext _context;

    public JobDescriptionRepository(ResumeScreeningDbContext context)
    {
        _context = context;
    }

    public async Task SaveJobDescriptionAsync(JobDescription jobDescription)
    {
        // Ensure only one active Job Description exists at a time by clearing the table first
        await _context.JobDescriptions.ExecuteDeleteAsync();

        // Add the new job description
        await _context.JobDescriptions.AddAsync(jobDescription);
        await _context.SaveChangesAsync();
    }

    public async Task<JobDescription?> GetJobDescriptionAsync()
    {
        return await _context.JobDescriptions.FirstOrDefaultAsync();
    }

    public async Task DeleteJobDescriptionAsync()
    {
        await _context.JobDescriptions.ExecuteDeleteAsync();
    }
}
