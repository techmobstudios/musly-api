using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ.Enums
{
    [DataContract]
    public enum GenreType
    {
        [EnumMember]
        [Description("Rap")]
        Rap = 1,
        [EnumMember]
        [Description("R&B")]
        RnB,
        [EnumMember]
        [Description("EDM")]
        EDM,
        [EnumMember]
        [Description("Christian Hip Hop")]
        ChristianHipHop,
        [EnumMember]
        [Description("Hip Hop Blends")]
        HipHopBlends,
        [EnumMember]
        [Description("Soul")]
        Soul,
        [EnumMember]
        [Description("Instrumentals")]
        Instrumentals,
        [EnumMember]
        [Description("Chopped and Screwed")]
        ChoppedAndScrewed,
    }
}
