using System.Runtime.Serialization;

namespace Application.Contracts.FNS;

[DataContract]
public class FnsErrorResponse
{
    [DataMember(Name = "code")]
    public int Code { get; set; }
    
    [DataMember(Name = "first")]
    public int First { get; set; }
    
    [DataMember(Name = "data")]
    public string Data { get; set; }
}