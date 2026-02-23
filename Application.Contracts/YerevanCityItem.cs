namespace Application.Contracts;

using System.Text.Json.Serialization;

public sealed class YerevanCityOrderResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("data")]
    public OrderData Data { get; set; } = default!;

    [JsonPropertyName("messages")]
    public List<ApiMessage> Messages { get; set; } = [];

    [JsonPropertyName("alertIcon")]
    public string? AlertIcon { get; set; }
}

public sealed class OrderData
{
    [JsonPropertyName("orderid")]
    public string OrderId { get; set; } = default!;
    [JsonPropertyName("createdate")]
    public DateTime CreateDate { get; set; }
    public int PaymentMethod { get; set; }
    public decimal UsedBonusAmount { get; set; }
    public decimal DeliveryPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public int Status { get; set; }
    public decimal TotalToPay { get; set; }
    public string ShopUserId { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string SurName { get; set; } = default!;
    public string? Photo { get; set; }
    public string PhoneNumber { get; set; } = default!;
    [JsonPropertyName("orderitems")]
    public List<OrderItem> OrderItems { get; set; } = [];
    public decimal TotalBonus { get; set; }
    public BranchAddress BranchAddress { get; set; } = default!;
}

public sealed class OrderItem
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal InitialPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string? Photo { get; set; }
    public decimal Weight { get; set; }
    public bool IsKilogram { get; set; }
    public decimal? MinimumWeight { get; set; }
    public decimal? WeightStep { get; set; }
    public decimal VisiblePrice { get; set; }
    public decimal Bonus { get; set; }
    public bool Cut { get; set; }
    public bool Grind { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsBag { get; set; }
    public decimal InitialWeight { get; set; }
    public int InitialQuantity { get; set; }
    public int InitialCount { get; set; }
    public bool IsOnline { get; set; }
    public string CategoryName { get; set; } = default!;
    public int CodeSap { get; set; }
    public object? Attributes { get; set; }
    public object? ProductTag { get; set; }
    public decimal? ProductPricePerUnit { get; set; }
    public decimal? WeightProductPricePerUnit { get; set; }
    public string? WeightMeasure { get; set; }
}

public sealed class BranchAddress
{
    public string Address { get; set; } = default!;
    public string Id { get; set; } = default!;
}

public sealed class ApiMessage
{
    public int Key { get; set; }
    public string Value { get; set; } = default!;
}