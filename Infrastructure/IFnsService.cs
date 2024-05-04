using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Infrastructure;

public interface IFnsService
{
    public Task<FnsResponse?> GetCheck(string qrRaw);
}

public class FnsService : IFnsService
{
    private const string Url = "https://proverkacheka.com/api/v1/check/get";
    private readonly string _token;

    public FnsService(string token)
    {
        _token = token;
    }

    public async Task<FnsResponse?> GetCheck(string qrRaw)
    {
        using HttpClient client = new HttpClient();
        
        HttpContent content = new FormUrlEncodedContent(
            new Dictionary<string, string>()
            {
                ["qrraw"] = qrRaw,
                ["token"] = _token
            });

        var response = await client.PostAsync(Url, content);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FnsResponse>(responseString);
        }

        return null;
    }
}

[DataContract]
public class FnsResponse
{
    [DataMember(Name = "code")]
    public int Code { get; set; }
    
    [DataMember(Name = "first")]
    public int First { get; set; }
    
    [DataMember(Name = "data")]
    public FnsData Data { get; set; }
}

[DataContract]
public class FnsData
{
    [DataMember(Name = "json")]
    public FnsCheckInfo Json { get; set; }
    
    [DataMember(Name = "html")]
    public string Html { get; set; }
}

[DataContract]
public class FnsCheckInfo
{
    [DataMember(Name = "code")]
    public int Code { get; set; }
    
    [DataMember(Name = "user")]
    public string User { get; set; }
    
    [DataMember(Name = "items")]
    public List<GoodsItem> Items { get; set; }
    
    [DataMember(Name = "region")]
    public string Region { get; set; }
    
    [DataMember(Name = "userInn")]
    public string UserInn { get; set; }
    
    [DataMember(Name = "dateTime")]
    public DateTime DateTime { get; set; }
    
    [DataMember(Name = "kktRegId")]
    public string KktRegId { get; set; }

    [DataMember(Name = "metadata")]
    public CheckMetadata Metadata { get; set; }
    
    [DataMember(Name = "nds18118")]
    public int Nds18118 { get; set; }
    
    [DataMember(Name = "totalSum")]
    public decimal TotalSum { get; set; }
    
    [DataMember(Name = "creditSum")]
    public decimal CreditSum { get; set; }
    
    [DataMember(Name = "numberKkt")]
    public string NumberKkt { get; set; }
    
    [DataMember(Name = "fiscalSign")]
    public decimal FiscalSign { get; set; }
    
    [DataMember(Name = "prepaidSum")]
    public decimal PrepaidSum { get; set; }
    
    [DataMember(Name = "retailPlace")]
    public string RetailPlace { get; set; }
    
    [DataMember(Name = "shiftNumber")]
    public int ShiftNumber { get; set; }
    
    [DataMember(Name = "cashTotalSum")]
    public decimal CashTotalSum { get; set; }
    
    [DataMember(Name = "internetSign")]
    public int InternetSign { get; set; }
    
    [DataMember(Name = "provisionSum")]
    public decimal ProvisionSum { get; set; }
    
    [DataMember(Name = "ecashTotalSum")]
    public decimal ECashTotalSum { get; set; }
    
    [DataMember(Name = "machineNumber")]
    public string MachineNumber { get; set; }
    
    [DataMember(Name = "operationType")]
    public int OperationType { get; set; }
    
    [DataMember(Name = "redefine_mask")]
    public int RedefineMask { get; set; }
    
    [DataMember(Name = "requestNumber")]
    public int RequestNumber { get; set; }
    
    [DataMember(Name = "sellerAddress")]
    public string SellerAddress { get; set; }
    
    [DataMember(Name = "fiscalDriveNumber")]
    public string FiscalDriveNumber { get; set; }
    
    [DataMember(Name = "messageFiscalSign")]
    public decimal MessageFiscalSign { get; set; }
    
    [DataMember(Name = "retailPlaceAddress")]
    public string RetailPlaceAddress { get; set; }
    
    [DataMember(Name = "appliedTaxationType")]
    public int AppliedTaxationType { get; set; }
    
    [DataMember(Name = "buyerPhoneOrAddress")]
    public string BuyerPhoneOrAddress { get; set; }
    
    [DataMember(Name = "fiscalDocumentNumber")]
    public int FiscalDocumentNumber { get; set; }
    
    [DataMember(Name = "fiscalDocumentFormatVer")]
    public int FiscalDocumentFormatVersion { get; set; }
}

[DataContract]
public class CheckMetadata
{
    [DataMember(Name = "id")]
    public long Id { get; set; }
    
    [DataMember(Name = "ofdid")]
    public string OFDId { get; set; }
    
    [DataMember(Name = "address")]
    public string Address { get; set; }
    
    [DataMember(Name = "subtype")]
    public string Subtype { get; set; }
    
    [DataMember(Name = "receiveDate")]
    public string ReceiveData { get; set; }
}

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

[DataContract]
public class ProviderData
{
    [DataMember(Name = "providerName")]
    public string ProviderName { get; set; }
    
    [DataMember(Name = "providerPhone")]
    public List<string> ProviderPhone { get; set; }
}

[DataContract]
public class FnsRequest
{
    [DataMember(Name = "qrurl")]
    public string QrUrl { get; set; }
    
    [DataMember(Name = "qrfile")]
    public string QrFile { get; set; }
    
    [DataMember(Name = "qrraw")]
    public string QrRaw { get; set; }
    
    [DataMember(Name = "manual")]
    public FnsManualRequest Manual { get; set; }
}

[DataContract]
public class FnsManualRequest
{
    [DataMember(Name = "fn")]
    public string Fn { get; set; }
    
    [DataMember(Name = "fd")]
    public string Fd { get; set; }
    
    [DataMember(Name = "fp")]
    public string Fp { get; set; }
    
    [DataMember(Name = "check_time")]
    public string CheckTime { get; set; }
    
    [DataMember(Name = "type")]
    public string Type { get; set; }
    
    [DataMember(Name = "sum")]
    public string Sum { get; set; }
} 