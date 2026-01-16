using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;
using System.Collections.Generic;

namespace SQ.CDT_SINAI.Web.Models.ViewModels
{
    public class EstablishmentLegalizationViewModel
    {
        public Establishment Establishment { get; set; }
        public PaginatedResult<DocumentType> DocumentTypes { get; set; } = new();
        public List<EstablishmentDocument> ExistingDocuments { get; set; } = new();
    }
}
