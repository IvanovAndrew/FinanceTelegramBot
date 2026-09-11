using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class SubCategoryDTO
{
    [DataMember]
    public string Code {get;set;}
    
    [DataMember]
    public string Name {get;set;}
}