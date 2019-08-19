using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class OutAssemblyModel
    {
        [DataMember]
        public int OutAssemblyId { get; set; }
        [DataMember]
        public int OutStockId { get; set; }
        [DataMember]
        public int OutputQuantity { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime EditDate { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public int SplitRatio { get; set; }
        [DataMember]
        public AssemblyChallanDeductionModel[] AssemblyChallanDeductions { get; set; }
        [DataMember]
        public AssemblyPODeductionModel[] AssemblyPODeductions { get; set; }
        public int AssemblyQntSum { get; set; }
    }
}