using Microsoft.SemanticKernel;
using ResumeScreeningAgent.Services;
using ResumeScreeningAgent.Models;
using QuestPDF.Infrastructure;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ResumeScreeningAgent.Data;
using DotNetEnv;

Env.Load();


var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License =
    LicenseType.Community;

builder.Services.AddRazorPages();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string apiKey =
    Environment.GetEnvironmentVariable("GEMINI_API_KEY")!;

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: "gemini-2.5-flash",
    apiKey: apiKey
);

Kernel kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton<GeminiService>();
builder.Services.AddScoped<ResumeService>();
builder.Services.AddScoped<JobDescriptionService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<FileUploadService>();
builder.Services.AddDbContext<ResumeScreeningDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString(
                "ResumeScreeningDb")));

builder.Services.AddScoped<ICandidateRepository, CandidateRepository>();
builder.Services.AddScoped<IJobDescriptionRepository, JobDescriptionRepository>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();





app.MapGet(
"/generate-report",
async (
    ResumeService resumeService,
    GeminiService geminiService,
    JobDescriptionService jdService,
    ReportService reportService) =>
{
    string folderPath =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Resumes");

    JobDescription jd =
        jdService.ParseJobDescription(
            "Talent Acquisition Executive",
            """
            Recruitment
            Screening
            Excel
            Naukri
            LinkedIn Recruiter
            """
        );

    List<Candidate> candidates =
        resumeService.ParseAllResumes(
            folderPath);

    candidates =
        resumeService.ScoreAllCandidates(
            candidates,
            jd.RequiredSkills);

    candidates =
        resumeService.RankCandidates(
            candidates);

   foreach (Candidate candidate in candidates)
{
    if (candidate.Score >= 80)
        candidate.Recommendation = "Strong Hire";
    else if (candidate.Score >= 60)
        candidate.Recommendation = "Hire";
    else if (candidate.Score >= 40)
        candidate.Recommendation = "Consider";
    else
        candidate.Recommendation = "Reject";
}

    string reportPath =
        reportService
            .GenerateRankingReport(
                candidates);

    return Results.Ok(new
    {
        Message = "Report Generated",
        FilePath = reportPath
    });
});

app.MapGet("/download-report", () =>
{
    string reportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Reports");
    if (!Directory.Exists(reportsFolder))
    {
        return Results.NotFound("No reports folder found.");
    }

    var directory = new DirectoryInfo(reportsFolder);
    var latestFile = directory.GetFiles("CandidateRankingReport_*.pdf")
                              .OrderByDescending(f => f.LastWriteTime)
                              .FirstOrDefault();

    if (latestFile == null)
    {
        return Results.NotFound("No generated reports found.");
    }

    return Results.File(
        latestFile.FullName,
        contentType: "application/pdf",
        fileDownloadName: "CandidateRankingReport.pdf"
    );
});

app.MapPost(
"/upload-resume",
async (
    IFormFile file,
    FileUploadService uploadService) =>
{
    if (file == null ||
        file.Length == 0)
    {
        return Results.BadRequest(
            "No file uploaded.");
    }

    string path =
        await uploadService
            .UploadResumeAsync(file);

    return Results.Ok(new
    {
        Message = "Resume uploaded successfully",
        FilePath = path
    });
})
.DisableAntiforgery();

app.MapGet(
"/all-resumes",
() =>
{
    string folderPath =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Resumes");

    if (!Directory.Exists(folderPath))
    {
        return Results.Ok(
            new List<string>());
    }

 var files =
    Directory.GetFiles(folderPath)
             .Select(file => Path.GetFileName(file))
             .ToList();

    return Results.Ok(files);
});

app.MapDelete(
"/delete-resume/{fileName}",
(string fileName) =>
{
    try
    {
        string filePath =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                "Resumes",
                fileName);

        if (!File.Exists(filePath))
        {
            return Results.NotFound(new
            {
                Message = "Resume not found"
            });
        }

        File.Delete(filePath);

        return Results.Ok(new
        {
            Message = $"{fileName} deleted successfully"
        });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            Message = ex.Message
        });
    }
});

app.MapPost("/upload-resumes",
async (IFormFileCollection files) =>
{
    if (files == null || files.Count == 0)
    {
        return Results.BadRequest("No files uploaded.");
    }

    string uploadFolder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Uploads",
        "Resumes");

    Directory.CreateDirectory(uploadFolder);

    List<string> uploadedFiles = new();

    foreach (var file in files)
    {
        string filePath = Path.Combine(
            uploadFolder,
            file.FileName);

        using var stream = new FileStream(
            filePath,
            FileMode.Create);

        await file.CopyToAsync(stream);

        uploadedFiles.Add(file.FileName);
    }

    return Results.Ok(new
    {
        message = "Files uploaded successfully",
        files = uploadedFiles
    });
})
.DisableAntiforgery()
.Accepts<IFormFileCollection>("multipart/form-data");

app.MapPost("/upload-job-description",
(
    JobDescriptionRequest request,
    JobDescriptionService jobDescriptionService
) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
    {
        return Results.BadRequest(
            "Job title is required.");
    }

    if (string.IsNullOrWhiteSpace(request.Description))
    {
        return Results.BadRequest(
            "Job description is required.");
    }

    JobDescription jd =
        jobDescriptionService.ParseJobDescription(
            request.Title,
            request.Description);

    jobDescriptionService.Save(jd);

    return Results.Ok(new
    {
        Message = "Job Description uploaded successfully",
        Title = jd.Title,
        Skills = jd.RequiredSkills
    });
});

app.MapGet("/job-description",
    (JobDescriptionService jobDescriptionService) =>
{
    var jobDescription = jobDescriptionService.Get();

    if (jobDescription is null)
    {
        return Results.NotFound(new
        {
            Message = "No Job Description found"
        });
    }

    return Results.Ok(jobDescription);
});

app.MapDelete("/job-description",
    (JobDescriptionService jobDescriptionService) =>
{
    jobDescriptionService.Delete();

    return Results.Ok(new
    {
        Message = "Job Description deleted successfully"
    });
});

app.MapPost("/match-resumes-with-job",
async (
    ResumeService resumeService,
    JobDescriptionService jobService,
    GeminiService geminiService) =>
{
    JobDescription? job =
        jobService.Get();

    if (job is null)
    {
        return Results.NotFound(
            new
            {
                Message =
                    "No Job Description found"
            });
    }

    string resumeFolder =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Resumes");

    List<Candidate> candidates =
        resumeService.ParseAllResumes(
            resumeFolder);

    candidates =
        resumeService.ScoreAllCandidates(
            candidates,
            job.RequiredSkills);

    candidates =
        resumeService.RankCandidates(
            candidates);

    foreach (Candidate candidate in candidates.Take(1))
    {
         try
    {
        await geminiService.AnalyzeCandidateAsync(candidate);

        await Task.Delay(5000);
    }
    catch (Exception ex)
    {
        candidate.Summary =
            $"Gemini Error: {ex.Message}";

        candidate.Strengths = "";
        candidate.Weaknesses = "";
        candidate.Recommendation = "Not Available";
    }
    }

    return Results.Ok(CleanCandidates(candidates));
});

app.MapGet("/dashboard",
(
    ResumeService resumeService,
    JobDescriptionService jobService
) =>
{
    JobDescription? job =
        jobService.Get();

    if (job is null)
    {
        return Results.NotFound(
            new
            {
                Message = "No Job Description found"
            });
    }

    string resumeFolder =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Resumes");

    List<Candidate> candidates =
        resumeService.ParseAllResumes(
            resumeFolder);

    candidates =
        resumeService.ScoreAllCandidates(
            candidates,
            job.RequiredSkills);

    candidates =
        resumeService.RankCandidates(
            candidates);

    DashboardResponse dashboard =
        new()
        {
            TotalResumes =
                candidates.Count,

            TopCandidate =
                candidates.FirstOrDefault()?.Name
                ?? "N/A",

            AverageScore =
                SafeNumber(candidates.Any()
                    ? candidates.Average(
                        c => c.Score)
                    : 0),

            StrongHireCount =
                candidates.Count(
                    c => c.Recommendation ==
                         "Strong Hire"),

            HireCount =
                candidates.Count(
                    c => c.Recommendation ==
                         "Hire"),

            ConsiderCount =
                candidates.Count(
                    c => c.Recommendation ==
                         "Consider"),

            RejectCount =
                candidates.Count(
                    c => c.Recommendation ==
                         "Reject")
        };

    return Results.Ok(dashboard);
});

app.MapGet("/candidate/{rank}/questions",
async (
    int rank,
    ResumeService resumeService,
    JobDescriptionService jobService,
    GeminiService geminiService) =>
{
    JobDescription? job =
        jobService.Get();

    if (job is null)
    {
        return Results.NotFound(
            new
            {
                Message =
                    "No Job Description found"
            });
    }

    string resumeFolder =
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "Uploads",
            "Resumes");

    List<Candidate> candidates =
        resumeService.ParseAllResumes(
            resumeFolder);

    candidates =
        resumeService.ScoreAllCandidates(
            candidates,
            job.RequiredSkills);

    candidates =
        resumeService.RankCandidates(
            candidates);

    Candidate? candidate =
        candidates.FirstOrDefault(
            c => c.Rank == rank);

    if (candidate is null)
    {
        return Results.NotFound(
            new
            {
                Message =
                    "Candidate not found"
            });
    }

    try
    {
        List<string> questions =
            await geminiService
                .GenerateInterviewQuestionsAsync(
                    candidate);

        return Results.Ok(
            new
            {
                Candidate =
                    candidate.Name,

                Questions =
                    questions
            });
    }
   catch (Exception)
{
    return Results.Ok(
        new
        {
            Candidate = candidate.Name,
            Questions = new List<string>
            {
                "Tell us about yourself.",
                "What are your strengths and weaknesses?",
                "Why do you want to work with us?",
            }
        });
}
});

app.MapRazorPages();

app.Run();

double SafeNumber(double value)
{
    if (double.IsNaN(value) || double.IsInfinity(value))
    {
        return 0;
    }
    return value;
}

Candidate? CleanCandidate(Candidate? c)
{
    if (c != null)
    {
        c.Score = SafeNumber(c.Score);
    }
    return c;
}

List<Candidate>? CleanCandidates(List<Candidate>? list)
{
    if (list != null)
    {
        foreach (var c in list)
        {
            CleanCandidate(c);
        }
    }
    return list;
}