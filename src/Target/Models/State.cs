using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Target.Models
{
    [DataContract]
    public class State
    {
        [DataMember]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [DataMember]
        public List<Device> Devices { get; set; } = new List<Device>();
    }
}
