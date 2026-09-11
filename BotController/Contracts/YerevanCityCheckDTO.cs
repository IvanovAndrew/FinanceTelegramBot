using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class YerevanCityCheckDTO
{
    [DataMember]
    public DateOnly Date { get; set; }

    [DataMember]
    public string Barcode { get; set; }
}