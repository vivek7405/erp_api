using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class BASFPODeduction
    {
        [DataMember]
        public BASFPOSelection[] BASFPOSelections { get; set; }
    }
}