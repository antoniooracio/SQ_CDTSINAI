using Microsoft.EntityFrameworkCore;
using SQ.CDT_SINAI.API.Data;
using SQ.CDT_SINAI.Shared.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SQ.CDT_SINAI.API.Workers
{
    public class ContractRenewalWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ContractRenewalWorker> _logger;

        public ContractRenewalWorker(IServiceProvider serviceProvider, ILogger<ContractRenewalWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Contract Renewal Worker iniciado.");

            // Executa imediatamente ao iniciar
            await ProcessRenewalsAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(2); // Próximo dia às 02:00
                var delay = nextRun - now;

                _logger.LogInformation($"Próxima verificação de renovação de contratos agendada para: {nextRun}");

                await Task.Delay(delay, stoppingToken);

                await ProcessRenewalsAsync(stoppingToken);
            }
        }

        private async Task ProcessRenewalsAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var today = DateTime.Today;

                try
                {
                    var contractsToRenew = await context.Contracts
                        .Where(c => c.Status == ContractStatus.Active && c.AutomaticRenewal && c.EndDate <= today)
                        .ToListAsync(stoppingToken);

                    foreach (var contract in contractsToRenew)
                    {
                        var newEndDate = contract.EndDate.AddMonths(contract.RenewalMonths);

                        var amendment = new ContractAmendment
                        {
                            ContractId = contract.Id,
                            AmendmentNumber = $"Renovação Automática - {today:dd/MM/yyyy}",
                            Type = AmendmentType.TermExtension,
                            SignatureDate = today,
                            NewEndDate = newEndDate,
                            Description = $"Renovação automática de {contract.RenewalMonths} meses processada pelo sistema."
                        };

                        contract.EndDate = newEndDate;
                        context.ContractAmendments.Add(amendment);
                        _logger.LogInformation($"Contrato {contract.ContractNumber} renovado até {newEndDate:dd/MM/yyyy}.");
                    }

                    if (contractsToRenew.Any())
                    {
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation($"{contractsToRenew.Count} contratos renovados com sucesso.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar renovações automáticas de contratos.");
                }
            }
        }
    }
}