using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ChallanDetailMod
    {
        [DataMember]
        public int ChallanId { get; set; }
        [DataMember]
        public string ChallanNo { get; set; }
        [DataMember]
        public DateTime? ChallanDate { get; set; }
        [DataMember]
        public DateTime? CreateDate { get; set; }
        [DataMember]
        public DateTime? EditDate { get; set; }
    }
}