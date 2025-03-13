using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ.Enums
{
    [DataContract]
    public enum AlbumStatus
    {
        [EnumMember]
        [Description("All")]
        All = 1,
        [EnumMember]
        [Description("Approved")]
        Approved,
        [EnumMember]
        [Description("Not Approved")]
        Notapproved
    }
}
