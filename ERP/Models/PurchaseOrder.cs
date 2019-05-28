using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class PurchaseOrder
    {
        [DataMember]
        public OutputCode OutputCode { get; set; }
        [DataMember]
        public InputCode[] InputCodes { get; set; }
    }
}