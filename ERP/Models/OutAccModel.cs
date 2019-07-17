using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class OutAccModel
    {
        [DataMember]
        public int OutAccId { get; set; }
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
        public AccChallanDeductionModel[] AccChallanDeductions { get; set; }
        [DataMember]
        public AccPODeductionModel[] AccPODeductions { get; set; }
    }
}