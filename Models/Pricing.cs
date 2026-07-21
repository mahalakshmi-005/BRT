using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BRT.Models
{
    public enum TrendDirection
    {
        Up = 1,
        Down = 2,
        Stable = 3,
        NewStock = 4
    }

    public class MarketPrice
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public int? PackingTypeId { get; set; }
        [ForeignKey(nameof(PackingTypeId))]
        public PackingType? PackingType { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TodayPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PreviousPrice { get; set; }

        [NotMapped]
        public decimal PriceDifference => TodayPrice - PreviousPrice;

        public bool GSTIncluded { get; set; } = true;

        public DateTime PriceDate { get; set; } = DateTime.UtcNow.Date;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedBy { get; set; }
    }

    public class MarketHighlight
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Required, MaxLength(150)]
        public string HighlightText { get; set; } = string.Empty; // "MP Garlic ↑"

        public TrendDirection TrendDirection { get; set; }
        public DateTime DisplayDate { get; set; } = DateTime.UtcNow.Date;
        public bool IsActive { get; set; } = true;
    }
}
