using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NoelFPS.Server.Services
{
    public class BackupService : BackgroundService
    {
        private readonly ILogger<BackupService> _logger;
        private readonly string _dbPath = "license.db";
        private readonly string _backupFolder = "Backups";

        public BackupService(ILogger<BackupService> logger)
        {
            _logger = logger;
            if (!Directory.Exists(_backupFolder)) Directory.CreateDirectory(_backupFolder);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (File.Exists(_dbPath))
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string backupPath = Path.Combine(_backupFolder, $"license_backup_{timestamp}.db");
                        File.Copy(_dbPath, backupPath, true);
                        _logger.LogInformation($"Backup realizado com sucesso: {backupPath}");
                        
                        // Limpar backups antigos (manter apenas os últimos 10)
                        var files = new DirectoryInfo(_backupFolder).GetFiles("*.db");
                        if (files.Length > 10)
                        {
                            foreach (var file in files.OrderBy(f => f.CreationTime).Take(files.Length - 10))
                            {
                                file.Delete();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao realizar backup: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken); // Backup a cada 24 horas
            }
        }
    }
}
