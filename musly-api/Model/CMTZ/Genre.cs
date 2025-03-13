using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class Genre : PersistentEntity
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}
