using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class InvoiceOutStockModel
    {
        [DataMember]
        public int InvoiceOutStockId { get; set; }
        [DataMember]
        public int BASFInvoiceId { get; set; }
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
        public InvoiceChallanDeductionModel[] InvoiceChallanDeductions { get; set; }        
    }
}