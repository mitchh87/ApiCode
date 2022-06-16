using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace DARSJsonWebService.Models.Parameters
{
    [DataContract]
    public class PhoneRegister
    {
        [DataMember]
        public string userId { get; set; }
        [DataMember]
        public string sessionId { get; set; }
        [DataMember]
        public string pushID { get; set; }
        [DataMember]
        public string phoneNumber { get; set; }
        [DataMember]
        public string personType { get; set; }
    }
}