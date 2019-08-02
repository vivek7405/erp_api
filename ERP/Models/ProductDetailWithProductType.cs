using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ProductDetailWithProductType
    {
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public int? ProductTypeId { get; set; }
        [DataMember]
        public string ProductTypeName { get; set; }
        [DataMember]
        public string InputCode { get; set; }
        [DataMember]
        public string InputMaterialDesc { get; set; }
        [DataMember]
        public string OutputCode { get; set; }
        [DataMember]
        public string OutputMaterialDesc { get; set; }
        [DataMember]
        public string ProjectName { get; set; }
        [DataMember]
        public int? SplitRatio { get; set; }
        [DataMember]
        public DateTime? CreateDate { get; set; }
        [DataMember]
        public DateTime? EditDate { get; set; }
    }
}