using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

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