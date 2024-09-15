using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

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