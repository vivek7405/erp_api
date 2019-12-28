using System;
using System.Runtime.Serialization;

namespace ERP.Models
{
    [DataContract]
    public class VendorChallanModel
    {
        [DataMember]
        public int VendorChallanNo { get; set; }
        [DataMember]
        public string VendorChallanNumber { get; set; }
        [DataMember]
        public DateTime VendorChallanDate { get; set; }
        [DataMember]
        public bool IsNg { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime EditDate { get; set; }
        [DataMember]
        public OutStockModel[] OutStocks { get; set; }
    }
}
