using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;
using System;

namespace SQ.CDT_SINAI.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalCollaborators { get; set; }
        public int TotalSpecializations { get; set; }
        public PaginatedResult<Incident> Incidents { get; set; } = new();
        public Dictionary<string, int> IncidentStats { get; set; } = new();
        public IncidentStatus CurrentFilter { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DocumentExpirationStatsDto DocumentStats { get; set; } = new();
    }
}