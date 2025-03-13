using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using musly_api.Model.CMTZ.Enums;

namespace musly_api.Model.CMTZ
{

    [DataContract]
    [KnownType(typeof(DateTime))]
    public class Album : PersistentEntity
    {
        [Required(ErrorMessage = "An Album Title is required")]
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string FilePath { get; set; }
        [DataMember]
        [NotMapped]
        public string ThumbImg { get; set; }
        [DataMember]
        public long? DjId { get; set; }
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
        public int? SigningTypeCode { get; set; }

        [DataMember]
        [NotMapped]
        public SigningType? SigningType
        {
            get { return (SigningType?)SigningTypeCode; }
            set { SigningTypeCode = (int?)value; }
        }

        [DataMember]
        public int? AlbumTypeCode { get; set; }
        [DataMember]
        [NotMapped]
        public AlbumType? AlbumType
        {
            get { return (AlbumType?)AlbumTypeCode; }
            set { AlbumTypeCode = (int?)value; }
        }

        [DataMember]
        public int? AlbumStatusCode { get; set; }

        [DataMember]
        [NotMapped]
        public AlbumStatus? AlbumStatus
        {
            get { return (AlbumStatus?)AlbumStatusCode; }
            set { AlbumStatusCode = (int?)value; }
        }

        [DataMember]
        public bool AgreeTerms { get; set; }
        [DataMember]
        public bool IsDownloadable { get; set; }
        [DataMember]
        public bool AllowComments { get; set; }
        [DataMember]
        public bool MakeCommentsPrivate { get; set; }

        [DataMember]
        public long Upvotes { get; set; }
        [DataMember]
        public long Downvotes { get; set; }
        [DataMember]
        public long TotalVotes { get; set; }

        [DataMember]
        public long TotalViews { get; set; }
        [DataMember]
        public long TotalDownloads { get; set; }
        [DataMember]
        public long FacebookLikes { get; set; }
        [DataMember]
        public long TwitterTweets { get; set; }


        [DataMember]
        public long UploadedBy { get; set; }

        [ForeignKey("UploadedBy")]
        [DataMember]
        public virtual User UploadedByUser { get; set; }

        [ForeignKey("DjId")]
        [DataMember]
        public virtual DJ DjUser { get; set; }

        //Field specific to Itune Albums 
        [DataMember]
        public bool? IsMixtape { get; set; }
        [DataMember]
        public string AffiliateLink { get; set; }
        [DataMember]
        public string TwitterLink { get; set; }
        [DataMember]
        public string InstagramLink { get; set; }
        [DataMember]
        public string RecordLabel { get; set; }

        [DataMember]
        public int? ItunesAlbumTypeCode { get; set; }

        [DataMember]
        [NotMapped]
        public ItunesAlbumType? ItunesAlbumType
        {
            get { return (ItunesAlbumType?)ItunesAlbumTypeCode; }
            set { ItunesAlbumTypeCode = (int?)value; }
        }
    }
}
