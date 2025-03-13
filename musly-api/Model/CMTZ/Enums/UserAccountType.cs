using System;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ.Enums
{
    [DataContract]
    public enum UserAccountType
    {
        [EnumMember]
        Normal,
        [EnumMember]
        Premium
    }
}
