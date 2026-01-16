using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SQ.CDT_SINAI.API.Data;

namespace SQ.CDT_SINAI.API.Extensions
{
    public static class DatabaseManagementService
    {
        public static void MigrationInitialisation(this IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var serviceDb = serviceScope.ServiceProvider.GetService<AppDbContext>();
                serviceDb.Database.Migrate();
            }
        }
    }
}