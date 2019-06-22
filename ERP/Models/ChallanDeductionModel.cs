using System;
using System.Runtime.Serialization;

namespace ERP.Models
{
    [DataContract]
    public class ChallanDeductionModel
    {
        [DataMember]
        public int ChallanDeductionId { get; set; }
        [DataMember]
        public int OutputCode { get; set; }
        [DataMember]
        public int ChallanProductId { get; set; }
        [DataMember]
        public int OutQuantity { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime EditDate { get; set; }

        [DataMember]
        public ChallanProductModel ChallanProduct { get; set; }
    }
}