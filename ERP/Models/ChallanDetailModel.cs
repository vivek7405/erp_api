using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ChallanDetailModel
    {
        [DataMember]
        public ChallanDetail ChallanDetail { get; set; }

        [DataMember]
        public ChallanProduct[] ChallanProducts { get; set; }
    }
}