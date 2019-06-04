using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class VendorChallanModel
    {
        public int VendorChallanNo { get; set; }
        public DateTime VendorChallanDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime EditDate { get; set; }
        public List<OutStockModel> OutStocks { get; set; }
    }
}
