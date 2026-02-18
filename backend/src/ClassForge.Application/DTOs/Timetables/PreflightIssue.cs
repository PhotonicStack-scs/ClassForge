namespace ClassForge.Application.DTOs.Timetables;

public record PreflightIssue(string Severity, string Category, string Message, string? EntityType, Guid? EntityId);

public record PreflightResponse(bool IsValid, int ErrorCount, int WarningCount, List<PreflightIssue> Issues);
