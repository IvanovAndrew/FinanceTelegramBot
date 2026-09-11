using System.Runtime.Serialization;

namespace Application.Contracts.FNS;

[DataContract]
public class FnsRequest
{
    [DataMember(Name = "qrurl")]
    public string QrUrl { get; set; }
    
    [DataMember(Name = "qrfile")]
    public string QrFile { get; set; }
    
    [DataMember(Name = "qrraw")]
    public string QrRaw { get; set; }
    
    [DataMember(Name = "manual")]
    public FnsManualRequest Manual { get; set; }
}