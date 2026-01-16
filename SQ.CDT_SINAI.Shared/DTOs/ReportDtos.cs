using System;
using System.Collections.Generic;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.Shared.DTOs
{
    public class ReportFilterDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Regional { get; set; }
        public string? Brand { get; set; }
        public string? Unit { get; set; }
        public List<int> Categories { get; set; } = new();
        public List<int> Statuses { get; set; } = new();
        public List<int> Severities { get; set; } = new();
        public List<int> Types { get; set; } = new();
    }

    public class ReportChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<double> Values { get; set; } = new();
    }

    public class ReportResultDto
    {
        public int TotalCount { get; set; }
        public ReportChartData StatusChart { get; set; } = new();
        public ReportChartData CategoryChart { get; set; } = new();
        public ReportChartData IncidentsByMonth { get; set; } = new();
        public ReportChartData AverageTimeByArea { get; set; } = new();
        public ReportChartData IncidentsByArea { get; set; } = new();
    }

    public class LegalizationReportFilterDto
    {
        public List<int> EstablishmentIds { get; set; } = new();
        public List<int> BrandIds { get; set; } = new();
        public string? Regional { get; set; }
        public List<ServiceLocationType> ServiceLocationTypes { get; set; } = new();
        public bool ComplianceOnly { get; set; }
        public List<DocumentStatus> Statuses { get; set; } = new();
    }

    public class StackedChartData
    {
        public List<string> Labels { get; set; } = new();
        public List<ChartDataset> Datasets { get; set; } = new();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = string.Empty;
        public List<int> Data { get; set; } = new();
        public string BackgroundColor { get; set; } = string.Empty;
    }

    public class LegalizationReportResultDto
    {
        public int TotalEstablishmentsFound { get; set; }
        public StackedChartData DocumentsByArea { get; set; } = new();
        public StackedChartData DocumentsByType { get; set; } = new();
    }
}