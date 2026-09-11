using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class FnsURLDTO
{
    [DataMember]
    public string Url { get; set; }
}