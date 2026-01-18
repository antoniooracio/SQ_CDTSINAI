using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.Shared.Models;

namespace SQ.CDT_SINAI.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Collaborator> Collaborators { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractAmendment> ContractAmendments { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<EstablishmentType> EstablishmentTypes { get; set; }
        public DbSet<Establishment> Establishments { get; set; }
        public DbSet<DocumentType> DocumentTypes { get; set; }
        public DbSet<EstablishmentDocument> EstablishmentDocuments { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<DocumentRenewalLog> DocumentRenewalLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configuração explícita para evitar ambiguidade entre as duas listas de documentos em EstablishmentType
            modelBuilder.Entity<EstablishmentType>()
                .HasMany(e => e.NecessaryDocuments)
                .WithMany()
                .UsingEntity(j => j.ToTable("EstablishmentTypeNecessaryDocuments"));

            modelBuilder.Entity<EstablishmentType>()
                .HasMany(e => e.ClosingDocuments)
                .WithMany()
                .UsingEntity(j => j.ToTable("EstablishmentTypeClosingDocuments"));
            
            // Seed de Perfis Iniciais
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Administrador" }, // Acesso Total
                new Role { Id = 2, Name = "Coordenador" },   // Acesso Regional/Geral
                new Role { Id = 3, Name = "Colaborador" }    // Acesso Restrito
            );

            // Seed de Permissões Iniciais
            modelBuilder.Entity<RolePermission>().HasData(
                // Coordenador (Exemplo)
                new RolePermission { Id = 1, RoleId = 2, Module = "Collaborator", Action = "View" },
                new RolePermission { Id = 2, RoleId = 2, Module = "Collaborator", Action = "Create" },
                new RolePermission { Id = 3, RoleId = 2, Module = "Collaborator", Action = "Edit" },
                new RolePermission { Id = 4, RoleId = 2, Module = "Establishment", Action = "View" },

                // Permissões de Contratos (Novos Módulos)
                new RolePermission { Id = 5, RoleId = 2, Module = "Contract", Action = "View" },
                new RolePermission { Id = 6, RoleId = 2, Module = "Contract", Action = "Create" },
                new RolePermission { Id = 7, RoleId = 2, Module = "Contract", Action = "Edit" },
                new RolePermission { Id = 8, RoleId = 2, Module = "ContractRenewalLog", Action = "View" },
                new RolePermission { Id = 9, RoleId = 2, Module = "ContractRenewalLog", Action = "Revert" },
                
                // Colaborador (Exemplo)
                new RolePermission { Id = 10, RoleId = 3, Module = "Collaborator", Action = "View" }
            );

            // Seed de Usuário Administrador Inicial
            modelBuilder.Entity<Collaborator>().HasData(new Collaborator
            {
                Id = 1,
                Name = "Administrador do Sistema",
                Email = "admin@sinai.com.br",
                Password = "admin", // Senha padrão para desenvolvimento
                Cpf = "000.000.000-00",
                RoleId = 1, // Administrador
                Active = true,
                BirthDate = new DateTime(1990, 1, 1),
                AdmissionDate = new DateTime(2023, 1, 1),
                PhoneNumber = "00000000000",
                AddressStreet = "Sede",
                AddressNumber = "1",
                AddressNeighborhood = "Centro",
                AddressCity = "Palmas",
                AddressState = "TO",
                AddressZipCode = "77000-000",
                JobTitle = "Super Admin",
                ProfessionalLicense = "N/A"
            });
        }
    }
}