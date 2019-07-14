using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ProductQuantity
    {
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public int RemainingQuantity { get; set; }
        [DataMember]
        public int RemainingQuantityPO { get; set; }

        [DataMember]
        public ProductQuantity[] AssemblyProductQnts { get; set; }        
    }
}