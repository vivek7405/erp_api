using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class InvoiceChallanDeductionModel
    {
        [DataMember]
        public int InvoiceChallanDeductionId { get; set; }
        [DataMember]
        public int InvoiceOutStockId { get; set; }
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