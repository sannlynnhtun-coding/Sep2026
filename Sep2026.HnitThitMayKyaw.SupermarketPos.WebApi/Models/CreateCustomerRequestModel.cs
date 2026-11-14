namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateCustomerRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}

public class UpsertCustomerRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}

public class UpdateCustomerRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }
}