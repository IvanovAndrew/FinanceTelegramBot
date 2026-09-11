using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class MoneyTransferDTO
{
    [DataMember]
    public bool IsOutcome { get; set; }
    
    [DataMember]
    public DateOnly Date { get; set; }
    
    [DataMember]
    public string Category { get; set; }
    
    [DataMember]
    public string? SubCategory { get; set; }
    
    [DataMember]
    public string? Shop { get; set; }
    
    [DataMember]
    public string? Description { get; set; }
    
    [DataMember]
    public decimal Amount { get; set; }
    
    [DataMember]
    public string Currency { get; set; }
}