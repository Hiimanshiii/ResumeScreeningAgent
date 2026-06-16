using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeScreeningAgent.Models;

public class JobDescription
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public List<string> RequiredSkills { get; set; } = new();

    public string Description { get; set; } = "";
}