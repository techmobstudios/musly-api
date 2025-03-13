using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using musly_api.Model.CMTZ.Enums;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class Track : PersistentEntity
    {
        [DataMember]
        public string TrackTitle { get; set; }

        [NotMapped]
        [DataMember]
        public string TrackURL { get; set; }

        [DataMember]
        public double Size { get; set; }
        [DataMember]
        public string Extension { get; set; }
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public long? Upvotes { get; set; }
        [DataMember]
        public long? Downvotes { get; set; }
        [DataMember]
        public long? TotalVotes { get; set; }

        [DataMember]
        public long? TotalPlays { get; set; }
        [DataMember]
        public long? TotalViews { get; set; }
        [DataMember]
        public long? TotalDownloads { get; set; }

        [DataMember]
        public long? AlbumId { get; set; }
        [ForeignKey("AlbumId")]
        [DataMember]
        public virtual Album Album { get; set; }

        // New Fields for Single specific 

        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        public long? DjId { get; set; }
        [ForeignKey("DjId")]
        [DataMember]
        public virtual DJ DjUser { get; set; }

        [DataMember]
        public string Artists { get; set; }
        [DataMember]
        public string KeyWords { get; set; }
        [DataMember]
        public int? GenreTypeCode { get; set; }
        [DataMember]
        [NotMapped]
        public GenreType? GenreType
        {
            get { return (GenreType?)GenreTypeCode; }
            set { GenreTypeCode = (int?)value; }
        }

        [DataMember]
        public string CoverImageName { get; set; }
        [DataMember]
        public string TrackFileName { get; set; }
        [DataMember]
        public int? SingleStatusCode { get; set; }
        [DataMember]
        [NotMapped]
        public AlbumStatus? SingleStatus
        {
            get { return (AlbumStatus?)SingleStatusCode; }
            set { SingleStatusCode = (int?)value; }
        }

        [DataMember]
        public long? UploadedBy { get; set; }
        [ForeignKey("UploadedBy")]
        [DataMember]
        public virtual User UploadedByUser { get; set; }

        [DataMember]
        public string AffiliateLink { get; set; }
        [DataMember]
        public string TwitterLink { get; set; }
        [DataMember]
        public string InstagramLink { get; set; }
        [DataMember]
        public string RecordLabel { get; set; }

        [DataMember]
        public bool? IsDownloadable { get; set; }
    }
}