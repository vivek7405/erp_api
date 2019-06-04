using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class OutStockModel
    {
        public int OutputCode { get; set; }
        public int VendorChallanNo { get; set; }
        public int OutputQuantity { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime EditDate { get; set; }
        public List<ChallanDeduction> ChallanDeductions { get; set; }
    }
}