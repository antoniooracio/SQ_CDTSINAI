using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SQ.CDT_SINAI.API.Services
{
    public class DocumentRenewalWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DocumentRenewalWorker> _logger;

        public DocumentRenewalWorker(IServiceProvider serviceProvider, ILogger<DocumentRenewalWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // 1. Executa imediatamente ao iniciar o serviço (Docker start)
            try
            {
                _logger.LogInformation("Iniciando verificação de renovação automática (Startup)...");
                await CheckAndRenewDocumentsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na execução inicial da renovação automática.");
            }

            // 2. Calcula o tempo até a próxima execução (02:00 da manhã)
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(2);
            if (now >= nextRun) nextRun = nextRun.AddDays(1);
            
            var delay = nextRun - now;
            _logger.LogInformation($"Próxima execução agendada para {nextRun} (aguardando {delay.TotalHours:F2}h).");

            await Task.Delay(delay, stoppingToken);

            // 3. Loop diário (a cada 24h a partir das 02:00)
            using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Executando verificação de renovação automática (Agendada)...");
                    await CheckAndRenewDocumentsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao executar renovação automática de documentos.");
                }
            }
        }

        private async Task CheckAndRenewDocumentsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateTime.Today;

            // Busca documentos ativos, com renovação automática e vencidos (ou vencendo hoje)
            var documentsToRenew = await context.EstablishmentDocuments
                .Where(d => d.Status == DocumentStatus.Active 
                            && d.AutomaticRenewal 
                            && d.ExpirationDate.HasValue 
                            && d.ExpirationDate.Value <= today)
                .ToListAsync(stoppingToken);

            foreach (var doc in documentsToRenew)
            {
                if (doc.RenewalMonths > 0)
                {
                    var oldDate = doc.ExpirationDate!.Value;
                    var newDate = oldDate.AddMonths(doc.RenewalMonths);

                    doc.ExpirationDate = newDate;
                    doc.LastUpdated = DateTime.Now;

                    context.DocumentRenewalLogs.Add(new DocumentRenewalLog
                    {
                        EstablishmentDocumentId = doc.Id,
                        RenewalDate = DateTime.Now,
                        OldExpirationDate = oldDate,
                        NewExpirationDate = newDate,
                        Details = $"Renovação automática de {doc.RenewalMonths} meses executada pelo sistema."
                    });

                    _logger.LogInformation($"Documento {doc.Id} renovado automaticamente.");
                }
            }

            if (documentsToRenew.Any()) await context.SaveChangesAsync(stoppingToken);
        }
    }
}