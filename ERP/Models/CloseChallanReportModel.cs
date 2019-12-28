using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class CloseChallanReportModel
    {
        [DataMember]
        public int ChallanId { get; set; }
        [DataMember]
        public string ChallanNo { get; set; }
        [DataMember]
        public string ChallanDate { get; set; }
        [DataMember]
        public string InputCode { get; set; }
        [DataMember]
        public string InputQuantity { get; set; }
        [DataMember]
        public string OutputQuantity { get; set; }
        [DataMember]
        public string VendorChallanNo { get; set; }
        [DataMember]
        public string VendorChallanDate { get; set; }
        [DataMember]
        public string RemainingQuantity { get; set; }
    }
}