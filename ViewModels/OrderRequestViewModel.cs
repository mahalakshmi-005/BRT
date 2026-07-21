using System.ComponentModel.DataAnnotations;

namespace BRT.ViewModels
{
    public class OrderItemInput
    {
        public int? ProductId { get; set; }
        public int? PackingTypeId { get; set; }
        public decimal? Quantity { get; set; }
    }

    public class OrderRequestViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string BuyerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Business name is required")]
        public string BusinessName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number")]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? BuyerAddress { get; set; }
        public string? City { get; set; }

        public List<OrderItemInput> Items { get; set; } = new()
        {
            new(), new(), new(), new(), new()
        };
    }
}
