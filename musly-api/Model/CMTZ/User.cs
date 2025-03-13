using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using musly_api.Model.CMTZ.Enums;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class User : PersistentEntity
    {
        [DataMember]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string Username { get; set; }

        [DataMember]
        public int UserRoleTypeCode { get; set; }

        [DataMember]
        [NotMapped]
        public UserRoleType UserRoleType
        {
            get { return (UserRoleType)UserRoleTypeCode; }
            set { UserRoleTypeCode = (short)value; }
        }

        [DataMember]
        public bool IsApproved { get; set; }
        [DataMember]
        public bool IsLocked { get; set; }

        [DataMember]
        public DateTime LastLoginDate { get; set; }
        [DataMember]
        public DateTime LastActivityDate { get; set; }
        [DataMember]
        public DateTime LastPasswordChangedDate { get; set; }

        [DataMember]
        public int UserAccountTypeCode { get; set; }

        [DataMember]
        [NotMapped]
        public UserAccountType UserAccountType
        {
            get { return (UserAccountType)UserAccountTypeCode; }
            set { UserAccountTypeCode = (short)value; }
        }

        [DataMember]
        public long PersonId { get; set; }

        [DataMember]
        public string IpAddress { get; set; }

        [DataMember]
        public string deviceIdentifier { get; set; }

        [DataMember]
        public string hemaEmail { get; set; }

        [DataMember]
        public string OS { get; set; }

        [DataMember]
        public string FilePath { get; set; }

        [ForeignKey("PersonId")]
        [DataMember]
        public virtual Person Person { get; set; }

        public User()
        {
            UserAccountType = UserAccountType.Normal;
        }

        /*public static implicit operator User(ApplicationUser v)
        {
            throw new NotImplementedException();
        }*/
    }
}
