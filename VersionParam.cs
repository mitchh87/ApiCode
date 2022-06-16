using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DARSJsonWebService.Models.Parameters
{
    public class VersionParam
    {
        [DataMember]
        public string VersionNo { get; set; }
        [DataMember]
        public string OsType { get; set; }
    }
}