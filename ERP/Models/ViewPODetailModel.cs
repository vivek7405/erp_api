using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ViewPODetailModel
    {
        [DataMember]
        public PODetail PODetail { get; set; }
        [DataMember]
        public POProductModel[] POProducts { get; set; }
    }
}