using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

[DataContract]
public class FnsManualRequest
{
    [DataMember(Name = "fn")]
    public string Fn { get; set; }
    
    [DataMember(Name = "fd")]
    public string Fd { get; set; }
    
    [DataMember(Name = "fp")]
    public string Fp { get; set; }
    
    [DataMember(Name = "check_time")]
    public string CheckTime { get; set; }
    
    [DataMember(Name = "type")]
    public string Type { get; set; }
    
    [DataMember(Name = "sum")]
    public string Sum { get; set; }
}