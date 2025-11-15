using System.Collections.Generic;
using SIGEBI.Application.Models;

namespace SIGEBI.Web.Models;

public sealed class HomeDashboardViewModel
{
    public required AdminSummary Summary { get; init; }
    public required IReadOnlyList<DashboardModule> Modules { get; init; }
    public required IReadOnlyList<ActivityItem> Activity { get; init; }
    public required IReadOnlyList<HeroLink> HeroLinks { get; init; }
    public required IReadOnlyList<SpotlightModule> SpotlightModules { get; init; }
}

public sealed record DashboardModule(
    string Title,
    string Description,
    string Icon,
    string ColorClass,
    string Category,
    string? Controller = null,
    string? Action = null,
    string? Page = null);

public sealed record ActivityItem(
    string Title,
    string Details,
    string TimeAgo,
    string ColorClass);

public sealed record HeroLink(
    string Label,
    string Icon,
    string? Controller = null,
    string? Action = null,
    string? Page = null);

public sealed record SpotlightModule(
    string Id,
    string Title,
    string Description,
    string ButtonText,
    string ButtonClass,
    string? Controller = null,
    string? Action = null,
    string? Page = null);
