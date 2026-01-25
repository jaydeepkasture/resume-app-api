using System.ComponentModel.DataAnnotations;

namespace ResumeInOneMinute.Domain.DTO;

public class ResumeDto
{
    public string Name { get; set; } = string.Empty;
    
    public string PhoneNo { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;
    
    public string LinkedIn { get; set; } = string.Empty;
    
    public string GitHub { get; set; } = string.Empty;
    
    public string Summary { get; set; } = string.Empty;
    
    public List<ExperienceDto> Experience { get; set; } = new();
    
    public List<string> Skills { get; set; } = new();
    
    public List<EducationDto> Education { get; set; } = new();
}

public class ExperienceDto
{
    public string Company { get; set; } = string.Empty;
    
    public string Position { get; set; } = string.Empty;
    
    public string From { get; set; } = string.Empty;
    
    public string To { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
}

public class EducationDto
{
    public string Degree { get; set; } = string.Empty;
    
    public string Field { get; set; } = string.Empty;
    
    public string Institution { get; set; } = string.Empty;
    
    public string Year { get; set; } = string.Empty;
}
