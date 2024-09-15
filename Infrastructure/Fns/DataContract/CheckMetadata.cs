using System.Runtime.Serialization;

namespace Infrastructure.Fns.DataContract;

[DataContract]
public class CheckMetadata
{
    [DataMember(Name = "id")]
    public long Id { get; set; }
    
    [DataMember(Name = "ofdid")]
    public string OFDId { get; set; }
    
    [DataMember(Name = "address")]
    public string Address { get; set; }
    
    [DataMember(Name = "subtype")]
    public string Subtype { get; set; }
    
    [DataMember(Name = "receiveDate")]
    public string ReceiveData { get; set; }
}