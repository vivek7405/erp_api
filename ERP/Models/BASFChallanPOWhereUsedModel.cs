using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFChallanPOWhereUsedModel
    {
        [DataMember]
        public string ChallanNo { get; set; }
        [DataMember]
        public string InputCode { get; set; }
        [DataMember]
        public string InputMaterialDesc { get; set; }
        [DataMember]
        public string InputQuantity { get; set; }
        [DataMember]
        public string RemainingQuantity { get; set; }
        [DataMember]
        public string TotalUsed { get; set; }
        [DataMember]
        public string VendorChallanNo { get; set; }
        [DataMember]
        public string VendorChallanDate { get; set; }
        [DataMember]
        public string VendorChallanOutQnt { get; set; }
        [DataMember]
        public string BASFInvoiceNo { get; set; }
        [DataMember]
        public string BASFInvoiceDate { get; set; }
        [DataMember]
        public string BASFInvoiceOutQnt { get; set; }
    }
}