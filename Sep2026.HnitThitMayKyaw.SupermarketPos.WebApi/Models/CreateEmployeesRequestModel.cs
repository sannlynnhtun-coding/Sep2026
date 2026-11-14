namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateEmployeesRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }
}

public class UpsertEmployeesRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }
}

public class UpdateEmployeesRequestModel
{
    public string FirstName { get; set; } = null!;

    public string? LastName { get; set; }

    public string? Phone { get; set; }
}