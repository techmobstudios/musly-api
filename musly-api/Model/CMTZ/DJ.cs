using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class DJ : PersistentEntity
    {
        [DataMember]
        public string DJName { get; set; }
    }
}
