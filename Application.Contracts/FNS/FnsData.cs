using System.Runtime.Serialization;

namespace Application.Contracts.FNS;

[DataContract]
public class FnsData
{
    [DataMember(Name = "json")]
    public FnsCheckInfo Json { get; set; }
    
    [DataMember(Name = "html")]
    public string Html { get; set; }
}