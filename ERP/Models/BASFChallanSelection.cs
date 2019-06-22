using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFChallanSelection
    {
        [DataMember]
        public ChallanDetail ChallanDetail { get; set; }

        [DataMember]
        public ChallanProduct ChallanProduct { get; set; }

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