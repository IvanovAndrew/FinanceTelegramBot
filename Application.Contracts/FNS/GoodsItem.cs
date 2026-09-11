namespace Application.Contracts.FNS;

public class GoodsItem
{
    public int Nds { get; set; }
    
    public decimal Sum { get; set; }

    public string Name { get; set; }
    
    public decimal Price { get; set; }
    
    public decimal Quantity { get; set; }
    
    public int PaymentType { get; set; }

    public int ProductType { get; set; }
    
    public ProductCode? ProductCodeNew { get; set; }
    
    public string ProviderInn { get; set; }
    
    public ProviderData ProviderData { get; set; }
 
    public int ItemsQuantityMeasure { get; set; }
    
    public int PaymentAgentByProductType { get; set; }
}

public class ProductCode
{
    public InternationalCode? Gs1m { get; set; }
}

public class InternationalCode
{
    public string Gtin { get; set; }

    public string Sernum { get; set; }

    public int ProductIdType { get; set; }

    public string RawProductCode { get; set; }
}