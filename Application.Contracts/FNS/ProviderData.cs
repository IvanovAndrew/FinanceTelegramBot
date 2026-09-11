using System.Runtime.Serialization;

namespace Application.Contracts.FNS;

[DataContract]
public class ProviderData
{
    [DataMember(Name = "providerName")]
    public string ProviderName { get; set; }
    
    [DataMember(Name = "providerPhone")]
    public List<string> ProviderPhone { get; set; }
}