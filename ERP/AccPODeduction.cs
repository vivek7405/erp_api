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
    
    public partial class AccPODeduction
    {
        public int AccPODeductionId { get; set; }
        public Nullable<int> OutAccId { get; set; }
        public Nullable<int> POProductId { get; set; }
        public Nullable<int> OutQuantity { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        public virtual OutAcc OutAcc { get; set; }
        public virtual POProduct POProduct { get; set; }
    }
}
