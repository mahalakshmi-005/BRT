using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BRT.Models
{
    public enum OrderStatus
    {
        Pending = 1,
        UnderReview = 2,
        PriceUpdated = 3,
        Confirmed = 4,
        Rejected = 5,
        Completed = 6,
        Cancelled = 7
    }

    public class OrderRequest
    {
        public int Id { get; set; }

        [Required, MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty; // e.g. BRT-20260716-0001

        // Buyer contact captured directly on the order — no account/login needed
        [Required, MaxLength(150)]
        public string BuyerName { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string BusinessName { get; set; } = string.Empty;

        [Required, MaxLength(15)]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? BuyerAddress { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalEstimatedAmount { get; set; }

        public bool TransportChargeApplicable { get; set; } = true;
        public string? AdminRemarks { get; set; }
        public DateTime? ExpectedDispatchDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderRequestItem> Items { get; set; } = new List<OrderRequestItem>();
        public ICollection<OrderStatusHistory> StatusHistory { get; set; } = new List<OrderStatusHistory>();
    }

    public class OrderRequestItem
    {
        public int Id { get; set; }

        public int OrderRequestId { get; set; }
        [ForeignKey(nameof(OrderRequestId))]
        public OrderRequest? OrderRequest { get; set; }

        public int ProductId { get; set; }
        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        public int? PackingTypeId { get; set; }
        [ForeignKey(nameof(PackingTypeId))]
        public PackingType? PackingType { get; set; }

        public decimal Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceAtOrderTime { get; set; }

        [NotMapped]
        public decimal LineEstimate => Quantity * PriceAtOrderTime;
    }

    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderRequestId { get; set; }
        [ForeignKey(nameof(OrderRequestId))]
        public OrderRequest? OrderRequest { get; set; }

        public OrderStatus OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        [MaxLength(100)]
        public string? ChangedBy { get; set; }
        public string? Remarks { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
