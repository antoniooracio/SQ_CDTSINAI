using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DocumentTypeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DocumentTypeController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<DocumentType>>> GetDocumentTypes([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
        {
            var query = _context.DocumentTypes.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => t.Name.Contains(search) || t.ResponsibleArea.Contains(search));
            }

            var totalCount = await query.CountAsync();
            var items = await query.OrderBy(t => t.Name)
                                   .Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PaginatedResult<DocumentType> 
            { 
                Items = items, 
                TotalCount = totalCount, 
                PageNumber = page, 
                PageSize = pageSize 
            };
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentType>> GetDocumentType(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType == null) return NotFound();
            return documentType;
        }

        [HttpPost]
        public async Task<ActionResult<DocumentType>> PostDocumentType(DocumentType documentType)
        {
            _context.DocumentTypes.Add(documentType);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDocumentType), new { id = documentType.Id }, documentType);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocumentType(int id, DocumentType documentType)
        {
            if (id != documentType.Id) return BadRequest();

            _context.Entry(documentType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DocumentTypes.Any(e => e.Id == id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocumentType(int id)
        {
            var documentType = await _context.DocumentTypes.FindAsync(id);
            if (documentType == null) return NotFound();

            // Validação: Verificar se está em uso por algum estabelecimento
            bool isInUse = await _context.EstablishmentDocuments.AnyAsync(ed => ed.DocumentTypeId == id);
            if (isInUse)
            {
                return BadRequest("Não é possível excluir este tipo de documento pois ele está sendo utilizado por um ou mais estabelecimentos.");
            }

            _context.DocumentTypes.Remove(documentType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}