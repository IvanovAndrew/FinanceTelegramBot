using System.Runtime.Serialization;

namespace TelegramBot.Contracts;

[DataContract]
public class CategoryDTO
{
    [DataMember]
    public string Code {get; set;}
    
    [DataMember]
    public string Name {get;set;}
    
    [DataMember]
    public List<SubCategoryDTO> SubCategories {get;set;}
    
    [DataMember]
    public bool IsPopular { get; set; }
}