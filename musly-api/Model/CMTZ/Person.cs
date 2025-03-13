using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.Serialization;
using musly_api.Model.CMTZ.Enums;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public class Person : PersistentEntity
    {

        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [NotMapped]
        public string Name
        {
            get { return String.Format("{0} {1}", FirstName, LastName); }
        }

        [DataMember]
        public short GenderCode { get; set; }
        [NotMapped]
        public Gender Gender
        {
            get { return (Gender)GenderCode; }
            set { GenderCode = (short)value; }
        }

        [DataMember]
        public DateTime? Birthday { get; set; }

        [DataMember]
        public long? CountryId { get; set; }

        [DataMember]
        public string Bio { get; set; }
        [DataMember]
        public string DisplayName { get; set; }

        [ForeignKey("CountryId")]
        [DataMember]
        public virtual Country Country { get; set; }

    }
}
