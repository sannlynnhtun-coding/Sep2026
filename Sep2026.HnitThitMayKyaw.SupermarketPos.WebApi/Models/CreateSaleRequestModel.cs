namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateSaleRequestModel
{
    public string SaleNumber { get; set; } = null!;

    public int? CustomerId { get; set; }

    public int EmployeeId { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal TotalAmount { get; set; }
}

