using Microsoft.AspNetCore.Http;

namespace ResumeScreeningAgent.Models;

public class MultipleResumeUploadRequest
{
    public List<IFormFile> Files { get; set; } = new();
}