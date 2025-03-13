using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ.Enums
{
    [DataContract]
    public enum SigningType
    {
        [EnumMember]
        Signed = 1,
        [EnumMember]
        Unsigned
    }
}
