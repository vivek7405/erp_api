using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class VendorChallanNoModel
    {
        [DataMember]
        public int VendorChallanNo { get; set; }
    }
}