using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Web;

namespace ERP.Models
{
    [DataContract]
    public class SuccessResponse
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember(Name = "statusCode")]
        public HttpStatusCode StatusCode { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}