using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

[DataContract]
public class ProviderData
{
    [DataMember(Name = "providerName")]
    public string ProviderName { get; set; }
    
    [DataMember(Name = "providerPhone")]
    public List<string> ProviderPhone { get; set; }
}