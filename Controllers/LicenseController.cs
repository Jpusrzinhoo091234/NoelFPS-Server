using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NoelFPS.Server.Data;
using NoelFPS.Server.Models;
using NoelFPS.Server.DTOs;
using System.Security.Cryptography;

namespace NoelFPS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LicenseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LicenseController> _logger;
        
        // Chave de segurança para o Painel Admin
        private string AdminApiKey => Environment.GetEnvironmentVariable("ADMIN_API_KEY") ?? "NoelFPS-Admin-Secret-Key-2026";

        public LicenseController(AppDbContext context, ILogger<LicenseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("validate")]
        public async Task<ActionResult<ValidateKeyResponse>> Validate([FromBody] ValidateKeyRequest request)
        {
            _logger.LogInformation($"Tentativa de validação: Key={request.Key}, HWID={request.HardwareId}");
            
            var accessKey = await _context.AccessKeys
                .FirstOrDefaultAsync(k => k.Key == request.Key);

            if (accessKey == null)
            {
                _logger.LogWarning($"Chave não encontrada: {request.Key}");
                return Ok(new ValidateKeyResponse { IsValid = false, Message = "Chave inválida." });
            }

            if (!accessKey.IsActive)
            {
                _logger.LogWarning($"Chave inativa: {request.Key}");
                return Ok(new ValidateKeyResponse { IsValid = false, Message = "Chave desativada." });
            }

            if (accessKey.ExpiresAt < DateTime.UtcNow)
            {
                _logger.LogInformation($"Chave expirada automaticamente: {request.Key}");
                accessKey.IsActive = false;
                await _context.SaveChangesAsync();
                return Ok(new ValidateKeyResponse { IsValid = false, Message = "Chave expirada." });
            }

            if (string.IsNullOrEmpty(accessKey.HardwareId))
            {
                _logger.LogInformation($"Primeiro vínculo de HWID para chave {request.Key}: {request.HardwareId}");
                accessKey.HardwareId = request.HardwareId;
                await _context.SaveChangesAsync();
            }
            else if (accessKey.HardwareId != request.HardwareId)
            {
                _logger.LogCritical($"TENTATIVA DE USO EM DISPOSITIVO DIFERENTE! Key={request.Key}, HWID_Original={accessKey.HardwareId}, HWID_Tentativa={request.HardwareId}");
                return Ok(new ValidateKeyResponse { IsValid = false, Message = "Chave já vinculada a outro dispositivo." });
            }

            accessKey.LastUsedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new ValidateKeyResponse 
            { 
                IsValid = true, 
                Message = "Acesso autorizado.",
                ExpiresAt = accessKey.ExpiresAt
            });
        }

        [HttpGet("list")]
        public async Task<ActionResult<IEnumerable<AccessKey>>> List()
        {
            var adminKey = Request.Headers["X-Admin-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(adminKey) || adminKey.Trim() != AdminApiKey.Trim()) 
            {
                _logger.LogWarning("Tentativa de acesso não autorizado ao Painel Admin.");
                return Unauthorized();
            }
            return await _context.AccessKeys.OrderByDescending(k => k.CreatedAt).ToListAsync();
        }

        [HttpPost("generate")]
        public async Task<ActionResult<AccessKey>> Generate([FromBody] GenerateKeyRequest request)
        {
            var adminKey = Request.Headers["X-Admin-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(adminKey) || adminKey.Trim() != AdminApiKey.Trim()) return Unauthorized();

            var newKey = new AccessKey
            {
                Key = GenerateRandomKey(),
                ExpiresAt = DateTime.UtcNow.AddDays(request.ValidityDays),
                AssociatedUser = request.AssociatedUser,
                IsActive = true
            };

            _context.AccessKeys.Add(newKey);
            await _context.SaveChangesAsync();

            return Ok(newKey);
        }

        [HttpPost("revoke/{id}")]
        public async Task<IActionResult> Revoke(int id)
        {
            var adminKey = Request.Headers["X-Admin-Key"].FirstOrDefault();
            if (string.IsNullOrEmpty(adminKey) || adminKey.Trim() != AdminApiKey.Trim()) return Unauthorized();

            var key = await _context.AccessKeys.FindAsync(id);
            if (key == null) return NotFound();

            key.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private string GenerateRandomKey()
        {
            var bytes = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return $"NOEL-{Convert.ToBase64String(bytes).Replace("/", "").Replace("+", "").Substring(0, 16).ToUpper()}";
        }
    }
}
