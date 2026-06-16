using Microsoft.EntityFrameworkCore;
using ResumeScreeningAgent.Models;

namespace ResumeScreeningAgent.Data;

public class ResumeScreeningDbContext : DbContext
{
    public ResumeScreeningDbContext(DbContextOptions<ResumeScreeningDbContext> options)
        : base(options)
    {
    }

    public DbSet<Candidate> Candidates { get; set; }

    public DbSet<JobDescription> JobDescriptions { get; set; }
}
