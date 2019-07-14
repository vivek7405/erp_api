using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ProductDetailModel
    {
        [DataMember]
        public ProductDetail ProductDetail { get; set; }
        [DataMember]
        public ProductMapping[] ProductMappings { get; set; }
    }
}