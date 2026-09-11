namespace Application.Contracts.FNS;

public class FnsCheckInfo
{
    public int Code { get; set; }
    
    public string User { get; set; }
    
    public List<GoodsItem> Items { get; set; }
    
    public string Region { get; set; }
    
    public string UserInn { get; set; }
    
    public DateTime DateTime { get; set; }
    
    public string KktRegId { get; set; }

    public CheckMetadata Metadata { get; set; }
    
    public int Nds18118 { get; set; }
    
    public decimal TotalSum { get; set; }
    
    public decimal CreditSum { get; set; }
    
    public string NumberKkt { get; set; }
    
    public decimal FiscalSign { get; set; }
    
    public decimal PrepaidSum { get; set; }
    
    public string RetailPlace { get; set; }
    
    public int ShiftNumber { get; set; }
    
    public decimal CashTotalSum { get; set; }
    
    public int InternetSign { get; set; }
    
    public decimal ProvisionSum { get; set; }
    
    public decimal ECashTotalSum { get; set; }
    
    public string MachineNumber { get; set; }
    
    public int OperationType { get; set; }
    
    public int RedefineMask { get; set; }
    
    public int RequestNumber { get; set; }
    
    public string SellerAddress { get; set; }
    
    public string FiscalDriveNumber { get; set; }
    
    public decimal MessageFiscalSign { get; set; }
    
    public string RetailPlaceAddress { get; set; }
    
    public int AppliedTaxationType { get; set; }
    
    public string BuyerPhoneOrAddress { get; set; }
    
    public int FiscalDocumentNumber { get; set; }
    
    public int FiscalDocumentFormatVersion { get; set; }
}