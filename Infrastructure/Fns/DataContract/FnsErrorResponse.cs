using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

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