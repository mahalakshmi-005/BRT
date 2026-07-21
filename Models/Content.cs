using System.ComponentModel.DataAnnotations;

namespace BRT.Models
{
    public class Banner
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(250)]
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        [MaxLength(50)]
        public string? ButtonText { get; set; }
        [MaxLength(200)]
        public string? ButtonLink { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class Testimonial
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string DealerName { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? BusinessName { get; set; }
        [MaxLength(100)]
        public string? City { get; set; }
        [Required]
        public string Message { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; } = 5;
        public string? ImageUrl { get; set; }
        public bool IsApproved { get; set; } = false;
        public int DisplayOrder { get; set; }
    }

    public class FAQ
    {
        public int Id { get; set; }
        [Required]
        public string Question { get; set; } = string.Empty;
        [Required]
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class GalleryImage
    {
        public int Id { get; set; }
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? Caption { get; set; }
        [MaxLength(50)]
        public string? Category { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ContactMessage
    {
        public int Id { get; set; }
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;
        [MaxLength(150)]
        public string? Email { get; set; }
        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
    }

    public class SiteSetting
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Key { get; set; } = string.Empty;
        [Required]
        public string Value { get; set; } = string.Empty;
    }
}
