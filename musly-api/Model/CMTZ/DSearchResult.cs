using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class DSearchResult<T>
    {
        [DataMember]
        public IEnumerable<T> Items { get; set; }
        [DataMember]
        public bool ResultsTrimmed { get; set; }
        [DataMember]
        public int Itemscount { get; set; }
    }
}
