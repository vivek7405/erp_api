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
    
    public partial class VendorChallan
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VendorChallan()
        {
            this.OutStocks = new HashSet<OutStock>();
        }
    
        public int VendorChallanNo { get; set; }
        public Nullable<System.DateTime> VendorChallanDate { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<OutStock> OutStocks { get; set; }
    }
}
