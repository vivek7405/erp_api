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
    using System;
    using System.Collections.Generic;
    
    public partial class OutAcc
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OutAcc()
        {
            this.AccChallanDeductions = new HashSet<AccChallanDeduction>();
            this.AccPODeductions = new HashSet<AccPODeduction>();
        }
    
        public int OutAccId { get; set; }
        public Nullable<int> OutStockId { get; set; }
        public Nullable<int> OutputQuantity { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccChallanDeduction> AccChallanDeductions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccPODeduction> AccPODeductions { get; set; }
        public virtual OutStock OutStock { get; set; }
    }
}
