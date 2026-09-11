using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class FnsRequisiteDTO
{
    [DataMember]
    public DateTime DateTime { get; set; }
    [DataMember]
    public decimal TotalPrice { get; set; }
    [DataMember]
    public string FiscalNumber { get; set; }
    [DataMember]
    public string FiscalDocumentNumber { get; set; }
    [DataMember]
    public string FiscalDocumentSign { get; set; }
}