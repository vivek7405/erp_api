using System;
using System.Runtime.Serialization;

namespace ERP.Models
{
    [DataContract]
    public class OutStockModel
    {
        [DataMember]
        public int OutStockId { get; set; }
        [DataMember]
        public int VendorChallanNo { get; set; }
        [DataMember]
        public int OutputQuantity { get; set; }
        [DataMember]
        public DateTime CreateDate { get; set; }
        [DataMember]
        public DateTime EditDate { get; set; }
        [DataMember]
        public int ProductId { get; set; }
        [DataMember]
        public int SplitRatio { get; set; }
        [DataMember]
        public ChallanDeductionModel[] ChallanDeductions { get; set; }
        [DataMember]
        public PODeductionModel[] PODeductions { get; set; }
        [DataMember]
        public OutAccModel[] OutAccs { get; set; }
        [DataMember]
        public OutAssemblyModel[] OutAssemblys { get; set; }
    }
}