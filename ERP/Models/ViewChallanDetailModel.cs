using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ViewChallanDetailModel
    {
        [DataMember]
        public ChallanDetail ChallanDetail { get; set; }
        [DataMember]
        public ChallanProductModel[] ChallanProducts { get; set; }
    }
}