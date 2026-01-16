using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("incidents")]
        public async Task<ActionResult<ReportResultDto>> GetIncidentReport(ReportFilterDto filter)
        {
            var query = _context.Incidents.AsQueryable();

            if (filter.StartDate.HasValue)
                query = query.Where(i => i.CreatedAt >= filter.StartDate.Value);
            
            if (filter.EndDate.HasValue)
                query = query.Where(i => i.CreatedAt <= filter.EndDate.Value.AddDays(1));

            if (!string.IsNullOrEmpty(filter.Regional))
                query = query.Where(i => i.InvolvedRegional == filter.Regional);

            if (!string.IsNullOrEmpty(filter.Brand))
                query = query.Where(i => i.InvolvedBrand == filter.Brand);

            if (!string.IsNullOrEmpty(filter.Unit))
                query = query.Where(i => i.LocationOrUnit != null && i.LocationOrUnit.Contains(filter.Unit));

            if (filter.Categories != null && filter.Categories.Any())
                query = query.Where(i => filter.Categories.Contains((int)i.Category));

            if (filter.Statuses != null && filter.Statuses.Any())
                query = query.Where(i => filter.Statuses.Contains((int)i.Status));

            if (filter.Severities != null && filter.Severities.Any())
                query = query.Where(i => filter.Severities.Contains((int)i.Severity));

            if (filter.Types != null && filter.Types.Any())
                query = query.Where(i => filter.Types.Contains((int)i.Type));

            var incidents = await query.ToListAsync();

            var result = new ReportResultDto
            {
                TotalCount = incidents.Count
            };

            // 1. Status Chart (Pie)
            var statusGroups = incidents.GroupBy(i => i.Status);
            foreach (var group in statusGroups)
            {
                result.StatusChart.Labels.Add(group.Key.ToString());
                result.StatusChart.Values.Add(group.Count());
            }

            // 2. Category Chart (Pie)
            var categoryGroups = incidents.GroupBy(i => i.Category);
            foreach (var group in categoryGroups)
            {
                result.CategoryChart.Labels.Add(group.Key.ToString());
                result.CategoryChart.Values.Add(group.Count());
            }

            // 3. Incidents by Month (Column)
            var monthGroups = incidents
                .OrderBy(i => i.CreatedAt)
                .GroupBy(i => i.CreatedAt.ToString("MM/yyyy"));
            
            foreach (var group in monthGroups)
            {
                result.IncidentsByMonth.Labels.Add(group.Key);
                result.IncidentsByMonth.Values.Add(group.Count());
            }

            // 4. Incidents by Area (Bar)
            var areaGroups = incidents
                .Where(i => !string.IsNullOrEmpty(i.TargetArea))
                .GroupBy(i => i.TargetArea);

            foreach (var group in areaGroups)
            {
                result.IncidentsByArea.Labels.Add(group.Key!);
                result.IncidentsByArea.Values.Add(group.Count());
            }

            // 5. Average Resolution Time by Area (Line/Column)
            var resolvedIncidents = incidents
                .Where(i => (i.Status == IncidentStatus.Responded || i.Status == IncidentStatus.Closed) && i.ResponseDate.HasValue && !string.IsNullOrEmpty(i.TargetArea))
                .GroupBy(i => i.TargetArea);

            foreach (var group in resolvedIncidents)
            {
                var avgHours = group.Average(i => (i.ResponseDate!.Value - i.CreatedAt).TotalHours);
                result.AverageTimeByArea.Labels.Add(group.Key!);
                result.AverageTimeByArea.Values.Add(Math.Round(avgHours, 2));
            }

            return result;
        }
    }
}