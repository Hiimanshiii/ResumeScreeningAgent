using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ResumeScreeningAgent.Models;

public class Candidate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string Phone { get; set; } = "";

    public List<string> Skills { get; set; } = new();

    public string Experience { get; set; } = "";

    public string Education { get; set; } = "";

    public double Score { get; set; }

    public string Summary { get; set; } = "";

public string Strengths { get; set; } = "";

public string Weaknesses { get; set; } = "";

public string Recommendation { get; set; } = "";

public int Rank { get; set; }

public List<string> MatchedSkills { get; set; } = new();

public List<string> MissingSkills { get; set; } = new();

}