using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class VendorChallanGridModel
    {
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public string OutputCode { get; set; }
        [DataMember]
        public string OutputMaterialDesc { get; set; }
        [DataMember]
        public string OutputQuantity { get; set; }
        [DataMember]
        public string InputCode { get; set; }
        [DataMember]
        public string InputMaterialDesc { get; set; }
        [DataMember]
        public string InputQuantity { get; set; }
        [DataMember]
        public string PartType { get; set; }
        [DataMember]
        public string BASFChallanNo { get; set; }
        [DataMember]
        public string Balance { get; set; }
        [DataMember]
        public string BASFPONumber { get; set; }
        [DataMember]
        public int RowSpan { get; set; }
    }
}
