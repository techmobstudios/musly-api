using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace musly_api.Model.CMTZ
{
    [DataContract]
    public abstract class PersistentEntity
    {
        [Key]
        [ScaffoldColumn(false)]
        [DataMember]
        public long Id { get; set; }
        [ScaffoldColumn(false)]
        [DataMember]
        public string RefId { get; set; }
        [ScaffoldColumn(false)]
        [DataMember]
        public short? StateCode { get; set; }
        [ScaffoldColumn(false)]
        [DataMember]
        public DateTime CreatedOn { get; set; }
        [ScaffoldColumn(false)]
        [DataMember]
        public DateTime? ModifiedOn { get; set; }

        protected PersistentEntity()
        {
            RefId = Guid.NewGuid().ToString().Split('-')[0];
            StateCode = 1;
            CreatedOn = DateTime.Now;
        }
    }
}
