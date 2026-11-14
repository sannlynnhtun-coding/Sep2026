namespace Sep2026.HnitThitMayKyaw.SupermarketPos.WebApi.Models;

public class CreateCategoryRequestModel
{
    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }
}

public class UpsertCategoryRequestModel
{
    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }
}

public class UpdateCategoryRequestModel
{
    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }
}