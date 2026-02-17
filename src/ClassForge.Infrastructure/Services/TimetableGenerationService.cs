using ClassForge.Application.Interfaces;
using ClassForge.Domain.Entities;
using ClassForge.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClassForge.Infrastructure.Services;

public class TimetableGenerationService : BackgroundService
{
    private readonly TimetableGenerationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TimetableGenerationService> _logger;

    public TimetableGenerationService(
        TimetableGenerationQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<TimetableGenerationService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timetable generation service started.");

        await foreach (var request in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessRequestAsync(request, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing timetable generation for {TimetableId}", request.TimetableId);
            }
        }
    }

    private async Task ProcessRequestAsync(TimetableGenerationRequest request, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();

        // Set tenant context for the background scope
        var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
        tenantProvider.SetTenantId(request.TenantId);

        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var generator = scope.ServiceProvider.GetRequiredService<ITimetableGenerator>();
        var inputBuilder = scope.ServiceProvider.GetRequiredService<SchedulingInputBuilder>();

        var timetable = await db.Timetables.FindAsync([request.TimetableId], ct);
        if (timetable is null)
        {
            _logger.LogWarning("Timetable {TimetableId} not found.", request.TimetableId);
            return;
        }

        try
        {
            _logger.LogInformation("Starting generation for timetable {TimetableId}", request.TimetableId);

            var input = await inputBuilder.BuildAsync(ct);
            var result = await generator.GenerateAsync(input, null, ct);

            if (result.Success)
            {
                timetable.Status = TimetableStatus.Draft;
                timetable.GeneratedAt = DateTime.UtcNow;
                timetable.QualityScore = result.QualityScore;

                foreach (var entry in result.Entries)
                {
                    var timetableEntry = new TimetableEntry
                    {
                        Id = Guid.NewGuid(),
                        TimetableId = timetable.Id,
                        TimeSlotId = entry.TimeSlotId,
                        SubjectId = entry.SubjectId,
                        TeacherId = entry.TeacherId,
                        RoomId = entry.RoomId,
                        IsDoublePeriod = entry.IsDoublePeriod,
                        CombinedLessonGroupId = entry.CombinedLessonGroupId,
                        Groups = entry.GroupIds.Select(gid => new TimetableEntryGroup
                        {
                            GroupId = gid
                        }).ToList()
                    };
                    db.TimetableEntries.Add(timetableEntry);
                }
            }
            else
            {
                timetable.Status = TimetableStatus.Failed;
                timetable.ErrorMessage = "Scheduling algorithm could not find a valid solution.";
            }

            // Persist reports
            foreach (var report in result.Reports)
            {
                var reportType = Enum.TryParse<ReportType>(report.Type, out var rt) ? rt : ReportType.Info;
                db.TimetableReports.Add(new TimetableReport
                {
                    Id = Guid.NewGuid(),
                    TimetableId = timetable.Id,
                    Type = reportType,
                    Category = report.Category,
                    Message = report.Message,
                    RelatedEntityType = report.RelatedEntityType,
                    RelatedEntityId = report.RelatedEntityId
                });
            }

            await db.SaveChangesAsync(ct);
            _logger.LogInformation("Timetable {TimetableId} generation completed. Success: {Success}", request.TimetableId, result.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Generation failed for timetable {TimetableId}", request.TimetableId);
            timetable.Status = TimetableStatus.Failed;
            timetable.ErrorMessage = ex.Message;
            await db.SaveChangesAsync(ct);
        }
    }
}
