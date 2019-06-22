using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFChallanDeduction
    {
        [DataMember]
        public BASFChallanSelection[] BASFChallanSelections { get; set; }
    }
}