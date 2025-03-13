using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class Country : PersistentEntity
    {
        [DataMember]
        public string Title { get; set; }
    }
}
