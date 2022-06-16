using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class BooleanResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public bool data { get; set; }

        public static BooleanResponse Create(int status, string msg, bool data)
        {
            BooleanResponse obj = new BooleanResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }
}
