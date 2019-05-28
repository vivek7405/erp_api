using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class PurchaseOrderModel
    {
        [DataMember]
        public OutputCode OutputCode { get; set; }
        [DataMember]
        public List<InputCodeModel> InputCodes { get; set; }
    }
}