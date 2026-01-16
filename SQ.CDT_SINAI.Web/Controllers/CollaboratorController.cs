using Microsoft.AspNetCore.Mvc;
using SQ.CDT_SINAI.Shared.DTOs;
using SQ.CDT_SINAI.Web.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.Web.Controllers
{
    public class CollaboratorController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ISpecializationService _specializationService;
        private readonly ICollaboratorService _collaboratorService;
        private readonly IRoleService _roleService;
        private readonly IEstablishmentService _establishmentService;

        public CollaboratorController(IAuthService authService, ISpecializationService specializationService, ICollaboratorService collaboratorService, IRoleService roleService, IEstablishmentService establishmentService)
        {
            _authService = authService;
            _specializationService = specializationService;
            _collaboratorService = collaboratorService;
            _roleService = roleService;
            _establishmentService = establishmentService;
        }

        public async Task<IActionResult> Index(int page = 1, string? search = null)
        {
            ViewData["CurrentFilter"] = search;
            var collaborators = await _collaboratorService.GetAllAsync(page, 10, search);
            return View(collaborators);
        }

        public async Task<IActionResult> Create()
        {
            // Carrega especializações para o dropdown/checkboxes
            ViewBag.Specializations = (await _specializationService.GetAllAsync(1, 100)).Items; // Carrega todas (ou um limite alto) para o dropdown
            ViewBag.Roles = (await _roleService.GetAllAsync(1, 100)).Items;
            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterCollaboratorDto model)
        {
            if (ModelState.IsValid)
            {
                var error = await _collaboratorService.CreateAsync(model);
                if (error == null)
                    return RedirectToAction(nameof(Index));
                
                ModelState.AddModelError(string.Empty, error);
            }
            ViewBag.Specializations = (await _specializationService.GetAllAsync(1, 100)).Items;
            ViewBag.Roles = (await _roleService.GetAllAsync(1, 100)).Items;
            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var collaborator = await _collaboratorService.GetByIdAsync(id);
            if (collaborator == null) return NotFound();

            var dto = new UpdateCollaboratorDto
            {
                Id = collaborator.Id,
                Name = collaborator.Name,
                Email = collaborator.Email,
                Cpf = collaborator.Cpf,
                Rg = collaborator.Rg,
                BirthDate = collaborator.BirthDate,
                PhoneNumber = collaborator.PhoneNumber,
                JobTitle = collaborator.JobTitle,
                ProfessionalLicense = collaborator.ProfessionalLicense,
                Active = collaborator.Active,
                AddressStreet = collaborator.AddressStreet,
                AddressNumber = collaborator.AddressNumber,
                AddressNeighborhood = collaborator.AddressNeighborhood,
                AddressCity = collaborator.AddressCity,
                AddressState = collaborator.AddressState,
                AddressZipCode = collaborator.AddressZipCode,
                SpecializationIds = collaborator.Specializations.Select(s => s.Id).ToList(),
                RoleId = collaborator.RoleId ?? 0,
                EstablishmentIds = collaborator.Establishments.Select(e => e.Id).ToList()
            };

            ViewBag.Specializations = (await _specializationService.GetAllAsync(1, 100)).Items;
            ViewBag.Roles = (await _roleService.GetAllAsync(1, 100)).Items;
            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateCollaboratorDto model)
        {
            if (id != model.Id) return BadRequest();

            if (ModelState.IsValid)
            {
                var error = await _collaboratorService.UpdateAsync(id, model);
                if (error == null)
                    return RedirectToAction(nameof(Index));
                
                ModelState.AddModelError(string.Empty, error);
            }
            ViewBag.Specializations = (await _specializationService.GetAllAsync(1, 100)).Items;
            ViewBag.Roles = (await _roleService.GetAllAsync(1, 100)).Items;
            ViewBag.Establishments = (await _establishmentService.GetAllAsync(1, 1000)).Items;
            return View(model);
        }
    }
}