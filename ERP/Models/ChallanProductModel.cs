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
        public ProductDetailWithProductType ProductDetail { get; set; }
        [DataMember]
        public ChallanDetail ChallanDetail { get; set; }
        [DataMember]
        public ICollection<ChallanDeduction> ChallanDeductions { get; set; }
        [DataMember]
        public ICollection<AccChallanDeduction> AccChallanDeductions { get; set; }
        [DataMember]
        public ICollection<AssemblyChallanDeduction> AssemblyChallanDeductions { get; set; }
        [DataMember]
        public int RemainingQuantity { get; set; }
        [DataMember]
        public bool CanDelete { get; set; }
    }
}