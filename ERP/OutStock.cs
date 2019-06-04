//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ERP
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    
    public partial class OutStock
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OutStock()
        {
            this.ChallanDeductions = new HashSet<ChallanDeduction>();
        }
    
        public int OutputCode { get; set; }
        public Nullable<int> VendorChallanNo { get; set; }
        public Nullable<int> OutputQuantity { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<ChallanDeduction> ChallanDeductions { get; set; }
        [JsonIgnore]
        public virtual VendorChallan VendorChallan { get; set; }
    }
}
