using System.Runtime.Serialization;

namespace Application.Contracts.FNS;

[DataContract]
public class FnsResponse
{
    [DataMember(Name = "code")]
    public int Code { get; set; }
    
    [DataMember(Name = "first")]
    public int First { get; set; }
    
    [DataMember(Name = "data")]
    public FnsData Data { get; set; }
}