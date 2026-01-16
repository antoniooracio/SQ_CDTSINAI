using Microsoft.AspNetCore.Authentication.Cookies;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Adicionar Cache em Memória
builder.Services.AddMemoryCache();

// Necessário para acessar Cookies nos Services
builder.Services.AddHttpContextAccessor();

// Registrar o AuthService e configurar o HttpClient
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IAuthService, SQ.CDT_SINAI.Web.Services.AuthService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.ISpecializationService, SQ.CDT_SINAI.Web.Services.SpecializationService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.ICollaboratorService, SQ.CDT_SINAI.Web.Services.CollaboratorService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IIncidentService, SQ.CDT_SINAI.Web.Services.IncidentService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IBrandService, SQ.CDT_SINAI.Web.Services.BrandService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IEstablishmentTypeService, SQ.CDT_SINAI.Web.Services.EstablishmentTypeService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IEstablishmentService, SQ.CDT_SINAI.Web.Services.EstablishmentService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.ILegalizationService, SQ.CDT_SINAI.Web.Services.LegalizationService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IDocumentTypeService, SQ.CDT_SINAI.Web.Services.DocumentTypeService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IRoleService, SQ.CDT_SINAI.Web.Services.RoleService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IPermissionService, SQ.CDT_SINAI.Web.Services.PermissionService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IDocumentRenewalLogService, SQ.CDT_SINAI.Web.Services.DocumentRenewalLogService>();
builder.Services.AddHttpClient<SQ.CDT_SINAI.Web.Services.IReportService, SQ.CDT_SINAI.Web.Services.ReportService>();

// Configuração de Autenticação (Cookie) para o Site
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
