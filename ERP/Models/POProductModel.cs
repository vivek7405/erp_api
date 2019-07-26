using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class POProductModel
    {
        [DataMember]
        public POProduct POProduct { get; set; }
        [DataMember]
        public ProductDetail ProductDetail { get; set; }
        [DataMember]
        public PODetail PODetail { get; set; }
        [DataMember]
        public ICollection<PODeduction> PODeductions { get; set; }
        [DataMember]
        public ICollection<AccPODeduction> AccPODeductions { get; set; }
        [DataMember]
        public ICollection<AssemblyPODeduction> AssemblyPODeductions { get; set; }
        [DataMember]
        public int RemainingQuantity { get; set; }
        [DataMember]
        public bool CanDelete { get; set; }
    }
}
