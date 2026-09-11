using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class CurrencyDTO
{
    [DataMember]
    public string Name { get; set; }
    
    [DataMember]
    public string Symbol { get; set; }

    [DataMember]
    public bool IsPopular { get; set; }

    [DataMember]
    public string Format { get; set; }
}