using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ.Enums
{
    [DataContract]
    public enum Gender
    {
        [EnumMember]
        Male,
        [EnumMember]
        Female
    }
}
