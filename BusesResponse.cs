using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DARSJsonWebService.Models.Responses
{
    [DataContract]
    public class BusesResponse
    {
        [DataMember]
        public int status { get; set; }
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public List<Buses> data { get; set; }

        public static BusesResponse Create(int status, string msg, List<Buses> data)
        {
            BusesResponse obj = new BusesResponse();
            obj.status = status;
            obj.msg = msg;
            obj.data = data;
            return obj;
        }
    }

    [DataContract]
    public class Buses
    {
        [DataMember]
        string _busNum;
        [DataMember]
        string _busLink;

        public Buses(string busNum, string busLink)
        {
            this._busNum = busNum;
            this._busLink = busLink;
        }
    }
}