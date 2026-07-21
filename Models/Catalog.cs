using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BRT.Models
{
    public enum CategoryType
    {
        Garlic = 1,
        Grocery = 2
    }

    public class Category
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameTamil { get; set; }

        [MaxLength(120)]
        public string Slug { get; set; } = string.Empty;

        public CategoryType Type { get; set; }
        public int DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<SubCategory> SubCategories { get; set; } = new List<SubCategory>();
    }

    public class SubCategory
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g. MP Garlic, Himachal Garlic, Kashmir Garlic

        [MaxLength(100)]
        public string? NameTamil { get; set; }

        [MaxLength(120)]
        public string Slug { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public class Product
    {
        public int Id { get; set; }

        public int SubCategoryId { get; set; }
        [ForeignKey(nameof(SubCategoryId))]
        public SubCategory? SubCategory { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? NameTamil { get; set; }

        [MaxLength(180)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Grade { get; set; } // Bomb, Laddu, Poona Laddu, AAA, AA, A, C, Bold, Medium

        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        [MaxLength(20)]
        public string? HSNCode { get; set; }

        public bool IsLooseAvailable { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PackingType> PackingTypes { get; set; } = new List<PackingType>();
        public ICollection<MarketPrice> MarketPrices { get; set; } = new List<MarketPrice>();
    }

    public class PackingType
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required, MaxLength(50)]
        public string PackSize { get; set; } = string.Empty; // "30 KG Bag", "50g", "1 Kg"

        [Required, MaxLength(10)]
        public string PackUnit { get; set; } = string.Empty; // KG, G

        public bool IsWholesale { get; set; } = true;
        public bool IsLoose { get; set; } = false;

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
