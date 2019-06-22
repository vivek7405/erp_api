using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ChallanProductModel
    {
        [DataMember]
        public ChallanProduct ChallanProduct { get; set; }
        [DataMember]
        public ProductDetail ProductDetail { get; set; }
        [DataMember]
        public ChallanDetail ChallanDetail { get; set; }
        [DataMember]
        public ICollection<ChallanDeduction> ChallanDeductions { get; set; }
        [DataMember]
        public int RemainingQuantity { get; set; }
    }
}