using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace DARSJsonWebService.Models.Parameters
{
    [DataContract]
    public class TokenParam
    {
        [DataMember]
        public string token { get; set; }
        [DataMember]
        public string collegeId { get; set; }
        [DataMember]
        public string columnNum { get; set; }
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public int classId { get; set; }
        [DataMember]
        public int subjectId { get; set; }
        [DataMember]
        public int sectionId { get; set; }
        [DataMember]
        public int courseResourceId { get; set; }
        [DataMember]
        public int studentId { get; set; }
        [DataMember]
        public int schoolYear { get; set; }
        [DataMember]
        public int showUnread { get; set; }
        [DataMember]
        public int threadId { get; set; }
        [DataMember]
        public int isDone { get; set; }
        [DataMember]
        public string dayDate { get; set; }
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string uType { get; set; }
        [DataMember]
        public string mailBody { get; set; }
        [DataMember]
        public string mailBody_NoHtml { get; set; }
        [DataMember]
        public string pushID { get; set; }
        [DataMember]
        public string MobVersion { get; set; }
        [DataMember]
        public string mobileType { get; set; }
        [DataMember]
        public string osVersion { get; set; }
        [DataMember]
        public int creator_Id { get; set; }
        [DataMember]
        public string PageSize { get; set; }
        [DataMember]
        public string PageNumber { get; set; }
        [DataMember]
        public int Id_MailType { get; set; }
        
    }
}