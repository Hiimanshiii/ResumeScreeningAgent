using Microsoft.AspNetCore.Http;

namespace ResumeScreeningAgent.Services;

public class FileUploadService
{
    private readonly string _resumeFolder;

    public FileUploadService()
    {
        _resumeFolder =
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "Uploads",
                "Resumes");

        Directory.CreateDirectory(
            _resumeFolder);
    }

    public async Task<string> UploadResumeAsync(
        IFormFile file)
    {
        string filePath =
            Path.Combine(
                _resumeFolder,
                file.FileName);

        using FileStream stream =
            new(
                filePath,
                FileMode.Create);

        await file.CopyToAsync(stream);

        return filePath;
    }

    public List<string> GetAllResumes()
    {
        return Directory
            .GetFiles(_resumeFolder)
            .Select(Path.GetFileName)
            .ToList()!;
    }

    public bool DeleteResume(
        string fileName)
    {
        string filePath =
            Path.Combine(
                _resumeFolder,
                fileName);

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);

        return true;
    }
}