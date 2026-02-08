namespace ResumeInOneMinute.Domain.Constance;

/// <summary>
/// Constants for MongoDB collection names
/// </summary>
public static class MongoCollections
{
    /// <summary>
    /// Collection for storing resume enhancement history
    /// </summary>
    public const string ResumeEnhancementHistory = "resume_enhancement_history";
    
    /// <summary>
    /// Collection for storing application logs
    /// </summary>
    public const string ApplicationLogs = "application_logs";
    
    /// <summary>
    /// Collection for storing audit trails
    /// </summary>
    public const string AuditTrails = "audit_trails";
    
    /// <summary>
    /// Collection for storing user activity logs
    /// </summary>
    public const string UserActivityLogs = "user_activity_logs";
    
    /// <summary>
    /// Collection for storing chat sessions for resume enhancement
    /// </summary>
    public const string ChatSessions = "chat_sessions";
    public const string HtmlTemplates = "html_templates";
    
    /// <summary>
    /// Collection for storing user master resumes
    /// </summary>
    public const string Resume = "resume";
}
