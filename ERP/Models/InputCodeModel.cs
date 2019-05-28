using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class InputCodeModel
    {
        [DataMember]
        public int InputCodeId { get; set; }

        [DataMember]
        public int? OutputCodeId { get; set; }

        [DataMember]
        public string InputCodeNo { get; set; }

        [DataMember]
        public string InputMaterialDesc { get; set; }

        [DataMember]
        public int InputQuantity { get; set; }

        [DataMember]
        public int SplitQuantity { get; set; }

        [DataMember]
        public int PartTypeId { get; set; }

        [DataMember]
        public string PartTypeName { get; set; }

        [DataMember]
        public string BASFChallanNo { get; set; }

        [DataMember]
        public DateTime? CreateDate { get; set; }

        [DataMember]
        public DateTime? EditDate { get; set; }        
    }
}