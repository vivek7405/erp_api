using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class AssemblyChallanDeductionModel
    {
        [DataMember]
        public int AssemblyChallanDeductionId { get; set; }
        [DataMember]
        public int OutAssemblyId { get; set; }
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