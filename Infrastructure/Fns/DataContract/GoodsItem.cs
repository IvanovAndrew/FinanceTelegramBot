using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

[DataContract]
public class GoodsItem
{
    [DataMember(Name = "nds")]
    public int Nds { get; set; }
    
    [DataMember(Name = "sum")]
    public decimal Sum { get; set; }

    [DataMember(Name = "name")]
    public string Name { get; set; }
    
    [DataMember(Name = "price")]
    public decimal Price { get; set; }
    
    [DataMember(Name = "quantity")]
    public decimal Quantity { get; set; }
    
    [DataMember(Name = "paymentType")]
    public int PaymentType { get; set; }

    [DataMember(Name = "productType")]
    public int ProductType { get; set; }
    
    [DataMember(Name = "providerInn")]
    public string ProviderInn { get; set; }
    
    [DataMember(Name = "providerData")]
    public ProviderData ProviderData { get; set; }
 
    [DataMember(Name = "itemsQuantityMeasure")]
    public int ItemsQuantityMeasure { get; set; }
    
    [DataMember(Name = "paymentAgentByProductType")]
    public int PaymentAgentByProductType { get; set; }
}