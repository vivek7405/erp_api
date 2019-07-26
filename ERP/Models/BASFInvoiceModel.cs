using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFInvoiceModel
    {
        [DataMember]
        public int BASFInvoiceId { get; set; }
        [DataMember]
        public int BASFInvoiceNo { get; set; }
        [DataMember]
        public DateTime BASFInvoiceDate { get; set; }
        [DataMember]
        public bool IsNg { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime EditDate { get; set; }
        [DataMember]
        public InvoiceOutStockModel[] InvoiceOutStocks { get; set; }
    }
}
