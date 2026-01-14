using System;
using System.ComponentModel.DataAnnotations;

namespace NoelFPS.Server.Models
{
    public class AccessKey
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Key { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? AssociatedUser { get; set; }
        
        public string? HardwareId { get; set; }
        
        public DateTime? LastUsedAt { get; set; }
    }
}
