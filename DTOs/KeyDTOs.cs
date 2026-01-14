namespace NoelFPS.Server.DTOs
{
    public class ValidateKeyRequest
    {
        public string Key { get; set; } = string.Empty;
        public string HardwareId { get; set; } = string.Empty;
    }

    public class ValidateKeyResponse
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }

    public class GenerateKeyRequest
    {
        public int ValidityDays { get; set; } = 30;
        public string? AssociatedUser { get; set; }
    }
}
