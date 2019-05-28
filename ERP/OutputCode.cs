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
    
    public partial class OutputCode
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public OutputCode()
        {
            this.InputCodes = new HashSet<InputCode>();
        }
    
        public int OutputCodeId { get; set; }
        public string ProjectName { get; set; }
        public string OutputCodeNo { get; set; }
        public string OutputMaterialDesc { get; set; }
        public int OutputQuantity { get; set; }
        public Nullable<System.DateTime> CreateDate { get; set; }
        public Nullable<System.DateTime> EditDate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonIgnore]
        public virtual ICollection<InputCode> InputCodes { get; set; }
    }
}
