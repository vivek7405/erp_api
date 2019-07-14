using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFPOSelection
    {
        [DataMember]
        public PODetail PODetail { get; set; }

        [DataMember]
        public POProduct POProduct { get; set; }

        [DataMember]
        public int? InputQuantity { get; set; }

        [DataMember]
        public int? OutputQuantity { get; set; }

        [DataMember]
        public int RemainingQuantity { get; set; }

        [DataMember]
        public bool IsChecked { get; set; }


        [DataMember]
        public int OutQuantity { get; set; }
        [DataMember]
        public int QntAfterDeduction { get; set; }
    }
}