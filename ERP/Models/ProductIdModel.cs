﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class ProductIdModel
    {
        [DataMember]
        public int ProductId { get; set; }
    }
}