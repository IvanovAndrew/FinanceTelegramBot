using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class SaveCheckDto
{
    [DataMember]
    public bool Success { get; set; }
    
    [DataMember]
    public string? Error { get; set; }
    
    [DataMember]
    public ShopExpensesDto? ShopExpenses { get; set; }
}